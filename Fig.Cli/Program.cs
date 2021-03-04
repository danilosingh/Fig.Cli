using CommandLine;
using Fig.Cli.Commands;
using Fig.Cli.Options;
using System;
using System.Diagnostics;

namespace Fig.Cli
{
    class Program
    {
        static void Main(string[] args)
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
                        MigrateDbOptions,
                        RunScriptsOptions,
                        ClearBranchesOptions,
                        FindInBranchesOptions,
                        SetOptions,
                        GuidOptions >(args)
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
            catch (ArgumentException ex)
            {
                ConsoleWrite(ex.Message);
                exitCode = 1;
            }
            catch (Exception ex)
            {
                ConsoleWrite("Error: {0}", ex.InnerException?.Message ?? ex.Message);
                exitCode = 1;
            }
            finally
            {
                if (Debugger.IsAttached)
                    Console.ReadKey();

                Environment.Exit(exitCode);
            }
        }

        private static void ConsoleWrite(string format, params object[] args)
        {
            if (string.IsNullOrEmpty(format))
                return;

            Console.WriteLine(format, args);
        }
    }
}
