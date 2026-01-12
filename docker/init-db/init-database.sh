#!/bin/bash
# Script para inicializar o banco de dados após o SQL Server estar pronto
# Este script deve ser executado manualmente após o container estar rodando

echo "Aguardando SQL Server estar pronto..."
sleep 30

echo "Executando script de inicialização do banco de dados..."

/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "${SA_PASSWORD:-YourStrong@Password123}" -i /scripts/01-create-user.sql

echo "Inicialização do banco de dados concluída!"
