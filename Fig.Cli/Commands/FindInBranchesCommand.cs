using Fig.Cli.Helpers;
using Fig.Cli.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Fig.Cli.Commands
{
    public class FindInBranchesCommand : AzureDevOpsCommand<FindInBranchesOptions>
    {
        public FindInBranchesCommand(FindInBranchesOptions opts, FigContext context) : base(opts, context)
        { }

        public override CommandResult Execute()
        {
            GitHelper.Fetch(prune: false, showCommand: false);

            if (GitHelper.HasChanges(false, false))
                return Fail("Has uncommited changes in current branch");

            var branches = GitHelper.GetLocalBranches();

            if(Options.OnlyDevBranches)
            {
                branches = branches.Where(c => c.StartsWith("dev/")).ToList();
            }

            var foundedBranches = new List<string>();
            
            foreach (var item in branches)
            {
                Write($"Searching in {item}... ");
                GitHelper.Checkout(item, showCommand: false, writeOutput: false);

                var fileName = Path.Combine(Context.RootDirectory, Options.FileName);

                if (!File.Exists(fileName))
                {
                    WriteLine($"[File {fileName} not found]");
                }
                else
                {
                    if(CultureInfo.CurrentCulture.CompareInfo.IndexOf(File.ReadAllText(fileName), Options.SearchText, CompareOptions.IgnoreCase) >= 0)
                    {
                        foundedBranches.Add(item);
                        WriteLine($"[Found]");
                    }
                    else
                    {
                        WriteLine($"[Not found]");
                    }
                }
            }

            WriteBreakLine();

            if (!foundedBranches.Any())
                return Ok("Not found.");

            WriteLine("Found in branches: \n{0}", string.Join(Environment.NewLine, foundedBranches));
        
            return Ok();
        }
    }
}
