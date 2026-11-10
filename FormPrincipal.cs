using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace ControlePendencias
{
    public partial class FormPrincipal : Form
    {
        // Lista que guarda as pendências enquanto o programa está aberto.
        private List<Pendencia> pendencias = new List<Pendencia>();
        private int idSelecionado = 0;
        private bool atualizandoLista = false;

        // O arquivo fica na pasta do usuário, mesmo depois de fechar o programa.
        private string pastaDados = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ControlePendenciasAdministrativas");

        public FormPrincipal()
        {
            InitializeComponent();

            cmbPrioridade.SelectedIndex = 1; // Média
            cmbSituacao.SelectedIndex = 0;  // Pendente
            cmbFiltro.SelectedIndex = 0;    // Todas

            CarregarPendencias();
            AtualizarLista();
        }

        // Lê as pendências salvas no arquivo JSON.
        private void CarregarPendencias()
        {
            string caminho = Path.Combine(pastaDados, "pendencias.json");

            if (!File.Exists(caminho))
                return;

            try
            {
                string texto = File.ReadAllText(caminho);
                List<Pendencia> lista = JsonSerializer.Deserialize<List<Pendencia>>(texto);

                if (lista != null)
                    pendencias = lista;
            }
            catch (Exception erro)
            {
                MessageBox.Show("Não foi possível abrir as pendências salvas.\n" + erro.Message,
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Grava todas as pendências no JSON.
        private void SalvarPendencias()
        {
            try
            {
                Directory.CreateDirectory(pastaDados);
                string caminho = Path.Combine(pastaDados, "pendencias.json");
                JsonSerializerOptions opcoes = new JsonSerializerOptions();
                opcoes.WriteIndented = true;

                string texto = JsonSerializer.Serialize(pendencias, opcoes);
                File.WriteAllText(caminho, texto);
            }
            catch (Exception erro)
            {
                MessageBox.Show("Não foi possível salvar os dados.\n" + erro.Message,
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Confere os campos obrigatórios antes de salvar ou editar.
        private bool CamposValidos()
        {
            if (txtDescricao.Text.Trim() == "")
            {
                MessageBox.Show("Informe a descrição da pendência.");
                txtDescricao.Focus();
                return false;
            }

            if (txtResponsavel.Text.Trim() == "")
            {
                MessageBox.Show("Informe o responsável pela pendência.");
                txtResponsavel.Focus();
                return false;
            }

            return true;
        }

        // Descobre o próximo número de identificação, sem repetir os existentes.
        private int ProximoId()
        {
            int numero = 1;

            foreach (Pendencia item in pendencias)
            {
                if (item.Id >= numero)
                    numero = item.Id + 1;
            }

            return numero;
        }

        // Adiciona uma nova pendência.
        private void btnSalvar_Click(object sender, EventArgs e)
        {
            if (!CamposValidos())
                return;

            Pendencia nova = new Pendencia();
            nova.Id = ProximoId();
            nova.Descricao = txtDescricao.Text.Trim();
            nova.Responsavel = txtResponsavel.Text.Trim();
            nova.Prazo = dtpPrazo.Value.Date;
            nova.Prioridade = cmbPrioridade.Text;
            nova.Situacao = cmbSituacao.Text;
            nova.Observacao = txtObservacao.Text.Trim();

            pendencias.Add(nova);
            SalvarPendencias();
            AtualizarLista();
            LimparCampos();
            MessageBox.Show("Pendência cadastrada com sucesso!");
        }

        // Busca a pendência selecionada na tabela.
        private Pendencia BuscarSelecionada()
        {
            foreach (Pendencia item in pendencias)
            {
                if (item.Id == idSelecionado)
                    return item;
            }

            return null;
        }

        // Atualiza os dados de uma pendência existente.
        private void btnEditar_Click(object sender, EventArgs e)
        {
            Pendencia item = BuscarSelecionada();

            if (item == null)
            {
                MessageBox.Show("Selecione uma pendência na tabela para editar.");
                return;
            }

            if (!CamposValidos())
                return;

            item.Descricao = txtDescricao.Text.Trim();
            item.Responsavel = txtResponsavel.Text.Trim();
            item.Prazo = dtpPrazo.Value.Date;
            item.Prioridade = cmbPrioridade.Text;
            item.Situacao = cmbSituacao.Text;
            item.Observacao = txtObservacao.Text.Trim();

            SalvarPendencias();
            AtualizarLista();
            LimparCampos();
            MessageBox.Show("Pendência atualizada com sucesso!");
        }

        // Marca uma pendência como concluída.
        private void btnConcluir_Click(object sender, EventArgs e)
        {
            Pendencia item = BuscarSelecionada();

            if (item == null)
            {
                MessageBox.Show("Selecione uma pendência na tabela para concluir.");
                return;
            }

            item.Situacao = "Concluída";
            SalvarPendencias();
            AtualizarLista();
            LimparCampos();
            MessageBox.Show("Pendência concluída!");
        }

        // Exclui uma pendência após confirmação.
        private void btnExcluir_Click(object sender, EventArgs e)
        {
            Pendencia item = BuscarSelecionada();

            if (item == null)
            {
                MessageBox.Show("Selecione uma pendência na tabela para excluir.");
                return;
            }

            DialogResult resposta = MessageBox.Show(
                "Deseja realmente excluir esta pendência?",
                "Confirmar exclusão", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resposta == DialogResult.Yes)
            {
                pendencias.Remove(item);
                SalvarPendencias();
                AtualizarLista();
                LimparCampos();
            }
        }

        // Limpa o formulário para cadastrar outra pendência.
        private void LimparCampos()
        {
            idSelecionado = 0;
            txtDescricao.Clear();
            txtResponsavel.Clear();
            txtObservacao.Clear();
            dtpPrazo.Value = DateTime.Today;
            cmbPrioridade.SelectedIndex = 1;
            cmbSituacao.SelectedIndex = 0;
            dgvPendencias.ClearSelection();
        }

        private void btnLimpar_Click(object sender, EventArgs e)
        {
            LimparCampos();
        }

        // Ao clicar em uma linha, exibe a pendência para edição.
        private void dgvPendencias_SelectionChanged(object sender, EventArgs e)
        {
            if (atualizandoLista || dgvPendencias.SelectedRows.Count == 0)
                return;

            DataGridViewRow linha = dgvPendencias.SelectedRows[0];
            if (linha.Cells[0].Value == null)
                return;

            idSelecionado = Convert.ToInt32(linha.Cells[0].Value);
            Pendencia item = BuscarSelecionada();

            if (item == null)
                return;

            txtDescricao.Text = item.Descricao;
            txtResponsavel.Text = item.Responsavel;
            dtpPrazo.Value = item.Prazo;
            cmbPrioridade.Text = item.Prioridade;
            cmbSituacao.Text = item.Situacao;
            txtObservacao.Text = item.Observacao;
        }

        // Atualiza tabela, filtros e resumo da tela inicial.
        private void AtualizarLista()
        {
            atualizandoLista = true;
            dgvPendencias.Rows.Clear();

            string textoBusca = txtPesquisar.Text.Trim().ToLower();
            string filtro = cmbFiltro.Text;
            DateTime hoje = DateTime.Today;

            int totalPendentes = 0;
            int totalAtrasadas = 0;
            int totalHoje = 0;
            int totalConcluidas = 0;

            foreach (Pendencia item in pendencias)
            {
                bool concluida = item.Situacao == "Concluída";
                bool atrasada = !concluida && item.Prazo.Date < hoje;
                bool venceHoje = !concluida && item.Prazo.Date == hoje;

                // Os números do resumo consideram todas as pendências cadastradas.
                if (concluida)
                    totalConcluidas++;
                else
                    totalPendentes++;

                if (atrasada)
                    totalAtrasadas++;
                if (venceHoje)
                    totalHoje++;

                // Filtro de situação e prazo.
                if (filtro == "Pendentes" && item.Situacao != "Pendente")
                    continue;
                if (filtro == "Em andamento" && item.Situacao != "Em andamento")
                    continue;
                if (filtro == "Concluídas" && !concluida)
                    continue;
                if (filtro == "Atrasadas" && !atrasada)
                    continue;
                if (filtro == "Vencem hoje" && !venceHoje)
                    continue;

                // Pesquisa pelo texto da tarefa ou pelo responsável.
                if (textoBusca != "")
                {
                    bool encontrouDescricao = item.Descricao.ToLower().Contains(textoBusca);
                    bool encontrouResponsavel = item.Responsavel.ToLower().Contains(textoBusca);
                    if (!encontrouDescricao && !encontrouResponsavel)
                        continue;
                }

                int linhaNova = dgvPendencias.Rows.Add(
                    item.Id, item.Descricao, item.Responsavel,
                    item.Prazo.ToString("dd/MM/yyyy"), item.Prioridade, item.Situacao);

                // Cores simples para facilitar a leitura.
                DataGridViewRow linha = dgvPendencias.Rows[linhaNova];
                if (concluida)
                    linha.DefaultCellStyle.BackColor = Color.FromArgb(224, 244, 231);
                else if (atrasada)
                    linha.DefaultCellStyle.BackColor = Color.FromArgb(255, 228, 228);
                else if (venceHoje)
                    linha.DefaultCellStyle.BackColor = Color.FromArgb(255, 246, 209);
            }

            lblValorPendentes.Text = totalPendentes.ToString();
            lblValorAtrasadas.Text = totalAtrasadas.ToString();
            lblValorHoje.Text = totalHoje.ToString();
            lblValorConcluidas.Text = totalConcluidas.ToString();

            dgvPendencias.ClearSelection();
            atualizandoLista = false;
        }

        private void txtPesquisar_TextChanged(object sender, EventArgs e)
        {
            AtualizarLista();
        }

        private void cmbFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            AtualizarLista();
        }
    }
}
