using Fig.Cli.Helpers;
using Fig.Cli.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace Fig.Cli.Commands
{
    public class RunScriptsCommand<T> : Command<T>
        where T : RunScriptsOptions
    {
        public RunScriptsCommand(T opts, FigContext context) : base(opts, context)
        { }

        public override CommandResult Execute()
        {
            PrepareOptions();
            EnsureValidOptions();
            return RunScripts();
        }

        protected CommandResult RunScripts()
        {
            if (!Options.SupressConfirmation && !ConfirmRun())
            {
                return Canceled();
            }

            var scripts = GetScripts();

            if (scripts.Count == 0)
            {
                return Ok("No scripts.");
            }

            RunScripts(scripts);
            return Ok();
        }

        private bool ConfirmRun()
        {
            if (Options.SupressConfirmation)
                return true;

            return Confirm("Do you want to run the scripts in the database {0} ({1})?.", Options.Database, Options.Server);
        }

        private void RunScripts(IList<string> scripts)
        {
            using (var conn = CreateConnection())
            {
                var transaction = conn.BeginTransaction();

                try
                {
                    if (!string.IsNullOrEmpty(Options.GreaterThan))
                        Console.WriteLine("Running greater than {0}", Options.GreaterThan);

                    for (int i = 0; i < scripts.Count; i++)
                    {
                        ExecScript(scripts[i], transaction, scripts.Count, i + 1);
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        protected virtual void ExecScript(string scriptPath, IDbTransaction transaction, int count, int currentIndex)
        {
            var allText = File.ReadAllText(scriptPath, Encoding.UTF8);
            var commandsText = allText.Split(new string[] { "GO;", "go;" }, StringSplitOptions.RemoveEmptyEntries);

            Console.WriteLine($"Running script {Path.GetFileName(scriptPath)} ({currentIndex}/{count}) {GetAuthorName(allText)}");
            try
            {
                foreach (var commandText in commandsText)
                {
                    ExecCommandText(commandText, transaction);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on script: {scriptPath} / Message: {ex.Message}");
                throw ex;
            }
        }

        private string GetAuthorName(string allText)
        {
            var i = allText.IndexOf("Autor:");

            if (i < 0)
                return null;

            i += 6;
            return "(" + allText[i..allText.IndexOf("\r\n", i)]?.Trim() + ")";
        }

        private void ExecCommandText(string commandText, IDbTransaction transaction)
        {
            using (var command = transaction.Connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = commandText;
                command.ExecuteNonQuery();
            }
        }

        protected IDbConnection CreateConnection()
        {
            IDbConnection connection = Options.Provider == DbProviders.SqlServer ?
                CreateSqlServerConnection() :
                CreatePostgreSqlConnection();

            connection.Open();
            return connection;
        }

        protected IDbConnection CreatePostgreSqlConnection()
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = Options.Server,
                Database = Options.Database,
                Password = Options.Password,
                Username = Options.UserName
            };

            return new NpgsqlConnection(connectionStringBuilder.ToString());
        }

        protected IDbConnection CreateSqlServerConnection()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = Options.Server,
                InitialCatalog = Options.Database,
                Password = Options.Password,
                UserID = Options.UserName
            };

            return new SqlConnection(connectionStringBuilder.ToString());
        }

        protected IList<string> GetScripts()
        {
            if (Options.IgnoreDirectoryExists && !Directory.Exists(Options.ScriptsDirectory))
                return new List<string>();

            var files = Directory.GetFiles(Options.ScriptsDirectory).Where(c => c.EndsWith(".sql")).ToList();

            if (!string.IsNullOrEmpty(Options.GreaterThan))
                files = files.Where(c => StringComparer.CurrentCulture.Compare(Path.GetFileName(c), Options.GreaterThan) == 1).ToList();

            return files.OrderBy(c => c).ToList();
        }

        protected virtual void PrepareOptions()
        {
            Options.ScriptsDirectory = Options.ScriptsDirectory ?? StringHelper.ConcatPath(Context.RootDirectory, Context.Options.DbScriptPath);
            Options.Server = Options.Server ?? Context.Options.DbServer;
            Options.Database = Options.Database ?? Context.Options.DbName;
            Options.UserName = Options.UserName ?? Context.Options.DbUserName;
            Options.Password = Options.Password ?? Context.Options.DbPassword;
            Options.GreaterThan = Options.GreaterThan;
            Options.Provider = Options.Provider ?? Context.Options.DbProvider;
        }

        protected void EnsureValidOptions()
        {
            if (string.IsNullOrEmpty(Options.ScriptsDirectory))
                throw new ArgumentException("Scripts directory not configured.");

            if (string.IsNullOrEmpty(Options.Provider))
                throw new ArgumentException("Provider not configured.");
            else if (Options.Provider != DbProviders.PostgreSql && Options.Provider != DbProviders.SqlServer)
                throw new ArgumentException("Invalid provider.");

            if (Options.Provider == DbProviders.SqlServer && string.IsNullOrEmpty(Options.Server))
                throw new ArgumentException("Server not configured.");

            if (string.IsNullOrEmpty(Options.Database))
                throw new ArgumentException("Database not configured.");

            if (string.IsNullOrEmpty(Options.UserName))
                throw new ArgumentException("UserName not configured.");

            if (string.IsNullOrEmpty(Options.Password))
                throw new ArgumentException("Password not configured.");
        }
    }

    public class RunScriptsCommand : RunScriptsCommand<RunScriptsOptions>
    {
        public RunScriptsCommand(RunScriptsOptions opts, FigContext context) : base(opts, context)
        {
        }
    }
}
