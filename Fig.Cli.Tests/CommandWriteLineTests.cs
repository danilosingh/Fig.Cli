using System;
using System.IO;
using Xunit;

namespace Fig.Cli.Tests
{
    public class CommandWriteLineTests
    {
        // Expõe os métodos protegidos de Command para teste, sem depender de FigContext
        // (WriteLine/Write não usam o Context).
        private class TestableCommand : Command
        {
            public TestableCommand() : base(null)
            {
            }

            public override CommandResult Execute()
            {
                return new CommandResult(true);
            }

            public void CallWriteLine(string message)
            {
                WriteLine(message);
            }

            public void CallWrite(string message)
            {
                Write(message);
            }
        }

        [Fact]
        public void WriteLine_WhenMessageContainsBraces_DoesNotThrowAndPrintsLiteral()
        {
            // Regressão: `fig show <id>` quebrava com FormatException quando o ticket
            // continha `{` (ex.: "/product-invoices/{id}/devolution"), pois WriteLine
            // passava a mensagem como composite format string sem args.
            var command = new TestableCommand();
            var message = "POST /v1/product-invoices/{id}/devolution";
            var original = Console.Out;

            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                try
                {
                    var exception = Record.Exception(() => command.CallWriteLine(message));

                    Assert.Null(exception);
                    Assert.Contains("{id}", writer.ToString());
                }
                finally
                {
                    Console.SetOut(original);
                }
            }
        }

        [Fact]
        public void Write_WhenMessageContainsBraces_DoesNotThrowAndPrintsLiteral()
        {
            var command = new TestableCommand();
            var message = "chave {ABC} sem args";
            var original = Console.Out;

            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                try
                {
                    var exception = Record.Exception(() => command.CallWrite(message));

                    Assert.Null(exception);
                    Assert.Contains("{ABC}", writer.ToString());
                }
                finally
                {
                    Console.SetOut(original);
                }
            }
        }
    }
}
