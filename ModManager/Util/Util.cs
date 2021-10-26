using CliFx;
using CurseForgeAPI;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ModManager.Util
{
    internal static class Util
    {
        internal static Task DownloadFile(IFile file, ProgressBar ticker)
        {
            var wc = new WebClient();

            wc.DownloadProgressChanged += (sender, e) => ticker.Report(e.BytesReceived / (double)e.TotalBytesToReceive);
            wc.DownloadFileCompleted += (sender, e) => ticker.Report(1);
            return wc.DownloadFileTaskAsync(file.DownloadUrl, $"mods/{file.FileName}");
        }

        internal static bool Confirm(IConsole console)
        {
            console.Output.Write("Type [y]es to continue: ");
            var confirm = console.Input.ReadLine();
            return string.Equals(confirm, "y", StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(confirm, "yes", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}