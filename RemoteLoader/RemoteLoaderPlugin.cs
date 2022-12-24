using MelonLoader;
using Newtonsoft.Json;
using RemoteLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonInfo(typeof(RemoteLoaderPlugin), "RemoteLoader", "1.1.0", "AuriRex")]
namespace RemoteLoader
{
    public class RemoteLoaderPlugin : MelonPlugin
    {
        private RemoteLoaderConfig _config;

        public static JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
        };

        private const string CONFIG_FILE_NAME = "RemoteLoaderConfig.json";
        private int _attemptedLoadedModsCount = 0;

        public override void OnPreModsLoaded()
        {
            var configPath = Path.Combine(MelonUtils.UserDataDirectory, CONFIG_FILE_NAME);

            LoadConfig(configPath);

            if((_config.PathsToModsToLoad?.Count ?? 0) == 0)
            {
                LoggerInstance.Warning($"No Mod paths set in {nameof(RemoteLoader)}s config file \"{CONFIG_FILE_NAME}\", this plugin won't do anything!");
                return;
            }

            var assemblies = new List<MelonAssembly>();
            foreach (var path in _config.PathsToModsToLoad)
            {
                ProcessModPath(path, assemblies);
            }

            var melons = new List<MelonMod>();
            foreach(var asm in assemblies)
            {
                asm.LoadMelons();
                foreach(var item in asm.LoadedMelons)
                {
                    if(item is MelonMod mod)
                    {
                        melons.Add(mod);
                    }
                }
            }

            var count = melons.Count();
            MelonBase.RegisterSorted(melons);

            LoggerInstance.Msg($"Loaded {count}/{_attemptedLoadedModsCount} {"Mod".MakePlural(count)}.");
        }

        private void ProcessModPath(string path, List<MelonAssembly> assemblies)
        {
            if(string.IsNullOrWhiteSpace(path))
                return;

            if (!File.Exists(path) && !Directory.Exists(path))
                return;

            FileAttributes attr = File.GetAttributes(path);

            if (attr.HasFlag(FileAttributes.Directory))
            {
                foreach(var file in Directory.EnumerateFiles(path))
                {
                    var asm = LoadModFromPath(file);
                    if (asm != null)
                        assemblies.Add(asm);
                }
            }
            else
            {
                var asm = LoadModFromPath(path);
                if (asm != null)
                    assemblies.Add(asm);
            }
        }

        private MelonAssembly LoadModFromPath(string path)
        {
            if (File.Exists(path))
            {
                if (Path.GetExtension(path)?.ToLower() != ".dll")
                {
                    return null;
                }

                try
                {
                    LoggerInstance.Msg(_attemptedLoadedModsCount % 2 == 0 ? ConsoleColor.DarkMagenta : ConsoleColor.Magenta, $"Loading Mod from path \"{path}\" ...");
                    _attemptedLoadedModsCount++;

                    return MelonLoader.MelonAssembly.LoadMelonAssembly(path, loadMelons: false);
                }
                catch (Exception ex)
                {
                    LoggerInstance.Error($"Failed to load Mod from path \"{path}\":");
                    LoggerInstance.Error($"{ex}: {ex.Message}\n{ex.StackTrace}");
                }
            }
            else
            {
                LoggerInstance.Warning($"No Mod exists at path \"{path}\"!");
            }

            return null;
        }

        private void LoadConfig(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    LoggerInstance.Msg($"Loading config file ... [{path}]");
                    _config = JsonConvert.DeserializeObject<RemoteLoaderConfig>(File.ReadAllText(path), JsonSerializerSettings);
                }
                else
                {
                    SaveConfig(path);
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"{ex}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void SaveConfig(string path)
        {
            _config = new RemoteLoaderConfig();
            LoggerInstance.Msg($"Config doesn't exist, saving default config file ... [{path}]");
            File.WriteAllText(path, JsonConvert.SerializeObject(_config, JsonSerializerSettings));
        }
    }
}
