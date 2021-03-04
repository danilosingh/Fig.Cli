using Fig.Cli.Extensions;
using Fig.Cli.Options;
using System;
using System.Data;
using System.IO;

namespace Fig.Cli.Commands
{
    public class MigrateDbCommand : RunScriptsCommand<MigrateDbOptions>
    {
        public MigrateDbCommand(MigrateDbOptions opts, FigContext context) : base(opts, context)
        { }

        public override CommandResult Execute()
        {
            PrepareOptions();
            CheckMigrationSchema();
            EnsureValidOptions();
            return RunScripts();
        }

        protected override void PrepareOptions()
        {
            Options.MigrationsTable = Options.MigrationsTable ?? Context.Options.DbMigrationsTable;
            base.PrepareOptions();
        }

        private void CheckMigrationSchema()
        {
            using (var conn = CreateConnection())
            {
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT 1 FROM {Options.MigrationsTable}";
                        var reader = cmd.ExecuteScalar();
                    }
                }
                catch
                {
                    CreateMigrationSchema(conn);
                }
            }
        }

        private void CreateMigrationSchema(IDbConnection conn)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"CREATE TABLE {Options.MigrationsTable} (id serial, script varchar(250), date timestamp);";
                cmd.ExecuteScalar();
            }
        }

        protected override void ExecScript(string scriptPath, IDbTransaction transaction, int count, int currentIndex)
        {
            var fileName = Path.GetFileName(scriptPath);

            if (!AllowExecScript(transaction, fileName))
            {
                Console.WriteLine($"Skiped script {fileName}");
                return;
            }
            
            base.ExecScript(scriptPath, transaction, count, currentIndex);

            SaveExecutedScript(transaction, fileName);
        }

        private void SaveExecutedScript(IDbTransaction transaction, string fileName)
        {
            using (var command = transaction.Connection.CreateCommand())
            {
                command.CommandText = $"INSERT INTO {Options.MigrationsTable} (script, date) VALUES (@script, current_timestamp)";
                command.AddParameter("@script", fileName);
                command.ExecuteNonQuery();
            }
        }

        private bool AllowExecScript(IDbTransaction transaction, string fileName)
        {
            using (var command = transaction.Connection.CreateCommand())
            {                
                command.CommandText = $"SELECT NULL FROM {Options.MigrationsTable} WHERE script = @script";
                command.AddParameter("script", fileName);
                return !command.HasRows();
            }
        }
    }
}
