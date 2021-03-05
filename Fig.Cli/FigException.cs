using System;

namespace Fig.Cli
{
    public class FigException : Exception
    {
        public FigException()
        {
        }

        public FigException(string message) : base(message)
        {
        }
    }
}
