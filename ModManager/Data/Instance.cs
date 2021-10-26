using CliFx.Exceptions;
using CurseForgeAPI;
using ModManager.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ModManager.Data
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class Instance
    {
        [JsonProperty("installedMods")]
        public List<InstalledMod> InstalledMods { get; set; } = new List<InstalledMod>();

        [JsonProperty("gameVersion")]
        public string GameVersion { get; set; } = "1.16.1";

        public Instance(string gameVersion) => GameVersion = gameVersion;

        public IEnumerable<InstalledMod> GetVersionMismatchedMods() => InstalledMods.Where(mod => !mod.IsVersion(GameVersion));

        public async Task<IEnumerable<InstalledMod>> GetUpgradableMods(CurseForgeClient cf)
        {
            var tasks = new List<Task<IMod>>();

            foreach (var instMod in InstalledMods)
            {
                tasks.Add(cf.GetMod(instMod.ModID, true));
            }

            await Task.WhenAll(tasks);

            var outdatedMods = new List<InstalledMod>();

            foreach (var instMod in InstalledMods)
            {
                if (await IsModUpgradable(instMod, cf).ConfigureAwait(false))
                    outdatedMods.Add(instMod);
            }

            return outdatedMods;
        }

        public async Task<bool> IsModUpgradable(InstalledMod? im, CurseForgeClient cf)
        {
            if (im is null)
                return false;
            var mod = await cf.GetMod(im.ModID, true);
            return mod.Files.Exists(file => file.FileDate > im.FileDate && file.IsVersion(GameVersion));
        }

        public Task<bool> IsModUpgradable(uint id, CurseForgeClient cf) => IsModUpgradable(InstalledMods.Find(im => im.ModID == id), cf);

        internal static Instance FromFile(string path)
        {
            var instance = JObject.Parse(File.ReadAllText(path)).ToObject<Instance>();
            if (instance?.InstalledMods is null)
                throw new CommandException("Could not read instance info.");
            return instance;
        }

        public bool IsModInstalled(IMod mod) => IsModInstalled(mod.ID);

        public bool IsModInstalled(uint id) => InstalledMods.Any(mod => mod.ModID == id);

        internal void ToFile(string path) => File.WriteAllText(path, JObject.FromObject(this).ToString());
    }
}