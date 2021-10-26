using CurseForgeAPI.Impl;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CurseForgeAPI
{
    public class CurseForgeClient
    {
        private readonly RestClient restClient;
        public const string API_URL = "https://addons-ecs.forgesvc.net";

        internal readonly Dictionary<uint, CurseForgeMod> modCache = new Dictionary<uint, CurseForgeMod>();

        public CurseForgeClient() : this(new RestClient(API_URL))
        {
        }

        public CurseForgeClient(RestClient restClient) => this.restClient = restClient;

        public async Task<IMod> GetMod(uint id, bool includeFiles = false)
        {
            var isCached = false;
            lock (modCache)
            {
                isCached = modCache.ContainsKey(id);
            }
            if (isCached)
            {
                CurseForgeMod result;
                lock (modCache)
                {
                    result = modCache[id];
                }
                if (includeFiles)
                    result.Files = await GetFiles(result).ConfigureAwait(false);
                return result;
            }
            var request = new RestRequest($"/api/v2/addon/{id}", Method.GET);
            var resp = await restClient.ExecuteAsync(request).ConfigureAwait(false);
            var json = resp.StatusCode switch
            {
                HttpStatusCode.OK => resp.Content,
                HttpStatusCode.NotFound => throw new Exception($"Mod {id} not found."),
                _ => throw new Exception($"Sumting went rong! (mod: {id}) Code {(int)resp.StatusCode} - {resp.StatusDescription}")
            };
            var mod = JObject.Parse(json).ToObject<CurseForgeMod>();
            if (mod is null)
                throw new Exception("Parsing went rong!");
            if (includeFiles)
                mod.Files = await GetFiles(mod).ConfigureAwait(false);
            lock (modCache)
                modCache.TryAdd(mod.ID, mod);
            return mod;
        }

        public async Task<ImmutableList<IFile>> GetFiles(uint modId, bool includeOptionalDependencies = false)
        {
            var request = new RestRequest($"/api/v2/addon/{modId}/files", Method.GET);
            var resp = await restClient.ExecuteAsync(request).ConfigureAwait(false);
            var json = resp.StatusCode switch
            {
                HttpStatusCode.OK => resp.Content,
                HttpStatusCode.NotFound => throw new Exception($"Mod {modId} not found."),
                _ => throw new Exception($"Sumting went rong! (mod: {modId}) Code {(int)resp.StatusCode} - {resp.StatusDescription}")
            };
            var files = JArray.Parse(json).ToObject<List<CurseForgeModFile>>();
            if (files is null)
                throw new Exception("Parsing went rong!");
            foreach (var file in files)
            {
                file.dependencies = file.dependencies
                    .Where(dep =>
                      dep.DependencyType == DependencyType.Required
                      || (includeOptionalDependencies && dep.DependencyType == DependencyType.Optional))
                    .ToList();
                foreach (var dep in file.dependencies)
                {
                    dep.Mod = await GetMod(dep.ModId, true).ConfigureAwait(false);
                }
            }
            return files.ToImmutableList<IFile>();
        }

        public Task<ImmutableList<IFile>> GetFiles(IMod mod) => mod.Files?.Count switch
        {
            0 => GetFiles(mod.ID),
            null => GetFiles(mod.ID),
            _ => Task.FromResult(mod.Files),
        };

        public Task<IFile> GetFile(IMod mod, uint fileID) => GetFile(mod.ID, fileID);

        public async Task<IFile> GetFile(uint modID, uint fileID)
        {
            var request = new RestRequest($"/api/v2/addon/{modID}/file/{fileID}", Method.GET);
            var resp = await restClient.ExecuteAsync(request).ConfigureAwait(false);
            var json = resp.StatusCode switch
            {
                HttpStatusCode.OK => resp.Content,
                HttpStatusCode.NotFound => throw new Exception($"File {fileID} not found. (mod: {modID})"),
                _ => throw new Exception($"Sumting went rong! (file: {modID}/{fileID} Code {(int)resp.StatusCode} - {resp.StatusDescription}")
            };
            var file = JObject.Parse(json).ToObject<CurseForgeModFile>();
            if (file is null)
                throw new Exception("Parsing went rong!");
            file.dependencies = file.dependencies
                                        .Where
                                        (
                                            dep =>
                                                dep.DependencyType == DependencyType.Required
                                                || dep.DependencyType == DependencyType.Optional
                                        )
                                        .ToList();
            foreach (var dep in file.dependencies)
            {
                dep.Mod = await GetMod(dep.ModId, true).ConfigureAwait(false);
            }
            return file;
        }
    }
}