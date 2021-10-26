using CurseForgeAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModManager.Data
{
    internal class InstalledMod
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; } = "";

        [JsonProperty("addonId")]
        public uint ModID { get; set; } = 0;

        [JsonProperty("modName")]
        public string ModName { get; set; } = "";

        [JsonProperty("fileId")]
        public uint FileID { get; set; } = 0;

        [JsonProperty("fileDate")]
        public DateTime FileDate { get; set; } = new DateTime(0);

        [JsonProperty("gameVersions")]
        public List<string> GameVersions { get; set; } = new List<string>();

        [JsonProperty("ignoreWrongVersion")]
        public bool IgnoreWrongVersion { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; } = "";

        public InstalledMod()
        {
        }

        public InstalledMod(uint ModID, string ModName, uint FileID, string FileName, DateTime FileDate, IEnumerable<string> GameVersions, string DisplayName)
        {
            this.ModID = ModID;
            this.ModName = ModName;
            this.FileDate = FileDate;
            this.FileID = FileID;
            this.FileName = FileName;
            this.GameVersions = GameVersions.ToList();
            this.DisplayName = DisplayName;
        }

        public InstalledMod(IMod mod, IFile file)
        {
            ModID = mod.ID;
            ModName = mod.Name;
            FileDate = file.FileDate;
            FileID = file.ID;
            FileName = file.FileName;
            GameVersions = file.GameVersions.ToList();
            DisplayName = file.DisplayName;
        }

        public bool IsVersion(string version) => GameVersions.Contains(version) || FileName.Contains(version) || DisplayName.Contains(version);
    }
}