using System.Text.RegularExpressions;

namespace Portfolio.Services;

public sealed class PortfolioUploadStorage(string uploadsRoot)
{
    private static readonly IReadOnlyDictionary<string, string> Kinds = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["projects"] = "projects",
        ["education"] = "education",
        ["profile"] = "profile",
        ["technologies"] = "technologies"
    };

    private static readonly IReadOnlyDictionary<string, string> ContentTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".webp"] = "image/webp"
    };

    public bool TryGetDirectory(string kind, out string directory)
    {
        if (!Kinds.TryGetValue(kind, out var safeKind))
        {
            directory = string.Empty;
            return false;
        }

        directory = Path.GetFullPath(Path.Combine(uploadsRoot, safeKind));
        return IsWithinRoot(directory, uploadsRoot);
    }

    public bool TryResolve(string kind, string fileName, out string filePath, out string contentType)
    {
        filePath = string.Empty;
        contentType = string.Empty;
        if (!TryGetDirectory(kind, out var directory) ||
            !Regex.IsMatch(fileName, @"\A[a-fA-F0-9]{32}\.(?:png|jpe?g|webp)\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            return false;

        var extension = Path.GetExtension(fileName);
        if (!ContentTypes.TryGetValue(extension, out var resolvedContentType))
            return false;
        contentType = resolvedContentType;

        filePath = Path.GetFullPath(Path.Combine(directory, fileName));
        return IsWithinRoot(filePath, directory) && File.Exists(filePath);
    }

    private static bool IsWithinRoot(string path, string root)
    {
        var relative = Path.GetRelativePath(Path.GetFullPath(root), Path.GetFullPath(path));
        return relative != "." && relative != ".." &&
               !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) &&
               !Path.IsPathRooted(relative);
    }
}
