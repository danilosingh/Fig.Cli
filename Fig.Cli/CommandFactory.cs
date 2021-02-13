using System;

namespace Fig.Cli
{
    public static class CommandFactory
    {
        public static CommandResult Execute<T>(BaseOptions opts)
            where T : Command
        {
            return ((T)Activator.CreateInstance(typeof(T), opts, FigContext.Instance)).Execute();
        }
    }
}
