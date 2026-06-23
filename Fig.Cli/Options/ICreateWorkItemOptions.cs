namespace Fig.Cli.Options
{
    public interface ICreateWorkItemOptions
    {
        string Title { get; }
        string DescFile { get; }
        string AcFile { get; }
        int? Parent { get; }
    }
}
