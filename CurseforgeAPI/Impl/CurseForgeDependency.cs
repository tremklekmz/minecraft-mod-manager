using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CurseForgeAPI.Impl
{
    internal class CurseForgeDependency : IDependency
    {
        [JsonProperty("addonId")]
        internal uint ModId { get; set; }

        public IMod Mod { get; internal set; }

        [JsonProperty("type")]
        public DependencyType DependencyType { get; internal set; }

        public bool Equals(IDependency other) => other != null && Mod.Equals(other.Mod) && DependencyType == other.DependencyType;
    }
}