using Fig.Cli.Helpers;
using Fig.Cli.Options;
using Newtonsoft.Json;

namespace Fig.Cli.Commands
{
    public class InitCommand : AzureDevOpsCommand<InitOptions>
    {
        public InitCommand(InitOptions opts, FigContext context) : base(opts, context)
        { }

        public override CommandResult Execute()
        {
            var figOptions = ConvertOptions(Options);
            Context.SetOptions(figOptions);
            ConfigureRepository(figOptions);            
            Context.SaveOptions();

            return CommandResult.Ok("Fig.Cli is configured.");
        }

        protected override void EnsureConfiguration()
        { }

        private FigOptions ConvertOptions(InitOptions options)
        {
            return JsonConvert.DeserializeObject<FigOptions>(JsonConvert.SerializeObject(options));
        }

        private void ConfigureRepository(FigOptions figOptions)
        {
            InitializeAzureContext();
            figOptions.RepositoryName = GitHelper.GetRemoteRepositoryName();
            figOptions.RepositoryId = AzureGitHelper.FindRepositoryId(AzureContext, AzureProjectHelper.FindDefaultProjectId(AzureContext), figOptions.RepositoryName)?.ToString();
        }
    }
}
