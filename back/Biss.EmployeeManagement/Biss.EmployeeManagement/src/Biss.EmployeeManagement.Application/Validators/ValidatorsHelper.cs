using Biss.EmployeeManagement.Application.Helpers;
using FluentValidation;
using System;

namespace Biss.EmployeeManagement.Application.Validators
{
    public static class ValidatorsHelper
    {
        public static bool IsValidCnpj(string cnpj)
        {
            if (string.IsNullOrEmpty(cnpj))
                return false;

            cnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");

            if (cnpj.Length != 14)
                return false;

            // Verificar se todos os dígitos são iguais (ex: 00000000000000)
            if (new string(cnpj[0], cnpj.Length) == cnpj)
                return false;

            int[] multiplier1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplier2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCnpj = cnpj.Substring(0, 12);
            int sum = 0;

            for (int i = 0; i < 12; i++)
                sum += int.Parse(tempCnpj[i].ToString()) * multiplier1[i];

            int rest = sum % 11;
            rest = rest < 2 ? 0 : 11 - rest;

            string digit = rest.ToString();
            tempCnpj += digit;

            sum = 0;

            for (int i = 0; i < 13; i++)
                sum += int.Parse(tempCnpj[i].ToString()) * multiplier2[i];

            rest = sum % 11;
            rest = rest < 2 ? 0 : 11 - rest;

            digit += rest.ToString();

            return cnpj.EndsWith(digit);
        }

        public static bool IsValidCpf(string cpf)
        {
            if (string.IsNullOrEmpty(cpf))
                return false;

            cpf = cpf.Replace(".", "").Replace("-", "");

            if (cpf.Length != 11)
                return false;

            // Verificar se todos os dígitos são iguais (ex: 00000000000)
            if (new string(cpf[0], cpf.Length) == cpf)
                return false;

            int[] multiplier1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplier2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int sum = 0;

            for (int i = 0; i < 9; i++)
                sum += int.Parse(tempCpf[i].ToString()) * multiplier1[i];

            int rest = sum % 11;
            rest = rest < 2 ? 0 : 11 - rest;

            string digit = rest.ToString();
            tempCpf += digit;

            sum = 0;

            for (int i = 0; i < 10; i++)
                sum += int.Parse(tempCpf[i].ToString()) * multiplier2[i];

            rest = sum % 11;
            rest = rest < 2 ? 0 : 11 - rest;

            digit += rest.ToString();

            return cpf.EndsWith(digit);
        }

        public static bool NotInFuture(DateTime date)
        {
            return date <= DateTime.Now;
        }

        public static bool IsEmailValid(string email)
        {
            return new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email);
        }

        public static bool IsDocumentValid(string document)
        {
            if (string.IsNullOrEmpty(document))
                return false;

            document = document.Replace(".", "").Replace("-", "");

            switch (document.Length)
            {
                case 11:
                    return IsValidCpf(document);
                case 14:
                    return IsValidCnpj(document);
                default:
                    return false;
            }
        }

        public static bool IsPhoneValid(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return false;

            phone = phone.Replace("-", "").Replace(" ", "").Replace(".", "");
            return phone.Length == 11 || phone.Length == 10;
        }

        public static bool IsZipCodeValid(string zipCode)
        {
            if (string.IsNullOrEmpty(zipCode))
                return false;

            zipCode = zipCode.Replace(".", "").Replace("-", "");

            if (zipCode.Length != 8)
                return false;

            return true;
        }

        public static IRuleBuilderOptions<T, string> ValidateEmail<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty()
                .WithMessage(_ => ResourceHelper.GetResource(
                    "REQUIRED_EMAIL", "E-mail é obrigatório!"))
                .EmailAddress()
                .WithMessage(_ => ResourceHelper.GetResource(
                    "INVALID_EMAIL", "E-mail inválido"));
        }

        public static IRuleBuilderOptions<T, string> ValidatePassword<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty()
                .WithMessage(_ => ResourceHelper.GetResource(
                    "REQUIRED_PASSWORD", "Senha é obrigatória!"))
                .MinimumLength(8)
                .WithMessage(_ => ResourceHelper.GetResource(
                    "MIN_LENGTH_PASSWORD", "Senha deve ter no mínimo 8 caracteres"))
                .MaximumLength(255)
                .WithMessage(_ => ResourceHelper.GetResource("MAX_LENGTH_PASSWORD", "Password must not exceed 255 characters"));
        }

        public static IRuleBuilderOptions<T, string> ValidatePhoneRequired<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty()
                .WithMessage(_ => ResourceHelper.GetResource(
                    "REQUIRED_PHONE", "Telefone é obrigatório!"))
                .Must(IsPhoneValid)
                .WithMessage(_ => ResourceHelper.GetResource(
                    "INVALID_PHONE", "Telefone inválido"));
        }

        public static IRuleBuilderOptions<T, string?> ValidatePhoneOptional<T>(
            this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .Must(phone => string.IsNullOrWhiteSpace(phone) || IsPhoneValid(phone))
                .WithMessage(_ => ResourceHelper.GetResource(
                    "INVALID_PHONE", "Telefone inválido"));
        }

        public static IRuleBuilderOptions<T, string> ValidateName<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty()
                .WithMessage(_ => ResourceHelper.GetResource(
                    "REQUIRED_NAME", "Nome é obrigatório!"))
                .MinimumLength(3)
                .WithMessage(_ => ResourceHelper.GetResource(
                    "MIN_LENGTH_NAME", "Nome deve ter no mínimo 3 caracteres"));
        }

        public static IRuleBuilderOptions<T, string> ValidateDocument<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty()
                .WithMessage(_ => ResourceHelper.GetResource(
                    "REQUIRED_DOCUMENT", "Documento é obrigatório!"))
                .Must(IsDocumentValid)
                .WithMessage(_ => ResourceHelper.GetResource(
                    "INVALID_DOCUMENT", "Documento inválido"));
        }

        public static IRuleBuilderOptions<T, Guid> ValidateId<T>(
            this IRuleBuilder<T, Guid> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty()
                .WithMessage(_ => ResourceHelper.GetResource(
                    "REQUIRED_ID", "Id é obrigatório!"));
        }

        public static IRuleBuilderOptions<T, Guid?> ValidateOptionalId<T>(
            this IRuleBuilder<T, Guid?> ruleBuilder)
        {
            return ruleBuilder
                .Must(id => !id.HasValue || id.Value != Guid.Empty)
                .WithMessage(_ => ResourceHelper.GetResource(
                    "INVALID_ID", "Id deve ser um GUID válido quando fornecido!"));
        }
    }
}
