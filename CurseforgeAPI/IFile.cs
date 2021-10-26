using System;
using System.Collections.Immutable;

namespace CurseForgeAPI
{
    public interface IFile : IEquatable<IFile>
    {
        public uint ID { get; }
        public string FileName { get; }
        public DateTime FileDate { get; }
        public ReleaseType ReleaseType { get; }
        public string DownloadUrl { get; }
        public ImmutableList<IDependency> Dependencies { get; }
        public ImmutableList<string> GameVersions { get; }
        public string DisplayName { get; set; }
        public int FileSize { get; }
    }
}