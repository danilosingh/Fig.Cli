using System;
using System.Collections.Generic;

namespace Fig.Cli
{
    public abstract class Command
    {
        private readonly IList<Func<CommandResult>> steps;
        protected FigContext Context { get; }

        public abstract CommandResult Execute();

        protected Command(FigContext context)
        {
            steps = new List<Func<CommandResult>>();
            Context = context;
        }

        protected void AddStep(Func<CommandResult> func)
        {
            steps.Add(func);
        }

        protected void AddStep(Func<bool> func)
        {
            steps.Add(() => new CommandResult(func()));
        }

        protected void AddStep(Action act)
        {
            bool func()
            {
                act();
                return true;
            }

            steps.Add(() => new CommandResult(func()));
        }

        protected void AddStep(bool condition, Func<CommandResult> func)
        {
            if (condition)
                AddStep(func);
        }

        protected CommandResult ExecuteSteps(string messageOk = null)
        {
            foreach (var step in steps)
            {
                var result = step();

                if (!result.IsValid)
                    return result;
            }

            return Ok(messageOk);
        }

        protected CommandResult Canceled()
        {
            return new CommandResult(false, "Canceled.");
        }

        protected CommandResult Ok(string message = "Completed.", params object[] args)
        {
            return CommandResult.Ok(message != null ? string.Format(message, args) : null);
        }

        protected CommandResult Fail(string message = "Operation failed.", params object[] args)
        {
            return new CommandResult(false, message != null ? string.Format(message, args) : null);
        }

        protected bool Confirm(string message, params object[] args)
        {
            if (!message.EndsWith("[Enter]"))
                message += " [Enter]";

            WriteLine(message, args);

            return Console.ReadKey().Key == ConsoleKey.Enter;
        }

        protected void WriteLine(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }

        protected void WriteBreakLine()
        {
            Console.WriteLine();
        }

        protected void Write(string message, params object[] args)
        {
            Console.Write(message, args);
        }

        protected bool IsInitialized()
        {
            return Context.IsInitialized();
        }

        protected virtual void EnsureConfiguration()
        {
            if (!IsInitialized())
            {
                throw new ArgumentException("Fig.Cli not configured. Use the option 'config'.");
            }
        }
    }

    public abstract class Command<T> : Command
    {
        public T Options { get; }

        protected Command(T opts, FigContext context) : base(context)
        {
            Options = opts;
        }
    }
}
