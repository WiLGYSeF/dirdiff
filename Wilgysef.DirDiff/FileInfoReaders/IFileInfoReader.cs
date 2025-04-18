﻿namespace Wilgysef.DirDiff.FileInfoReaders;

public interface IFileInfoReader
{
    Task<FileInfoResult> GetInfoAsync(string path);
}
