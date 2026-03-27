using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SistemaEstoque
{
    public class StoreForm : Form
    {
        private string lojaNome;
        private string nomeProdutoOriginal = "";
        private TextBox txtNome, txtQuantidade, txtFornecedor, txtPreco;
        private DataGridView dgvProdutos;

        public StoreForm(string lojaNome)
        {
            this.lojaNome = lojaNome;
            ConfigurarInterface();
            CarregarProdutos();
        }

        private void ConfigurarInterface()
        {
            this.Text = $"Estoque: {lojaNome}";
            this.Size = new Size(800, 600);
            this.BackColor = Color.FromArgb(44, 62, 80);
            this.StartPosition = FormStartPosition.CenterParent;

            Panel painelTopo = new Panel { Dock = DockStyle.Top, Height = 140, BackColor = Color.FromArgb(52, 73, 94) };

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
                    nomeProdutoOriginal = dgvProdutos.Rows[e.RowIndex].Cells["Nome"].Value.ToString();
                    txtNome.Text = nomeProdutoOriginal;
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
            parent.Controls.Add(lbl); parent.Controls.Add(txt);
        }

        private Button CriarBotao(string texto, Color cor, Point local)
        {
            return new Button { Text = texto, BackColor = cor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = local, Size = new Size(80, 35), Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
        }

        private void CarregarProdutos()
        {
            var lojas = DatabaseHelper.CarregarDados();
            var loja = lojas.FirstOrDefault(l => l.Nome == lojaNome);
            if (loja != null)
            {
                dgvProdutos.DataSource = null;
                dgvProdutos.DataSource = loja.Produtos.ToList();
                if (dgvProdutos.Columns["Preco"] != null)
                    dgvProdutos.Columns["Preco"].DefaultCellStyle.Format = "C2";
            }
            nomeProdutoOriginal = "";
            txtNome.Clear(); txtQuantidade.Clear(); txtFornecedor.Clear(); txtPreco.Clear();
        }

        private void AdicionarProduto()
        {
            if (!ValidarCampos(out int qtd, out double preco)) return;
            var lojas = DatabaseHelper.CarregarDados();
            var loja = lojas.FirstOrDefault(l => l.Nome == lojaNome);
            if (loja != null)
            {
                loja.Produtos.Add(new Produto { Nome = txtNome.Text, Quantidade = qtd, Fornecedor = txtFornecedor.Text, Preco = preco });
                DatabaseHelper.SalvarDados(lojas);
                CarregarProdutos();
            }
        }

        private void AtualizarProduto()
        {
            if (string.IsNullOrEmpty(nomeProdutoOriginal) || !ValidarCampos(out int qtd, out double preco)) return;
            var lojas = DatabaseHelper.CarregarDados();
            var loja = lojas.FirstOrDefault(l => l.Nome == lojaNome);
            var prod = loja?.Produtos.FirstOrDefault(p => p.Nome == nomeProdutoOriginal);
            if (prod != null)
            {
                prod.Nome = txtNome.Text;
                prod.Quantidade = qtd;
                prod.Fornecedor = txtFornecedor.Text;
                prod.Preco = preco;
                DatabaseHelper.SalvarDados(lojas);
                CarregarProdutos();
            }
        }

        private void DeletarProduto()
        {
            if (string.IsNullOrEmpty(nomeProdutoOriginal)) return;
            var lojas = DatabaseHelper.CarregarDados();
            var loja = lojas.FirstOrDefault(l => l.Nome == lojaNome);
            if (loja != null)
            {
                loja.Produtos.RemoveAll(p => p.Nome == nomeProdutoOriginal);
                DatabaseHelper.SalvarDados(lojas);
                CarregarProdutos();
            }
        }

        private bool ValidarCampos(out int qtd, out double preco)
        {
            preco = 0; qtd = 0;
            if (string.IsNullOrWhiteSpace(txtNome.Text) || !int.TryParse(txtQuantidade.Text, out qtd) || !double.TryParse(txtPreco.Text, out preco))
            {
                MessageBox.Show("Verifique os campos.");
                return false;
            }
            return true;
        }
    }
}
