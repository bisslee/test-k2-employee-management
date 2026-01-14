using Biss.EmployeeManagement.Api.Helper;
using Biss.EmployeeManagement.CrossCutting.DependencyInjection;
using Biss.EmployeeManagement.Domain.Constants;
using Biss.EmployeeManagement.Infrastructure;
using Biss.EmployeeManagement.Infrastructure.Serialization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System;
using System.Globalization;
using System.Reflection;

namespace Biss.EmployeeManagement.Api.Extensions
{
    public static class ConfigureServicesExtension
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connStr = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connStr));

            // Adicionar compressão de resposta
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });


            services.AddAutoMapper();
            services.AddMediator();
            services.AddRepository();
            services.AddValidators();
            services.AddApplicationServices();
            services.ConfigureLogging(configuration);
            services.AddHealthChecksInjection();

            // Configurar segurança
            services.ConfigureSecurity(configuration);

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] { "en-US", "pt-BR", "es" };
                options.DefaultRequestCulture = new RequestCulture("pt-BR");
                options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
                options.SupportedUICultures = options.SupportedCultures;
            });

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    var jsonOptions = JsonSerializationConfig.CreateApiOptions();
                    options.JsonSerializerOptions.PropertyNamingPolicy = jsonOptions.PropertyNamingPolicy;
                    options.JsonSerializerOptions.WriteIndented = jsonOptions.WriteIndented;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = jsonOptions.DefaultIgnoreCondition;
                    options.JsonSerializerOptions.ReferenceHandler = jsonOptions.ReferenceHandler; // Importante: evitar referências circulares
                    foreach (var converter in jsonOptions.Converters)
                    {
                        options.JsonSerializerOptions.Converters.Add(converter);
                    }
                });

            services.AddEndpointsApiExplorer();
            services.ConfigureSwagger();

            // Configuração robusta do CORS
            services.ConfigureCors(configuration);

            return services;
        }

        private static void ConfigureSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurar SecuritySettings
            services.Configure<SecuritySettings>(configuration.GetSection("Security"));

            // Configurar JWT Authentication
            var jwtSettings = configuration.GetSection("Security:JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured in appsettings.json");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(secretKey)
                    ),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Configurar HTTPS redirection
            var securitySettings = configuration.GetSection("Security").Get<SecuritySettings>();
            if (securitySettings?.HttpsRedirection.EnableHttpsRedirection == true)
            {
                services.AddHttpsRedirection(options =>
                {
                    options.HttpsPort = securitySettings.HttpsRedirection.HttpsPort;
                });
            }
        }

        private static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Biss Employee Management API",
                    Version = "v1.0.0",
                    Description = @"Biss Employee Management API - Sistema de Gerenciamento de Funcionários
                        - **Desenvolvimento**: http://localhost:8080
                        - **Swagger**: http://localhost:8080/swagger
                    ",
                    Contact = new OpenApiContact
                    {
                        Name = "Biss Development Team",
                        Email = "contato@biss.com.br",
                        Url = new Uri("https://biss.com.br")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });

                // Configurar autenticação JWT (preparado para futuras implementações)
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // Incluir comentários XML se existirem
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Configurar filtros
                c.OperationFilter<AddAcceptLanguageHeaderOperationFilter>();
                c.EnableAnnotations();

                // Configurar esquemas personalizados
                c.CustomSchemaIds(type => type.Name);

                // Configurar respostas padrão
                c.MapType<DateTime>(() => new OpenApiSchema { Type = "string", Format = "date-time" });
                c.MapType<DateTime?>(() => new OpenApiSchema { Type = "string", Format = "date-time", Nullable = true });
                c.MapType<Guid>(() => new OpenApiSchema { Type = "string", Format = "uuid" });
                c.MapType<Guid?>(() => new OpenApiSchema { Type = "string", Format = "uuid", Nullable = true });

                // Configurar tags para organizar endpoints
                c.TagActionsBy(api =>
                {
                    if (api.GroupName != null)
                    {
                        return new[] { api.GroupName.ToString() };
                    }

                    var controllerActionDescriptor = api.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
                    if (controllerActionDescriptor != null)
                    {
                        return new[] { controllerActionDescriptor.ControllerName };
                    }

                    throw new InvalidOperationException("Unable to determine tag for endpoint.");
                });

                c.DocInclusionPredicate((name, api) => true);
            });
        }

        private static void ConfigureCors(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(ServiceConstants.CorsPolicy, builder =>
                {
                    var allowedExactOrigins = configuration
                        .GetSection("CorsSettings:AllowedExactOrigins")
                        .Get<string[]>() ?? Array.Empty<string>();

                    var allowedDomainsWithSubdomains = configuration
                        .GetSection("CorsSettings:AllowedDomainsWithSubdomains")
                        .Get<string[]>() ?? Array.Empty<string>();

                    builder.SetIsOriginAllowed(origin =>
                    {
                        if (allowedExactOrigins.Contains(origin))
                        {
                            return true;
                        }

                        if (Uri.CheckHostName(origin) != UriHostNameType.Unknown)
                        {
                            var host = new Uri(origin).Host;

                            return allowedDomainsWithSubdomains.Any(domain =>
                                host.Equals(domain, StringComparison.OrdinalIgnoreCase) ||
                                host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase)
                            );
                        }

                        return false;
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders(
                        "X-Total-Count",
                        "X-Pagination",
                        "X-Correlation-Id",
                        "X-Trace-Id"
                    );
                });

                options.AddPolicy("PublicPolicy", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .WithMethods("GET")
                        .AllowAnyHeader();
                });
            });
        }
    }
}
