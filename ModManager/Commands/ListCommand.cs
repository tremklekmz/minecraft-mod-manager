using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CurseForgeAPI;
using ModManager.Data;
using Newtonsoft.Json;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModManager.Commands
{
    [Command("list", Description = "List installed mods.")]
    public class ListCommand : ICommand
    {
        [CommandOption('l', Description = "List additional info about mods.", IsRequired = false)]
        public bool LongInfo { get; set; }

        public ValueTask ExecuteAsync(IConsole console)
        {
            try
            {
                var instance = Instance.FromFile("mmm.json");
                console.Output.WriteLine($"Game version: {instance.GameVersion}.");
                if (instance.InstalledMods.Count == 0)
                {
                    console.Output.WriteLine("No mods are installed.");
                    return default;
                }
                console.Output.WriteLine($"Found {instance.InstalledMods.Count} mods.");
                var differentVersionMods = instance.GetVersionMismatchedMods();
                var longestName = instance.InstalledMods.Max(mod => mod.ModName.Length + mod.ModID.ToString(CultureInfo.InvariantCulture).Length + 3);
                var longestFilename = instance.InstalledMods.Max(mod => mod.FileName.Length);
                foreach (var mod in instance.InstalledMods.OrderBy(mod => mod.ModName))
                {
                    var length = mod.ModName.Length;
                    var sb = new StringBuilder();
                    sb.Append(mod.ModName).Append(" (").Append(mod.ModID).Append(')');
                    if (LongInfo)
                    {
                        sb.Append(' ', longestName - length + 2)
                            .Append(mod.FileName);
                        length = mod.FileName.Length;
                        sb.Append(' ', longestFilename - length + 2)
                            .Append(mod.FileDate.ToString("dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture));
                    }
                    console.Output.Write(sb.ToString());
                    if (differentVersionMods.Contains(mod))
                        console.WithForegroundColor(System.ConsoleColor.Yellow, () => console.Output.Write("  [!]"));
                    console.Output.Write(console.Output.NewLine);
                }
            }
            catch (FileNotFoundException e)
            {
                throw new CommandException("Could not find file \"mmm.json\".  Did you forget to initialize?", e);
            }
            catch (IOException e)
            {
                throw new CommandException("Could not read file \"mmm.json\".", e);
            }
            catch (JsonReaderException e)
            {
                throw new CommandException("\"mmm.json\" is invalid. Please re-initialize this instance and reinstall your mods.", e);
            }
            return default;
        }
    }

    [Command("list up", Description = "List upgradable mods.")]
    public class ListOutdatedCommand : ICommand
    {
        public async ValueTask ExecuteAsync(IConsole console)
        {
            try
            {
                var instance = Instance.FromFile("mmm.json");
                if (instance.InstalledMods.Count == 0)
                {
                    console.Output.WriteLine("No mods are installed.");
                    return;
                }

                var cf = new CurseForgeClient();

                var outdatedMods = (await instance.GetUpgradableMods(cf)).ToList();

                if (outdatedMods.Count == 0)
                {
                    console.Output.WriteLine("Nothing to upgrade.");
                    return;
                }

                console.Output.WriteLine($"Found {outdatedMods.Count} upgradable mods:");
                foreach (var mod in outdatedMods)
                    console.Output.Write($"{mod.ModName} (id:{mod.ModID}), ");
                console.Output.Write("\b\b  ");
                console.Output.Write(console.Output.NewLine);
            }
            catch (FileNotFoundException e)
            {
                throw new CommandException("Could not find file \"mmm.json\".  Did you forget to initialize?", e);
            }
            catch (IOException e)
            {
                throw new CommandException("Could not read file \"mmm.json\".", e);
            }
            catch (JsonReaderException e)
            {
                throw new CommandException("\"mmm.json\" is invalid. Please re-initialize this instance and reinstall your mods.", e);
            }
            return;
        }
    }
}