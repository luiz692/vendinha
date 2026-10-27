using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Vendinha.Data;
using Vendinha.Models;

namespace Vendinha.Services
{
    public class DividaService
    {
        private readonly Database database;

        public DividaService(Database database)
        {
            this.database = database;
        }

        public List<Divida> ListarPorCliente(int clienteId)
        {
            var dividas = new List<Divida>();

            using (var conexao = database.AbrirConexao())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = @"SELECT id, cliente_id, valor, paga, data_criacao, data_pagamento
                                        FROM dividas
                                        WHERE cliente_id = @clienteId
                                        ORDER BY data_criacao DESC, id DESC";
                comando.Parameters.AddWithValue("@clienteId", clienteId);

                using (var leitor = comando.ExecuteReader())
                {
                    while (leitor.Read())
                    {
                        dividas.Add(LerDivida(leitor));
                    }
                }
            }

            return dividas;
        }

        public Divida? BuscarPorId(int id)
        {
            using (var conexao = database.AbrirConexao())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = @"SELECT id, cliente_id, valor, paga, data_criacao, data_pagamento
                                        FROM dividas
                                        WHERE id = @id";
                comando.Parameters.AddWithValue("@id", id);

                using (var leitor = comando.ExecuteReader())
                {
                    if (leitor.Read())
                    {
                        return LerDivida(leitor);
                    }
                }
            }

            return null;
        }

        public List<string> Criar(Divida divida)
        {
            var erros = Validar(divida);
            if (erros.Count > 0)
            {
                return erros;
            }

            if (TemDividaEmAberto(divida.ClienteId))
            {
                erros.Add("Esse cliente já tem uma dívida em aberto");
                return erros;
            }

            divida.Paga = false;
            divida.DataCriacao = DateTime.Now;
            divida.DataPagamento = null;

            using (var conexao = database.AbrirConexao())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = @"INSERT INTO dividas (cliente_id, valor, paga, data_criacao)
                                        VALUES (@clienteId, @valor, 0, @dataCriacao);
                                        SELECT last_insert_rowid();";
                comando.Parameters.AddWithValue("@clienteId", divida.ClienteId);
                comando.Parameters.AddWithValue("@valor", divida.Valor);
                comando.Parameters.AddWithValue("@dataCriacao", divida.DataCriacao);
                divida.Id = Convert.ToInt32(comando.ExecuteScalar());
            }

            return erros;
        }

        public List<string> Pagar(Divida divida)
        {
            var erros = new List<string>();

            if (divida.Paga)
            {
                erros.Add("Essa dívida já está paga");
                return erros;
            }

            divida.Paga = true;
            divida.DataPagamento = DateTime.Now;

            using (var conexao = database.AbrirConexao())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = @"UPDATE dividas
                                        SET paga = 1, data_pagamento = @dataPagamento
                                        WHERE id = @id";
                comando.Parameters.AddWithValue("@dataPagamento", divida.DataPagamento);
                comando.Parameters.AddWithValue("@id", divida.Id);
                comando.ExecuteNonQuery();
            }

            return erros;
        }

        public bool TemDividaEmAberto(int clienteId)
        {
            using (var conexao = database.AbrirConexao())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = "SELECT COUNT(*) FROM dividas WHERE cliente_id = @clienteId AND paga = 0";
                comando.Parameters.AddWithValue("@clienteId", clienteId);
                return Convert.ToInt32(comando.ExecuteScalar()) > 0;
            }
        }

        public List<string> Validar(Divida divida)
        {
            var context = new ValidationContext(divida);
            var resultados = new List<ValidationResult>();
            Validator.TryValidateObject(divida, context, resultados, true);

            var erros = new List<string>();
            foreach (var resultado in resultados)
            {
                erros.Add(resultado.ErrorMessage ?? "Dado inválido");
            }

            return erros;
        }

        private Divida LerDivida(SqliteDataReader leitor)
        {
            return new Divida
            {
                Id = leitor.GetInt32(0),
                ClienteId = leitor.GetInt32(1),
                Valor = leitor.GetDecimal(2),
                Paga = leitor.GetBoolean(3),
                DataCriacao = leitor.GetDateTime(4),
                DataPagamento = leitor.IsDBNull(5) ? null : leitor.GetDateTime(5)
            };
        }
    }
}
