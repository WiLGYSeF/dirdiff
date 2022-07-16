using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirDiff.DirMetaSnapshotWriters;

public class DirMetaSnapshotWriterOptions
{
    public bool WriteHash { get; set; } = true;

    public bool WriteHashAlgorithm { get; set; } = false;

    public bool WriteCreatedTime { get; set; } = false;

    public bool WriteLastModifiedTime { get; set; } = true;

    public bool WriteFileSize { get; set; } = true;

    public string Separator { get; set; } = "  ";

    public string NoneValue { get; set; } = "-";
}
