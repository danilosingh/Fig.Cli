namespace Fig.Cli.Options
{
    public class FigOptions : BaseOptions
    {
        public string ProjectUrl { get; set; }
        public string ProjectName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Pat { get; set; }
        public string RepositoryId { get; set; }
        public string RepositoryName { get; set; }
        public string DeveloperId { get; set; }
        public string DeveloperName { get; set; }
        public string DbScriptPath { get; set; }
        public string DbUserName { get; set; }
        public string DbPassword { get; set; }
        public string DbServer { get; set; }
        public string DbName { get; set; }
        public string DbProvider { get; set; }
    }
}
