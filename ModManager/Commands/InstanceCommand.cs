using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using ModManager.Data;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ModManager.Commands
{
    [Command("instance init", Description = "Initialize this game instance.")]
    public class InstanceInitCommand : ICommand
    {
        [CommandParameter(0, Description = "Version of this game instance.")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Version { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public ValueTask ExecuteAsync(IConsole console)
        {
            var instance = new Instance(Version);
            try
            {
                instance.ToFile("mmm.json");
            }
            catch (IOException e)
            {
                throw new CommandException("Could not initialize instance. Cannot write file \"mmm.json\"", e);
            }
            catch (Exception e)
            {
                throw new CommandException("Could not initialize instance.", e);
            }

            return default;
        }
    }

    [Command("instance version", Description = "Changes this instance's game version.")]
    public class InstanceVersionCommand : ICommand
    {
        [CommandParameter(0, Description = "Version of this game instance.")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Version { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public ValueTask ExecuteAsync(IConsole console)
        {
            try
            {
                var instance = Instance.FromFile("mmm.json");
                instance.GameVersion = Version;
                instance.ToFile("mmm.json");
            }
            catch (FileNotFoundException e)
            {
                throw new CommandException("Could not find file \"mmm.json\".  Did you forget to initialize?", e);
            }
            catch (IOException e)
            {
                throw new CommandException("Could not initialize instance. Cannot write file \"mmm.json\"", e);
            }
            catch (Exception e)
            {
                throw new CommandException("Could not initialize instance.", e);
            }

            return default;
        }
    }
}