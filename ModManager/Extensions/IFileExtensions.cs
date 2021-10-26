using CurseForgeAPI;

namespace ModManager.Extensions
{
    internal static class IFileExtensions
    {
        public static bool IsVersion(this IFile file, string version)
            => file.GameVersions.Contains(version) || file.FileName.Contains(version) || file.DisplayName.Contains(version);
    }
}