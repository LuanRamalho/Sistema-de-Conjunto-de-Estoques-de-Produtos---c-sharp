using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Encodings.Web; 
using System.Text.Unicode;

namespace SistemaEstoque
{
    // Modelos de Dados (Sem IDs)
    public class Produto
    {
        public string Nome { get; set; }
        public int Quantidade { get; set; }
        public string Fornecedor { get; set; }
        public double Preco { get; set; }
    }

    public class Loja
    {
        public string Nome { get; set; }
        public List<Produto> Produtos { get; set; } = new List<Produto>();
    }

    public static class DatabaseHelper
    {
        private const string FileName = "estoque.json";

        public static List<Loja> CarregarDados()
        {
            if (!File.Exists(FileName)) return new List<Loja>();
            
            string json = File.ReadAllText(FileName);
            return JsonSerializer.Deserialize<List<Loja>>(json) ?? new List<Loja>();
        }

        public static void SalvarDados(List<Loja> lojas)
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                // Esta linha permite que caracteres acentuados (Latin1) sejam salvos normalmente
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) 
            };
            
            string json = JsonSerializer.Serialize(lojas, options);
            File.WriteAllText(FileName, json);
        }

        public static void InitializeDatabase()
        {
            if (!File.Exists(FileName))
            {
                SalvarDados(new List<Loja>());
            }
        }
    }
}
