namespace Fig.Cli
{
    public class CommandResult
    {
        public CommandResult(bool isValid, string message=null)
        {
            IsValid = isValid;
            Message = message;
        }

        public bool IsValid { get; private set; }
        public string Message { get; private set; }

        public static CommandResult Ok(string message = null)
        {
            return new CommandResult(true, message);
        }
    }
}
