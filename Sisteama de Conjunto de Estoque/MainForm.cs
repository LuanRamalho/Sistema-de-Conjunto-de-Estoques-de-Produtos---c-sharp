using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SistemaEstoque
{
    public class MainForm : Form
    {
        private TextBox txtNomeLoja;
        private DataGridView dgvLojas;
        private string nomeLojaOriginal = "";

        public MainForm()
        {
            ConfigurarInterface();
            CarregarLojas();
        }

        private void ConfigurarInterface()
        {
            this.Text = "Gerenciador de Comércio - Lojas (JSON)";
            this.Size = new Size(600, 500);
            this.BackColor = Color.FromArgb(44, 62, 80);
            this.StartPosition = FormStartPosition.CenterScreen;

            Panel painelTopo = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(52, 73, 94) };
            
            Label lblNome = new Label { Text = "Nome da Loja:", ForeColor = Color.White, Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            txtNomeLoja = new TextBox { Location = new Point(20, 45), Width = 250, Font = new Font("Segoe UI", 10) };

            Button btnAdicionar = CriarBotao("Adiciona", Color.FromArgb(39, 174, 96), new Point(290, 40));
            btnAdicionar.Click += (s, e) => AdicionarLoja();

            Button btnAtualizar = CriarBotao("Atualiza", Color.FromArgb(41, 128, 185), new Point(380, 40));
            btnAtualizar.Click += (s, e) => AtualizarLoja();

            Button btnDeletar = CriarBotao("Deletar", Color.FromArgb(192, 57, 43), new Point(470, 40));
            btnDeletar.Click += (s, e) => DeletarLoja();

            painelTopo.Controls.AddRange(new Control[] { lblNome, txtNomeLoja, btnAdicionar, btnAtualizar, btnDeletar });

            dgvLojas = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.FromArgb(236, 240, 241),
                BorderStyle = BorderStyle.None
            };

            dgvLojas.DefaultCellStyle.SelectionBackColor = Color.LimeGreen;
            dgvLojas.DefaultCellStyle.SelectionForeColor = Color.Black;

            dgvLojas.CellContentClick += DgvLojas_CellContentClick;
            dgvLojas.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    nomeLojaOriginal = dgvLojas.Rows[e.RowIndex].Cells["Nome"].Value.ToString();
                    txtNomeLoja.Text = nomeLojaOriginal;
                }
            };

            this.Controls.Add(dgvLojas);
            this.Controls.Add(painelTopo);
        }

        private Button CriarBotao(string texto, Color cor, Point local)
        {
            return new Button { Text = texto, BackColor = cor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = local, Size = new Size(80, 35), Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
        }

        private void CarregarLojas()
        {
            var lojas = DatabaseHelper.CarregarDados();
            dgvLojas.DataSource = null;
            dgvLojas.Columns.Clear();
            dgvLojas.DataSource = lojas.Select(l => new { l.Nome }).ToList();

            DataGridViewLinkColumn linkCol = new DataGridViewLinkColumn
            {
                DataPropertyName = "Nome",
                Name = "Nome",
                HeaderText = "NOME DA LOJA (Clique para abrir)",
                LinkColor = Color.Blue,
                TrackVisitedState = false
            };
            dgvLojas.Columns.RemoveAt(0);
            dgvLojas.Columns.Add(linkCol);

            nomeLojaOriginal = "";
            txtNomeLoja.Clear();
        }

        private void DgvLojas_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvLojas.Columns[e.ColumnIndex] is DataGridViewLinkColumn)
            {
                string nome = dgvLojas.Rows[e.RowIndex].Cells["Nome"].Value.ToString();
                StoreForm storeForm = new StoreForm(nome);
                storeForm.ShowDialog();
                CarregarLojas();
            }
        }

        private void AdicionarLoja()
        {
            if (string.IsNullOrWhiteSpace(txtNomeLoja.Text)) return;
            var lojas = DatabaseHelper.CarregarDados();
            if (lojas.Any(l => l.Nome == txtNomeLoja.Text)) return;

            lojas.Add(new Loja { Nome = txtNomeLoja.Text });
            DatabaseHelper.SalvarDados(lojas);
            CarregarLojas();
        }

        private void AtualizarLoja()
        {
            if (string.IsNullOrEmpty(nomeLojaOriginal) || string.IsNullOrWhiteSpace(txtNomeLoja.Text)) return;
            var lojas = DatabaseHelper.CarregarDados();
            var loja = lojas.FirstOrDefault(l => l.Nome == nomeLojaOriginal);
            if (loja != null)
            {
                loja.Nome = txtNomeLoja.Text;
                DatabaseHelper.SalvarDados(lojas);
                CarregarLojas();
            }
        }

        private void DeletarLoja()
        {
            if (string.IsNullOrEmpty(nomeLojaOriginal)) return;
            if (MessageBox.Show("Excluir loja e estoque?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var lojas = DatabaseHelper.CarregarDados();
                lojas.RemoveAll(l => l.Nome == nomeLojaOriginal);
                DatabaseHelper.SalvarDados(lojas);
                CarregarLojas();
            }
        }
    }
}
