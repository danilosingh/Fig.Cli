using Fig.Cli.Options;
using System.Linq;

namespace Fig.Cli.Commands
{
    public class ScriptTemplateCommand : GitCommand<ScriptTemplateOptions>
    {
        public ScriptTemplateCommand(ScriptTemplateOptions opts, FigContext context) : base(opts, context)
        { }

        public override CommandResult Execute()
        {
            var template = Context.Options.Templates.FirstOrDefault(c => c.Name == Options.TemplateName) ??
                new FigOptionsScriptTemplate(Options.TemplateName, Options.Script);
 
            if (!Context.Options.Templates.Contains(template))
            {
                Context.Options.Templates.Add(template);
            }

            Context.SaveOptions();

            return Ok("Template {0} created.", Options.TemplateName);
        }
    }
}
