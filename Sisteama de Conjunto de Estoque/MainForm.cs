using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace SistemaEstoque
{
    public class MainForm : Form
    {
        private TextBox txtNomeLoja;
        private DataGridView dgvLojas;
        private int lojaSelecionadaId = 0;

        public MainForm()
        {
            ConfigurarInterface();
            CarregarLojas();
        }

        private void ConfigurarInterface()
        {
            // Configurações da Janela
            this.Text = "Gerenciador de Comércio - Lojas";
            this.Size = new Size(600, 500);
            this.BackColor = Color.FromArgb(44, 62, 80); 
            this.StartPosition = FormStartPosition.CenterScreen;

            // Painel Superior
            Panel painelTopo = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(52, 73, 94) };
            
            Label lblNome = new Label { Text = "Nome da Loja:", ForeColor = Color.White, Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            txtNomeLoja = new TextBox { Location = new Point(20, 45), Width = 250, Font = new Font("Segoe UI", 10) };

            // Botões
            Button btnAdicionar = CriarBotao("Adiciona", Color.FromArgb(39, 174, 96), new Point(290, 40));
            btnAdicionar.Click += (s, e) => AdicionarLoja();

            Button btnAtualizar = CriarBotao("Atualiza", Color.FromArgb(41, 128, 185), new Point(380, 40));
            btnAtualizar.Click += (s, e) => AtualizarLoja();

            Button btnDeletar = CriarBotao("Deletar", Color.FromArgb(192, 57, 43), new Point(470, 40));
            btnDeletar.Click += (s, e) => DeletarLoja();

            painelTopo.Controls.AddRange(new Control[] { lblNome, txtNomeLoja, btnAdicionar, btnAtualizar, btnDeletar });

            // Configuração da Tabela (DataGridView)
            dgvLojas = new DataGridView();
            dgvLojas.Dock = DockStyle.Fill;
            dgvLojas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLojas.AllowUserToAddRows = false;
            dgvLojas.ReadOnly = true;
            dgvLojas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLojas.BackgroundColor = Color.FromArgb(236, 240, 241);
            dgvLojas.BorderStyle = BorderStyle.None;

            // Determina a cor de fundo da célula clicada pelo usuário
            dgvLojas.DefaultCellStyle.SelectionBackColor = Color.LimeGreen;
            dgvLojas.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Eventos da Tabela
            dgvLojas.CellContentClick += DgvLojas_CellContentClick;
            dgvLojas.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    lojaSelecionadaId = Convert.ToInt32(dgvLojas.Rows[e.RowIndex].Cells["Id"].Value);
                    txtNomeLoja.Text = dgvLojas.Rows[e.RowIndex].Cells["Nome"].Value.ToString();
                }
            };

            this.Controls.Add(dgvLojas);
            this.Controls.Add(painelTopo);
        }

        private Button CriarBotao(string texto, Color cor, Point local)
        {
            return new Button { 
                Text = texto, 
                BackColor = cor, 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, 
                Location = local, 
                Size = new Size(80, 35), 
                Font = new Font("Segoe UI", 9, FontStyle.Bold), 
                Cursor = Cursors.Hand 
            };
        }

        private void CarregarLojas()
        {
            using (var conn = new SqliteConnection(DatabaseHelper.ConnectionString))
            {
                conn.Open();
                var cmd = new SqliteCommand("SELECT Id, Nome FROM Lojas", conn);
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                dgvLojas.DataSource = null;
                dgvLojas.Columns.Clear();
                dgvLojas.DataSource = dt;
                dgvLojas.Columns["Id"].Visible = false;

                // Coluna de Link para abrir o estoque
                DataGridViewLinkColumn linkCol = new DataGridViewLinkColumn
                {
                    DataPropertyName = "Nome",
                    Name = "Nome",
                    HeaderText = "NOME DA LOJA (Clique para abrir o estoque)",
                    LinkColor = Color.Blue,
                    ActiveLinkColor = Color.Red,
                    TrackVisitedState = false
                };
                dgvLojas.Columns.RemoveAt(1); 
                dgvLojas.Columns.Add(linkCol);
            }
            lojaSelecionadaId = 0;
            txtNomeLoja.Clear();
        }

        private void DgvLojas_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvLojas.Columns[e.ColumnIndex] is DataGridViewLinkColumn)
            {
                int id = Convert.ToInt32(dgvLojas.Rows[e.RowIndex].Cells["Id"].Value);
                string nome = dgvLojas.Rows[e.RowIndex].Cells["Nome"].Value.ToString();
                
                StoreForm storeForm = new StoreForm(id, nome);
                storeForm.ShowDialog(); 
                CarregarLojas(); // Atualiza a lista ao voltar
            }
        }

        private void AdicionarLoja()
        {
            if (string.IsNullOrWhiteSpace(txtNomeLoja.Text)) return;
            using (var conn = new SqliteConnection(DatabaseHelper.ConnectionString))
            {
                conn.Open();
                var cmd = new SqliteCommand("INSERT INTO Lojas (Nome) VALUES (@nome)", conn);
                cmd.Parameters.AddWithValue("@nome", txtNomeLoja.Text);
                cmd.ExecuteNonQuery();
            }
            CarregarLojas();
        }

        private void AtualizarLoja()
        {
            if (lojaSelecionadaId == 0 || string.IsNullOrWhiteSpace(txtNomeLoja.Text)) return;
            using (var conn = new SqliteConnection(DatabaseHelper.ConnectionString))
            {
                conn.Open();
                var cmd = new SqliteCommand("UPDATE Lojas SET Nome = @nome WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@nome", txtNomeLoja.Text);
                cmd.Parameters.AddWithValue("@id", lojaSelecionadaId);
                cmd.ExecuteNonQuery();
            }
            CarregarLojas();
        }

        private void DeletarLoja()
        {
            if (lojaSelecionadaId == 0) return;
            var confirm = MessageBox.Show("Deseja excluir esta loja e todo o seu estoque?", "Confirmar", MessageBoxButtons.YesNo);
            if (confirm == DialogResult.Yes)
            {
                using (var conn = new SqliteConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();
                    var cmd = new SqliteCommand("DELETE FROM Lojas WHERE Id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", lojaSelecionadaId);
                    cmd.ExecuteNonQuery();
                }
                CarregarLojas();
            }
        }
    }
}