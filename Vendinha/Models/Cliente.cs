using System.ComponentModel.DataAnnotations;
using Vendinha.Validations;

namespace Vendinha.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome precisa ter entre 3 e 100 caracteres")]
        public string Nome { get; set; }

        private string cpf;
        [Required(ErrorMessage = "O CPF é obrigatório")]
        [Cpf]
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
        [EmailAddress(ErrorMessage = "E-mail inválido")]
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

        public decimal TotalDivida { get; set; }

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