using System;
using System.Collections.Immutable;

namespace CurseForgeAPI
{
    public interface IMod : IEquatable<IMod>
    {
        public uint ID { get; }
        public string Name { get; }
        public string Summary { get; }
        public ImmutableList<IFile> Files { get; }

        public IFile GetLatestFile(string version, string modLoader);

        public IFile GetLatestFile(string version);

        public IFile GetLatestFile();
    }
}