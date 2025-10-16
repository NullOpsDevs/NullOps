using System.Formats.Tar;

namespace NullOps.Tests.E2ESuite;

public static class TarHelper
{
    public static void CreateTarFromPaths(string rootDirectory, string[] paths, Stream outputStream)
    {
        using var tarWriter = new TarWriter(outputStream, TarEntryFormat.Pax, leaveOpen: true);

        foreach (var path in paths)
        {
            var relativePath = Path.GetRelativePath(rootDirectory, path).Replace('\\', '/');

            if (Directory.Exists(path))
            {
                var entry = new PaxTarEntry(TarEntryType.Directory, relativePath);
                tarWriter.WriteEntry(entry);
            }
            else if (File.Exists(path))
            {
                tarWriter.WriteEntry(path, relativePath);
            }
        }
    }
}