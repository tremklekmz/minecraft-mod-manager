using CliFx;
using System.Reflection;
using System.Threading.Tasks;

namespace ModManager
{
    internal static class Program
    {
        private static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "dev";

        private static async Task<int> Main() =>
            await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .UseVersionText(Version)
            .UseExecutableName("ModManager")
#if !DEBUG
            .AllowDebugMode(false)
            .AllowPreviewMode(false)
#endif
            .Build()
            .RunAsync();
    }
}