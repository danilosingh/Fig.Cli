using Fig.Cli.Options;
using Fig.Cli.Versioning;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using System.Collections.Generic;
using System.Linq;

namespace Fig.Cli.Commands
{
    public class NextBuildNameCommand : AzureDevOpsCommand<NextBuildNameOptions>
    {
        public NextBuildNameCommand(NextBuildNameOptions opts, FigContext context) : base(opts, context)
        {
        }

        public override CommandResult Execute()
        {
            var version = VersionInfo.LoadVersion();

            if (version != null)
                return Fail("Version file not found.");

            var buildClient = AzureContext.Connection.GetClient<BuildHttpClient>();
            var builds = buildClient.GetBuildsAsync(definitions: new List<int>() { Options.BuildDefitionId }, queryOrder: BuildQueryOrder.StartTimeDescending).Result;
            var revision = builds.Where(c => c.BuildNumber.StartsWith(version.ToString())).Count() + 1;
            return Fail();
        }

        protected override void EnsureConfiguration()
        { }

        protected override VssCredentials CreateCredentials()
        {
            return !string.IsNullOrEmpty(Options.AccessToken) ? 
                new VssBasicCredential(string.Empty, Options.AccessToken) : 
                base.CreateCredentials();
        }
    }
}
