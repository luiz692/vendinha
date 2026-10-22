using System.ComponentModel.DataAnnotations;

namespace Vendinha.Models
{
    public class Divida
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor da dívida precisa ser maior que zero")]
        public decimal Valor { get; set; }

        public bool Paga { get; set; }

        public DateTime DataCriacao { get; set; }

        public DateTime? DataPagamento { get; set; }
    }
}
