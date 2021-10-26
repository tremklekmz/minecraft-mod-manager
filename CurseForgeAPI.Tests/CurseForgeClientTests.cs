using System.Linq;
using Xunit;

namespace CurseForgeAPI.Tests
{
    public class CurseForgeClientTests
    {
        private readonly CurseForgeClient client;

        public CurseForgeClientTests()
        {
            client = new CurseForgeClient();
        }

        [Fact]
        public void GetMod()
        {
            var REI = client.GetMod(310111).Result;
            Assert.Equal<uint>(310111, REI.ID);
        }

        [Fact]
        public void FilesNotEmpty()
        {
            var REI = client.GetMod(310111, includeFiles: true).Result;
            Assert.Equal<uint>(310111, REI.ID);
            Assert.NotEmpty(REI.Files);
        }

        [Fact]
        public void FilesHaveAFabricVersion()
        {
            var REI = client.GetMod(310111, includeFiles: true).Result;
            Assert.Equal<uint>(310111, REI.ID);
            Assert.Contains(REI.Files, (file) => file.GameVersions.Contains("Fabric"));
        }

        [Fact]
        public void GetFiles()
        {
            var files = client.GetFiles(310111).Result;
            Assert.NotEmpty(files);
            Assert.NotNull(files[0].FileName);
        }

        [Fact]
        public void OptionalDependecies()
        {
            var files = client.GetFiles(354231, includeOptionalDependencies: true).Result;
            Assert.Contains(files, file => file.Dependencies.Any(dep => dep.DependencyType == DependencyType.Optional));
        }
    }
}