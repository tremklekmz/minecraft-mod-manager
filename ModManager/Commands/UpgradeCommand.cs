using CliFx;
using CliFx.Attributes;
using CurseForgeAPI;
using ModManager.Data;
using ModManager.Util;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ModManager.Commands
{
    [Command("up", Description = "Upgrades a mod/mods.")]
    public class UpgradeCommand : ICommand
    {
        [CommandParameter(0, Name = "mod_id", Description = "Id of the mod to upgrade.")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public HashSet<uint> ModIDs { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        [CommandOption("force", 'f', Description = "Download the latest file disregarding the version listed on CurseForge.")]
        public bool Force { get; set; }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            var instance = Instance.FromFile("mmm.json");

            var cf = new CurseForgeClient();

            var skippedMods = new List<IMod>();
            var toUpgrade = new List<IMod>();

            foreach (var id in ModIDs)
            {
                if (await instance.IsModUpgradable(id, cf) || Force)
                    toUpgrade.Add(await cf.GetMod(id));
                else
                    skippedMods.Add(await cf.GetMod(id));
            }

            if (skippedMods.Count > 0)
            {
                console.Output.WriteLine("The following mods will not be upgraded:");
                foreach (var mod in skippedMods)
                    console.Output.Write($"{mod.Name} (id:{mod.ID}), ");
                console.Output.Write("\b\b  ");
                console.Output.Write(console.Output.NewLine);
            }

            if (toUpgrade.Count == 0)
                return;

            console.Output.WriteLine("The following mods will be upgraded:");
            foreach (var mod in toUpgrade)
                console.Output.Write($"{mod.Name} (id:{mod.ID}), ");
            console.Output.Write("\b\b  ");
            console.Output.Write(console.Output.NewLine);

            if (!Util.Util.Confirm(console))
            {
                console.Output.WriteLine("No mods were upgraded.");
                return;
            }

            var upgraded = 0;
            foreach (var mod in toUpgrade)
            {
                var toDl = mod.GetLatestFile(instance.GameVersion, "Fabric");
                if (Force)
                    toDl = mod.GetLatestFile();

                var fileName = instance.InstalledMods.Find(im => im.ModID == mod.ID)?.FileName;
                if (fileName != null)
                    File.Delete($"mods/{fileName}");

                console.Output.Write($"Downloading {toDl.FileName}: ");

                using (var progress = new ProgressBar(console))
                    await Util.Util.DownloadFile(toDl, progress);

                console.Output.WriteLine("Done.");

                instance.InstalledMods.RemoveAll(im => im.ModID == mod.ID);
                instance.InstalledMods.Add(new InstalledMod(mod, toDl));
                ++upgraded;
            }

            instance.ToFile("mmm.json");

            console.Output.WriteLine($"Successfully upgraded {upgraded} mods.");
        }
    }

    [Command("up all", Description = "Upgrades all upgradable mods (you can list those using `list up`).")]
    public class UpgradeAll : ICommand
    {
        [CommandOption("force", 'f', Description = "Download the latest file disregarding the version listed on CurseForge.")]
        public bool Force { get; set; }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            var instance = Instance.FromFile("mmm.json");
            var cf = new CurseForgeClient();

            var upgradable = await instance.GetUpgradableMods(cf);
            if (Force)
                upgradable = instance.InstalledMods;

            var toUpgrade = (await Task.WhenAll(upgradable.Select(im => cf.GetMod(im.ModID)).ToArray()).ConfigureAwait(false)).ToList();

            if (toUpgrade.Count == 0)
            {
                console.Output.WriteLine("Nothing to upgrade.");
                return;
            }

            console.Output.WriteLine("The following mods will be upgraded:");
            foreach (var mod in toUpgrade)
                console.Output.Write($"{mod.Name} (id:{mod.ID}), ");
            console.Output.Write("\b\b  ");
            console.Output.Write(console.Output.NewLine);

            if (!Util.Util.Confirm(console))
            {
                console.Output.WriteLine("No mods were upgraded.");
                return;
            }

            var upgraded = 0;
            foreach (var mod in toUpgrade)
            {
                var toDl = mod.GetLatestFile(instance.GameVersion, "Fabric");
                if (Force)
                    toDl = mod.GetLatestFile();

                var fileName = instance.InstalledMods.Find(im => im.ModID == mod.ID)?.FileName;
                if (fileName != null)
                    File.Delete($"mods/{fileName}");

                console.Output.Write($"Downloading {toDl.FileName}: ");

                using (var progress = new ProgressBar(console))
                    await Util.Util.DownloadFile(toDl, progress);

                console.Output.WriteLine("Done.");

                instance.InstalledMods.RemoveAll(im => im.ModID == mod.ID);
                instance.InstalledMods.Add(new InstalledMod(mod, toDl));
                ++upgraded;
            }

            instance.ToFile("mmm.json");

            console.Output.WriteLine($"Successfully upgraded {upgraded} mods.");
        }
    }
}