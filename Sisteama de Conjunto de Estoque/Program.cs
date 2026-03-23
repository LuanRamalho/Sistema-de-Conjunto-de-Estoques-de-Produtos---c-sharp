using System;
using System.Windows.Forms;

namespace SistemaEstoque
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Inicializa o banco de dados antes da interface iniciar
            DatabaseHelper.InitializeDatabase();
            
            Application.Run(new MainForm());
        }
    }
}