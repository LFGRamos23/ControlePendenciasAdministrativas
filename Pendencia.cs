using System;

namespace ControlePendencias
{
    // Representa uma pendência cadastrada pela gerente administrativa.
    public class Pendencia
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = "";
        public string Responsavel { get; set; } = "";
        public DateTime Prazo { get; set; }
        public string Prioridade { get; set; } = "Média";
        public string Situacao { get; set; } = "Pendente";
        public string Observacao { get; set; } = "";
    }
}
