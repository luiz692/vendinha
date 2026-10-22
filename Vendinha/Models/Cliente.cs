using System.ComponentModel.DataAnnotations;

namespace Vendinha.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Nome { get; set; }

        private string cpf;
        [Required]
        public string Cpf
        {
            get { return this.cpf; }
            set
            {
                this.cpf = value?.Replace(".", "").Replace("-", "").Trim();
            }
        }

        public DateTime DataNascimento { get; set; }

        private string? email;
        [EmailAddress]
        public string? Email
        {
            get { return this.email; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    this.email = null;
                }
                else
                {
                    this.email = value.ToLower().Trim();
                }
            }
        }

        public int Idade
        {
            get
            {
                var hoje = DateTime.Today;
                var idade = hoje.Year - DataNascimento.Year;
                var hojeAnoNascimento = hoje.AddYears(-idade);
                if (DataNascimento > hojeAnoNascimento)
                {
                    idade--;
                }
                return idade;
            }
        }
    }
}