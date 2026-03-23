using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace SistemaEstoque
{
    public class StoreForm : Form
    {
        private int lojaId;
        private string lojaNome;
        private int produtoSelecionadoId = 0;

        private TextBox txtNome, txtQuantidade, txtFornecedor, txtPreco;
        private DataGridView dgvProdutos;

        public StoreForm(int lojaId, string lojaNome)
        {
            this.lojaId = lojaId;
            this.lojaNome = lojaNome;
            ConfigurarInterface();
            CarregarProdutos();
        }

        private void ConfigurarInterface()
        {
            this.Text = $"Estoque da Loja: {lojaNome}";
            this.Size = new Size(800, 600);
            this.BackColor = Color.FromArgb(44, 62, 80);
            this.StartPosition = FormStartPosition.CenterParent;

            Panel painelTopo = new Panel { Dock = DockStyle.Top, Height = 140, BackColor = Color.FromArgb(52, 73, 94) };

            // Labels e TextBoxes (Design colorido e organizado)
            CriarCampo(painelTopo, "Produto:", out txtNome, 20, 20);
            CriarCampo(painelTopo, "Quantidade:", out txtQuantidade, 200, 20);
            CriarCampo(painelTopo, "Fornecedor:", out txtFornecedor, 380, 20);
            CriarCampo(painelTopo, "Preço (R$):", out txtPreco, 560, 20);

            Button btnAdicionar = CriarBotao("Adicionar", Color.FromArgb(39, 174, 96), new Point(20, 80));
            btnAdicionar.Click += (s, e) => AdicionarProduto();

            Button btnAtualizar = CriarBotao("Atualizar", Color.FromArgb(41, 128, 185), new Point(110, 80));
            btnAtualizar.Click += (s, e) => AtualizarProduto();

            Button btnDeletar = CriarBotao("Deletar", Color.FromArgb(192, 57, 43), new Point(200, 80));
            btnDeletar.Click += (s, e) => DeletarProduto();

            painelTopo.Controls.AddRange(new Control[] { btnAdicionar, btnAtualizar, btnDeletar });

            dgvProdutos = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };

            dgvProdutos.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    produtoSelecionadoId = Convert.ToInt32(dgvProdutos.Rows[e.RowIndex].Cells["Id"].Value);
                    txtNome.Text = dgvProdutos.Rows[e.RowIndex].Cells["Nome"].Value.ToString();
                    txtQuantidade.Text = dgvProdutos.Rows[e.RowIndex].Cells["Quantidade"].Value.ToString();
                    txtFornecedor.Text = dgvProdutos.Rows[e.RowIndex].Cells["Fornecedor"].Value.ToString();
                    txtPreco.Text = dgvProdutos.Rows[e.RowIndex].Cells["Preco"].Value.ToString();
                }
            };

            this.Controls.Add(dgvProdutos);
            this.Controls.Add(painelTopo);
        }

        private void CriarCampo(Panel parent, string textoLabel, out TextBox txt, int x, int y)
        {
            Label lbl = new Label { Text = textoLabel, ForeColor = Color.White, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txt = new TextBox { Location = new Point(x, y + 20), Width = 150 };
            parent.Controls.Add(lbl);
            parent.Controls.Add(txt);
        }

        private Button CriarBotao(string texto, Color cor, Point local)
        {
            return new Button { Text = texto, BackColor = cor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = local, Size = new Size(80, 35), Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
        }

        private void LimparCampos()
        {
            txtNome.Clear(); txtQuantidade.Clear(); txtFornecedor.Clear(); txtPreco.Clear();
            produtoSelecionadoId = 0;
        }

        private void CarregarProdutos()
        {
            using (var conn = new SqliteConnection(DatabaseHelper.ConnectionString))
            {
                conn.Open();
                var cmd = new SqliteCommand("SELECT Id, Nome, Quantidade, Fornecedor, Preco FROM Produtos WHERE LojaId = @lojaId", conn);
                cmd.Parameters.AddWithValue("@lojaId", lojaId);
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                dgvProdutos.DataSource = dt;
                dgvProdutos.Columns["Id"].Visible = false;

                // ADICIONE ESTA LINHA ABAIXO:
                dgvProdutos.Columns["Preco"].DefaultCellStyle.Format = "C2"; 
            }
            LimparCampos();
        }

        private void AdicionarProduto()
        {
            if (!ValidarCampos(out int qtd, out double preco)) return;

            using (var conn = new SqliteConnection(DatabaseHelper.ConnectionString))
            {
                conn.Open();
                var cmd = new SqliteCommand("INSERT INTO Produtos (LojaId, Nome, Quantidade, Fornecedor, Preco) VALUES (@lojaId, @nome, @qtd, @fornecedor, @preco)", conn);
                cmd.Parameters.AddWithValue("@lojaId", lojaId);
                cmd.Parameters.AddWithValue("@nome", txtNome.Text);
                cmd.Parameters.AddWithValue("@qtd", qtd);
                cmd.Parameters.AddWithValue("@fornecedor", txtFornecedor.Text);
                cmd.Parameters.AddWithValue("@preco", preco);
                cmd.ExecuteNonQuery();
            }
            CarregarProdutos();
        }

        private void AtualizarProduto()
        {
            if (produtoSelecionadoId == 0 || !ValidarCampos(out int qtd, out double preco)) return;

            using (var conn = new SqliteConnection(DatabaseHelper.ConnectionString))
            {
                conn.Open();
                var cmd = new SqliteCommand("UPDATE Produtos SET Nome=@nome, Quantidade=@qtd, Fornecedor=@fornecedor, Preco=@preco WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@nome", txtNome.Text);
                cmd.Parameters.AddWithValue("@qtd", qtd);
                cmd.Parameters.AddWithValue("@fornecedor", txtFornecedor.Text);
                cmd.Parameters.AddWithValue("@preco", preco);
                cmd.Parameters.AddWithValue("@id", produtoSelecionadoId);
                cmd.ExecuteNonQuery();
            }
            CarregarProdutos();
        }

        private void DeletarProduto()
        {
            if (produtoSelecionadoId == 0) return;

            using (var conn = new SqliteConnection(DatabaseHelper.ConnectionString))
            {
                conn.Open();
                var cmd = new SqliteCommand("DELETE FROM Produtos WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", produtoSelecionadoId);
                cmd.ExecuteNonQuery();
            }
            CarregarProdutos();
        }

        private bool ValidarCampos(out int qtd, out double preco)
        {
            preco = 0;
            if (string.IsNullOrWhiteSpace(txtNome.Text) || string.IsNullOrWhiteSpace(txtFornecedor.Text))
            {
                MessageBox.Show("Preencha o Nome e o Fornecedor.");
                qtd = 0; return false;
            }
            if (!int.TryParse(txtQuantidade.Text, out qtd))
            {
                MessageBox.Show("Quantidade deve ser um número inteiro.");
                return false;
            }
            if (!double.TryParse(txtPreco.Text, out preco))
            {
                MessageBox.Show("Preço inválido.");
                return false;
            }
            return true;
        }
    }
}