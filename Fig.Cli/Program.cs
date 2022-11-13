using CommandLine;
using Fig.Cli.Commands;
using Fig.Cli.Extensions;
using Fig.Cli.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fig.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            var exitCode = 0;

            try
            {
                var result = Parser.Default
                    .ParseArguments<
                        InitOptions,
                        StartWorkItemOptions,
                        DoneWorkItemOptions,
                        SyncOptions,
                        MergeOptions,
                        RebaseOptions,
                        ReleaseOptions,
                        PullRequestOptions,
                        MergePullRequestOptions,
                        ScriptOptions,
                        ScriptTemplateOptions,
                        MigrateDbOptions,
                        RunScriptsOptions,
                        ClearBranchesOptions,
                        FindInBranchesOptions,
                        SetOptions,
                        GuidOptions>(args)
                    .MapResult(
                        (InitOptions opts) => CommandFactory.Execute<InitCommand>(opts),
                        (StartWorkItemOptions opts) => CommandFactory.Execute<StartWorkItemCommand>(opts),
                        (DoneWorkItemOptions opts) => CommandFactory.Execute<DoneWorkItemCommand>(opts),
                        (SyncOptions opts) => CommandFactory.Execute<SyncCommand>(opts),
                        (MergeOptions opts) => CommandFactory.Execute<MergeCommand>(opts),
                        (RebaseOptions opts) => CommandFactory.Execute<RebaseCommand>(opts),
                        (ReleaseOptions opts) => CommandFactory.Execute<ReleaseCommand>(opts),
                        (PullRequestOptions opts) => CommandFactory.Execute<PullRequestCommand>(opts),
                        (MergePullRequestOptions opts) => CommandFactory.Execute<MergePullRequestCommand>(opts),
                        (ScriptOptions opts) => CommandFactory.Execute<ScriptCommand>(opts),
                        (ScriptTemplateOptions opts) => CommandFactory.Execute<ScriptTemplateCommand>(opts),
                        (MigrateDbOptions opts) => CommandFactory.Execute<MigrateDbCommand>(opts),
                        (RunScriptsOptions opts) => CommandFactory.Execute<RunScriptsCommand>(opts),
                        (ClearBranchesOptions opts) => CommandFactory.Execute<ClearBranchesCommand>(opts),
                        (FindInBranchesOptions opts) => CommandFactory.Execute<FindInBranchesCommand>(opts),
                        (SetOptions opts) => CommandFactory.Execute<SetOptionCommand>(opts),
                        (GuidOptions opts) => CommandFactory.Execute<GuidCommand>(opts),
                        (errs) => new CommandResult(false));

                if (!result.IsValid)
                {
                    exitCode = 1;
                }

                if (!string.IsNullOrEmpty(result.Message))
                {
                    ConsoleWrite(result.Message);
                }
            }
            catch (FigException ex)
            {
                ConsoleWrite(ex.Message);
                exitCode = 1;
            }
            catch (Exception ex)
            {
                ConsoleWrite(GetFormattedException(ex));
                exitCode = 1;
            }

            return exitCode;
        }

        private static string GetFormattedException(Exception ex)
        {
            var exceptions = new List<Exception>() { ex };

            while (ex.InnerException != null)
            {
                exceptions.Add(ex.InnerException);
                ex = ex.InnerException;
            }

            var figException = exceptions.FirstOrDefault(c => c is FigException);

            if (figException != null)
            {
                return figException.Message;
            }

            return "Error: " + string.Join(Environment.NewLine,
                exceptions.AsEnumerable().Reverse()
                .Select(c => $"{c.Message} on {c.StackTrace}"));
        }

        private static void ConsoleWrite(string format, params object[] args)
        {
            if (string.IsNullOrEmpty(format))
                return;

            Console.WriteLine(format, args);
        }
    }
}
