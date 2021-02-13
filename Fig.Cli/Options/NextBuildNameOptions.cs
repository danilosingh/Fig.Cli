namespace Fig.Cli.Options
{
    public class NextBuildNameOptions : BaseOptions
    {
        public string AccessToken { get; set; }
        public int BuildDefitionId { get; internal set; }
    }
}
