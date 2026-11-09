namespace TuitionDesk.Infrastructure.Persistence;

public sealed record DatabaseOptions(string DataDirectory, string DatabasePath, string ConnectionString)
{
    public string BackupDirectory => DataPaths.BackupDirectory(DataDirectory);
}
