using System.Data;

namespace Fig.Cli.Extensions
{
    public static class DbExtensions
    {
        public static void AddParameter(this IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        public static bool HasRows(this IDbCommand command)
        {
            using (var reader = command.ExecuteReader())
            {
                return reader.Read();
            }
        }
    }
}
