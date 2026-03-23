using System;
using Microsoft.Data.Sqlite;
using System.IO;

namespace SistemaEstoque
{
    public static class DatabaseHelper
    {
        private const string DbName = "estoque_comercio.db";
        public static string ConnectionString => $"Data Source={DbName}";

        public static void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Lojas (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nome TEXT NOT NULL
                    );
                    CREATE TABLE IF NOT EXISTS Produtos (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        LojaId INTEGER NOT NULL,
                        Nome TEXT NOT NULL,
                        Quantidade INTEGER NOT NULL,
                        Fornecedor TEXT NOT NULL,
                        Preco REAL NOT NULL,
                        FOREIGN KEY(LojaId) REFERENCES Lojas(Id) ON DELETE CASCADE
                    );
                ";
                command.ExecuteNonQuery();
            }
        }
    }
}