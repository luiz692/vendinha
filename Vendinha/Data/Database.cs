using Microsoft.Data.Sqlite;

namespace Vendinha.Data
{
    public class Database
    {
        private readonly string connectionString;

        public Database(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public SqliteConnection AbrirConexao()
        {
            var conexao = new SqliteConnection(connectionString);
            conexao.Open();

            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = "PRAGMA foreign_keys = ON;";
                comando.ExecuteNonQuery();
            }

            return conexao;
        }

        public void CriarTabelas(string caminhoScript)
        {
            var script = File.ReadAllText(caminhoScript);

            using (var conexao = AbrirConexao())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = script;
                comando.ExecuteNonQuery();
            }
        }
    }
}
