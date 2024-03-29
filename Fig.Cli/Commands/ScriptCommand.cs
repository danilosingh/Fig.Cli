﻿using Fig.Cli.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Fig.Cli.Commands
{
    public class ScriptCommand : GitCommand<ScriptOptions>
    {
        public ScriptCommand(ScriptOptions opts, FigContext context) : base(opts, context)
        { }

        public override CommandResult Execute()
        {
            if (string.IsNullOrEmpty(Context.Options.DbScriptPath))
                throw new FigException(@"Script path not configured. Use config --spath database\script");

            var scriptName = DateTime.Now.ToString("yyMMddHHmmss");

            if (!string.IsNullOrEmpty(Context.Options.DeveloperId))
            {
                scriptName += Context.Options.DeveloperId;
            }

            scriptName += ".sql";

            var path = ConcatSlash(Context.RootDirectory) + ConcatSlash(Context.Options.DbScriptPath);

            if (!Directory.Exists(path))
                throw new FigException(string.Format(@"Script path {0} not found", path));

            string fileName = path + scriptName;
            WriteScript(fileName, GetTemplate());

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(fileName) { UseShellExecute = true }
            };
            process.Start();

            return Ok("Script {0} created.", fileName);
        }

        private string ConcatSlash(string path)
        {
            if (path[path.Length - 1] != '\\')
                path += @"\";
            return path;
        }

        private string GetTemplate()
        {
            var script = string.Empty.PadLeft(100, '-') + Environment.NewLine +
                "-- Autor: " + Context.Options.DeveloperName.Replace(Environment.NewLine, string.Empty) + Environment.NewLine +
                "-- Data: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + Environment.NewLine +
                string.Empty.PadLeft(100, '-') + Environment.NewLine;

            if (!string.IsNullOrEmpty(Options.TemplateName))
            {
                var template = Context.Options.Templates.FirstOrDefault(c => c.Name == Options.TemplateName);

                if (template == null)
                {
                    throw new FigException("Template not found.");
                }

                script += Environment.NewLine + template.Script;
            }

            return script;
        }

        private void WriteScript(string fileName, string contents)
        {
            using (var sw = new StreamWriter(new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite), new UTF8Encoding(false)))
            {
                sw.Write(contents);
            }
        }
    }
}
