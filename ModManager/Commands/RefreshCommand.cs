using CliFx;
using CliFx.Attributes;
using CurseForgeAPI;
using ModManager.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModManager.Commands
{
    [Command("refresh", Description = "Refresh mod metadata.")]
    public class RefreshCommand : ICommand
    {
        public async ValueTask ExecuteAsync(IConsole console)
        {
            var cf = new CurseForgeClient();
            var instance = Instance.FromFile("mmm.json");

            var tasks = new List<Task<IFile>>();

            var dict = new Dictionary<uint, InstalledMod>();

            foreach (var im in instance.InstalledMods)
            {
                tasks.Add(cf.GetFile(im.ModID, im.FileID));
                dict.Add(im.FileID, im);
            }

            var files = await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);

            foreach (var file in files)
            {
                var im = dict[file.ID];
                im.GameVersions = file.GameVersions.ToList();
            }

            instance.ToFile("mmm.json");

            console.Output.WriteLine("Mod metadata refreshed.");
        }
    }
}