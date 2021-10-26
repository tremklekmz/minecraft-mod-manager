using CliFx;
using CliFx.Attributes;
using CurseForgeAPI;
using ModManager.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ModManager.Commands
{
    [Command("suggest", Description = "Suggest mods based on installed ones.")]
    public class SuggestCommand : ICommand
    {
        public async ValueTask ExecuteAsync(IConsole console)
        {
            var instance = Instance.FromFile("mmm.json");

            var cf = new CurseForgeClient();

            var files = await Task.WhenAll(instance.InstalledMods.Select(im => cf.GetFile(im.ModID, im.FileID)).ToArray()).ConfigureAwait(false);

            foreach (var sugg in files.SelectMany(file => file.Dependencies.Where(dep => dep.DependencyType == DependencyType.Optional)).Select(dep => dep.Mod).Distinct())
            {
                if (instance.InstalledMods.All(mod => sugg.ID != mod.ModID))
                    console.Output.WriteLine($"{sugg.Name} ({sugg.ID}) - {sugg.Summary}");
            }
        }
    }
}