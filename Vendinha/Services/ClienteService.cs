using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Vendinha.Data;
using Vendinha.Models;

namespace Vendinha.Services
{
    public class ClienteService
    {
        private readonly Database database;

        public ClienteService(Database database)
        {
            this.database = database;
        }

        public List<Cliente> Consultar()
        {
            var clientes = new List<Cliente>();

            using (var conexao = database.AbrirConexao())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = "SELECT id, nome, cpf, data_nascimento, email FROM clientes ORDER BY nome";

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
                comando.CommandText = "SELECT id, nome, cpf, data_nascimento, email FROM clientes WHERE id = @id";
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

        public bool Criar(Cliente cliente)
        {
            if (!Validar(cliente))
            {
                return false;
            }

            if (CpfExiste(cliente.Cpf, 0))
            {
                return false;
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

            return true;
        }

        public bool Atualizar(Cliente cliente)
        {
            if (!Validar(cliente))
            {
                return false;
            }

            if (CpfExiste(cliente.Cpf, cliente.Id))
            {
                return false;
            }

            using (var conexao = database.AbrirConexao())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = @"UPDATE clientes
                                        SET nome = @nome, cpf = @cpf, data_nascimento = @dataNascimento, email = @email
                                        WHERE id = @id";
                PreencherParametros(comando, cliente);
                comando.Parameters.AddWithValue("@id", cliente.Id);
                return comando.ExecuteNonQuery() > 0;
            }
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

        public bool Validar(Cliente cliente)
        {
            var context = new ValidationContext(cliente);
            var erros = new List<ValidationResult>();
            return Validator.TryValidateObject(cliente, context, erros, true);
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
                Email = leitor.IsDBNull(4) ? null : leitor.GetString(4)
            };
        }
    }
}
