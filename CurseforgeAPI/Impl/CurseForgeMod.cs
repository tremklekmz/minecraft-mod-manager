using Newtonsoft.Json;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace CurseForgeAPI.Impl
{
    internal class CurseForgeMod : IMod
    {
        [JsonProperty("id")]
        public uint ID { get; internal set; }

        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("summary")]
        public string Summary { get; internal set; }

        [JsonIgnore]
        public ImmutableList<IFile> Files { get; internal set; }

        public bool Equals(IMod other) => ID == other.ID;

        public override int GetHashCode() => ID.GetHashCode();

        [JsonIgnore]
        private IFile latestFile;

        public IFile GetLatestFile(string version, string modLoader)
        {
            try
            {
                latestFile ??= Files
                                .Where(
                                        file =>
                                            (file.GameVersions.Contains(version) || file.FileName.Contains(version) || file.DisplayName.Contains(version))
                                            && file.GameVersions.Contains(modLoader)
                                        )
                                .OrderByDescending(file => file.FileDate)
                                .First();
            }
            catch (InvalidOperationException)
            {
                latestFile = GetLatestFile(version);
            }
            return latestFile;
        }

        public IFile GetLatestFile(string version)
        {
            try
            {
                latestFile ??= Files
                                .Where(file => file.GameVersions.Contains(version) || file.FileName.Contains(version) || file.DisplayName.Contains(version))
                                .OrderByDescending(file => file.FileDate)
                                .First();
            }
            catch (InvalidOperationException)
            {
            }
            return latestFile;
        }

        public IFile GetLatestFile() => Files
                                        .OrderByDescending(file => file.FileDate)
                                        .First();
    }
}