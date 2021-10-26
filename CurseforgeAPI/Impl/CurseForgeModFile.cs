using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace CurseForgeAPI.Impl
{
    internal class CurseForgeModFile : IFile
    {
        [JsonProperty("id")]
        public uint ID { get; internal set; }

        [JsonProperty("fileName")]
        public string FileName { get; internal set; }

        [JsonProperty("fileDate")]
        public DateTime FileDate { get; internal set; }

        [JsonProperty("releaseType")]
        public ReleaseType ReleaseType { get; internal set; }

        [JsonProperty("downloadUrl")]
        public string DownloadUrl { get; internal set; }

        [JsonProperty("dependencies")]
        internal List<CurseForgeDependency> dependencies;

        [JsonProperty("gameVersion")]
        public ImmutableList<string> GameVersions { get; internal set; }

        [JsonProperty("fileLength")]
        public int FileSize { get; internal set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonIgnore]
        public ImmutableList<IDependency> Dependencies => dependencies.ToImmutableList<IDependency>();

        public bool Equals(IFile other) => ID == other.ID;
    }
}