namespace Fig.Cli.Options
{
    public class FigOptionsScriptTemplate
    {
        public FigOptionsScriptTemplate(string name, string script)
        {
            Name = name;
            Script = script;
        }

        public FigOptionsScriptTemplate()
        {

        }

        public string Name { get; set; }
        public string Script { get; set; }
    }
}