using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Vendinha.Data;
using Vendinha.Models;

namespace Vendinha.Services
{
    public class ClienteService
    {
        private const string SelectCliente = @"SELECT c.id, c.nome, c.cpf, c.data_nascimento, c.email,
                                                      COALESCE(SUM(CASE WHEN d.paga = 0 THEN d.valor ELSE 0 END), 0) AS total_divida
                                               FROM clientes c
                                               LEFT JOIN dividas d ON d.cliente_id = c.id";

        private readonly Database database;

        public ClienteService(Database database)
        {
            this.database = database;
        }

        public List<Cliente> Listar(int pagina, string? busca)
        {
            if (pagina < 1)
            {
                pagina = 1;
            }

            var clientes = new List<Cliente>();

            using (var conexao = database.AbrirConexao())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = SelectCliente + @"
                                        WHERE c.nome LIKE @busca
                                        GROUP BY c.id
                                        ORDER BY total_divida DESC, c.nome
                                        LIMIT 10 OFFSET @offset";
                comando.Parameters.AddWithValue("@busca", "%" + busca?.Trim() + "%");
                comando.Parameters.AddWithValue("@offset", (pagina - 1) * 10);

                using (var leitor = comando.ExecuteReader())
                {
                    while (leitor.Read())
                    {
                        clientes.Add(LerCliente(leitor));
                    }
                }
            }

            return clientes;
        }

        public Cliente? BuscarPorId(int id)
        {
            using (var conexao = database.AbrirConexao())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = SelectCliente + @"
                                        WHERE c.id = @id
                                        GROUP BY c.id";
                comando.Parameters.AddWithValue("@id", id);

                using (var leitor = comando.ExecuteReader())
                {
                    if (leitor.Read())
                    {
                        return LerCliente(leitor);
                    }
                }
            }

            return null;
        }

        public List<string> Criar(Cliente cliente)
        {
            var erros = Validar(cliente);
            if (erros.Count > 0)
            {
                return erros;
            }

            if (CpfExiste(cliente.Cpf, 0))
            {
                erros.Add("Já existe um cliente cadastrado com esse CPF");
                return erros;
            }

            using (var conexao = database.AbrirConexao())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = @"INSERT INTO clientes (nome, cpf, data_nascimento, email)
                                        VALUES (@nome, @cpf, @dataNascimento, @email);
                                        SELECT last_insert_rowid();";
                PreencherParametros(comando, cliente);
                cliente.Id = Convert.ToInt32(comando.ExecuteScalar());
            }

            return erros;
        }

        public List<string> Atualizar(Cliente cliente)
        {
            var erros = Validar(cliente);
            if (erros.Count > 0)
            {
                return erros;
            }

            if (CpfExiste(cliente.Cpf, cliente.Id))
            {
                erros.Add("Já existe outro cliente cadastrado com esse CPF");
                return erros;
            }

            using (var conexao = database.AbrirConexao())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = @"UPDATE clientes
                                        SET nome = @nome, cpf = @cpf, data_nascimento = @dataNascimento, email = @email
                                        WHERE id = @id";
                PreencherParametros(comando, cliente);
                comando.Parameters.AddWithValue("@id", cliente.Id);
                comando.ExecuteNonQuery();
            }

            return erros;
        }

        public bool Excluir(int id)
        {
            using (var conexao = database.AbrirConexao())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = "DELETE FROM clientes WHERE id = @id";
                comando.Parameters.AddWithValue("@id", id);
                return comando.ExecuteNonQuery() > 0;
            }
        }

        public bool CpfExiste(string cpf, int idIgnorado)
        {
            using (var conexao = database.AbrirConexao())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = "SELECT COUNT(*) FROM clientes WHERE cpf = @cpf AND id <> @id";
                comando.Parameters.AddWithValue("@cpf", cpf);
                comando.Parameters.AddWithValue("@id", idIgnorado);
                return Convert.ToInt32(comando.ExecuteScalar()) > 0;
            }
        }

        public List<string> Validar(Cliente cliente)
        {
            var context = new ValidationContext(cliente);
            var resultados = new List<ValidationResult>();
            Validator.TryValidateObject(cliente, context, resultados, true);

            var erros = new List<string>();
            foreach (var resultado in resultados)
            {
                erros.Add(resultado.ErrorMessage ?? "Dado inválido");
            }

            if (cliente.DataNascimento == default)
            {
                erros.Add("A data de nascimento é obrigatória");
            }
            else if (cliente.DataNascimento > DateTime.Today)
            {
                erros.Add("A data de nascimento não pode ser no futuro");
            }

            return erros;
        }

        private void PreencherParametros(SqliteCommand comando, Cliente cliente)
        {
            comando.Parameters.AddWithValue("@nome", cliente.Nome);
            comando.Parameters.AddWithValue("@cpf", cliente.Cpf);
            comando.Parameters.AddWithValue("@dataNascimento", cliente.DataNascimento.ToString("yyyy-MM-dd"));
            comando.Parameters.AddWithValue("@email", (object?)cliente.Email ?? DBNull.Value);
        }

        private Cliente LerCliente(SqliteDataReader leitor)
        {
            return new Cliente
            {
                Id = leitor.GetInt32(0),
                Nome = leitor.GetString(1),
                Cpf = leitor.GetString(2),
                DataNascimento = leitor.GetDateTime(3),
                Email = leitor.IsDBNull(4) ? null : leitor.GetString(4),
                TotalDivida = leitor.GetDecimal(5)
            };
        }
    }
}
