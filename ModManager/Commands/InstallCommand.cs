using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CurseForgeAPI;
using ModManager.Data;
using ModManager.Extensions;
using ModManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModManager.Commands
{
    [Command("install", Description = "Installs a mod/mods.")]
    public class InstallCommand : ICommand
    {
        [CommandParameter(0, Name = "mod_id", Description = "Curseforge ID of the mod.")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public HashSet<uint> ModIDs { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        [CommandOption("fallback-latest", Description = "If there is no file which matcher version exactly, the latest file is used instead.")]
        public bool DownloadLatest { get; set; }

        private readonly CurseForgeClient cf = new CurseForgeClient();

        public async ValueTask ExecuteAsync(IConsole console)
        {
            var instance = Instance.FromFile("mmm.json");

            var tasks = new List<Task<IMod>>();
            foreach (var id in ModIDs)
            {
                tasks.Add(cf.GetMod(id, true));
            }
            await Task.WhenAll(tasks);

            var mods = tasks.Select(task => task.Result).ToHashSet();

            int oldsize;
            do
            {
                oldsize = mods.Count;
                foreach (var mod in mods.ToArray())
                {
                    try
                    {
                        var deps = mod.GetLatestFile(instance.GameVersion, "Fabric")
                                    ?.Dependencies
                                    .Where(dep => dep.DependencyType == DependencyType.Required)
                                    .Select(dep => dep.Mod);
                        if (deps is null)
                        {
                            deps = mod.GetLatestFile()
                                    ?.Dependencies
                                    .Where(dep => dep.DependencyType == DependencyType.Required)
                                    .Select(dep => dep.Mod);
                        }
                        if (deps is null)
                            continue;
                        mods.UnionWith(deps);
                    }
                    catch (Exception e)
                    {
#if DEBUG
                        throw;
#endif
                        throw new CommandException($"Error resolving dependencies for {mod.Name}.", e);
                    }
                }
            } while (oldsize < mods.Count);

            var toInstall = mods.Where
                (
                    mod =>
                        (mod.Files.Any(file => file.IsVersion(instance.GameVersion)) || DownloadLatest)
                        && !instance.IsModInstalled(mod)
                ).ToArray();

            var wrongVer = mods.Where(mod => !mod.Files.Any(file => file.IsVersion(instance.GameVersion) && !DownloadLatest)).ToArray();
            var skippedMods = mods.Where(mod => instance.IsModInstalled(mod)).ToArray();

            if (skippedMods.Any())
            {
                console.WithForegroundColor(ConsoleColor.Yellow, () => console.Output.WriteLine("The following mods will not be installed (they already are):"));
                foreach (var mod in skippedMods)
                    console.Output.Write($"{mod.Name} (id:{mod.ID}), ");
                console.Output.Write("\b\b  ");
                console.Output.Write(console.Output.NewLine);
            }

            if (wrongVer.Any())
            {
                console.WithForegroundColor(ConsoleColor.Yellow,
                                                () => console
                                                        .Output
                                                        .WriteLine($"The following mods will not be installed (they do not support version {instance.GameVersion}):"));
                foreach (var mod in wrongVer)
                    console.Output.Write($"{mod.Name} (id:{mod.ID}), ");
                console.Output.Write("\b\b  ");
                console.Output.Write(console.Output.NewLine);
            }

            if (!toInstall.Any())
                return;
            console.WithForegroundColor(ConsoleColor.Green,
                                                () => console.Output.WriteLine("The following mods will be installed:"));
            foreach (var mod in toInstall)
            {
                console.Output.WriteLine($"{mod.Name} (id:{mod.ID}) - {mod.Summary}");
            }

            if (!Util.Util.Confirm(console))
            {
                console.Output.WriteLine("No mods were installed.");
                return;
            }

            var installed = 0;
            foreach (var mod in toInstall)
            {
                var toDl = mod.GetLatestFile(instance.GameVersion, "Fabric");
                if (DownloadLatest)
                    toDl = mod.GetLatestFile();

                console.Output.Write($"Downloading {toDl.FileName}: ");

                using (var progress = new ProgressBar(console))
                    await Util.Util.DownloadFile(toDl, progress);

                console.Output.WriteLine("Done.");

                instance.InstalledMods.Add(new InstalledMod(mod, toDl));
                ++installed;
            }

            instance.ToFile("mmm.json");

            console.Output.WriteLine($"Successfuly installed {installed} mods.");
        }
    }
}