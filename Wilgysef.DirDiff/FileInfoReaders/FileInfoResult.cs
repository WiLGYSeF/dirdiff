namespace Wilgysef.DirDiff.FileInfoReaders;

public class FileInfoResult
{
    public long? Length { get; set; }

    public DateTime? CreationTimeUtc { get; set; }

    public DateTime? LastWriteTimeUtc { get; set; }
}
