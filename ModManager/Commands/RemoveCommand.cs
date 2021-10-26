using CliFx;
using CliFx.Attributes;
using ModManager.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ModManager.Commands
{
    [Command("remove", Description = "Removes a mod/mods.")]
    public class RemoveCommand : ICommand
    {
        [CommandParameter(0, Name = "mod_id", Description = "Curseforge ID of the mod.")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public HashSet<uint> ModIDs { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public ValueTask ExecuteAsync(IConsole console)
        {
            var instance = Instance.FromFile("mmm.json");

            var mods = instance.InstalledMods.Where(mod => ModIDs.Contains(mod.ModID)).ToArray();

            console.Output.WriteLine("The following mods will be removed:");
            foreach (var mod in mods)
                console.Output.Write($"{mod.ModName}, ");
            console.Output.Write("\b\b  ");
            console.Output.Write(console.Output.NewLine);

            if (!Util.Util.Confirm(console))
            {
                console.Output.WriteLine("No mods were removed.");
                return default;
            }

            var removed = 0;
            foreach (var mod in mods)
            {
                File.Delete("mods/" + mod.FileName);
                instance.InstalledMods.RemoveAll(im => im.ModID == mod.ModID);
                ++removed;
            }

            instance.ToFile("mmm.json");

            console.Output.WriteLine($"Removed {removed} mods.");

            return default;
        }
    }
}