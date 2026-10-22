using System.ComponentModel.DataAnnotations;

namespace Vendinha.Validations
{
    public class CpfAttribute : ValidationAttribute
    {
        public CpfAttribute()
        {
            ErrorMessage = "CPF inválido";
        }

        public override bool IsValid(object? value)
        {
            var cpf = value as string;
            if (cpf == null)
            {
                return true;
            }

            cpf = cpf.Replace(".", "").Replace("-", "").Trim();

            if (cpf.Length != 11)
            {
                return false;
            }

            foreach (var c in cpf)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }

            if (cpf == new string(cpf[0], 11))
            {
                return false;
            }

            var primeiroDigito = CalcularDigito(cpf, 9);
            var segundoDigito = CalcularDigito(cpf, 10);

            return cpf[9] - '0' == primeiroDigito && cpf[10] - '0' == segundoDigito;
        }

        private int CalcularDigito(string cpf, int quantidade)
        {
            var soma = 0;
            var peso = quantidade + 1;
            for (var i = 0; i < quantidade; i++)
            {
                soma += (cpf[i] - '0') * peso;
                peso--;
            }

            var resto = soma % 11;
            if (resto < 2)
            {
                return 0;
            }
            return 11 - resto;
        }
    }
}
