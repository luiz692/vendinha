using System.ComponentModel.DataAnnotations;
using Vendinha.Models;

namespace Vendinha.Services
{
    public class ClienteService
    {
        private List<Cliente> lista = new List<Cliente>();
        private int proximoId = 1;

        public List<Cliente> Consultar()
        {
            return lista.ToList();
        }

        public bool Criar(Cliente cliente)
        {
            if (!Validar(cliente))
            {
                return false;
            }

            if (CpfExiste(cliente.Cpf))
            {
                return false;
            }

            cliente.Id = proximoId;
            proximoId++;
            lista.Add(cliente);
            return true;
        }

        public bool CpfExiste(string cpf)
        {
            return lista.Any(c => c.Cpf == cpf);
        }

        public bool Validar(Cliente cliente)
        {
            var context = new ValidationContext(cliente);
            var erros = new List<ValidationResult>();
            return Validator.TryValidateObject(cliente, context, erros, true);
        }
    }
}
