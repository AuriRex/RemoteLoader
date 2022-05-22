using MelonLoader;
using Newtonsoft.Json;
using RemoteLoader;
using System;
using System.IO;

[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonInfo(typeof(RemoteLoaderPlugin), "RemoteLoader", "1.0.0", "AuriRex")]
namespace RemoteLoader
{
    public class RemoteLoaderPlugin : MelonPlugin
    {
        private RemoteLoaderConfig _config;

        private static JsonSerializerSettings _jsonSerializerSettings = null;
        public static JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
        };

        private const string kConfigFileName = "RemoteLoaderConfig.json";
        private int coolCountThatMakesConsoleColorGoAlternate = 0;

        public override void OnApplicationEarlyStart()
        {
            var configPath = Path.Combine(MelonUtils.UserDataDirectory, kConfigFileName);

            LoadConfig(configPath);

            if((_config.PathsToModsToLoad?.Count ?? 0) == 0)
            {
                LoggerInstance.Warning($"No Mod paths set in {nameof(RemoteLoader)}s config file \"{kConfigFileName}\", this plugin won't do anything!");
                return;
            }

            foreach(var path in _config.PathsToModsToLoad)
            {
                ProcessModPath(path);
            }
        }

        private void ProcessModPath(string path)
        {
            if(string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            FileAttributes attr = File.GetAttributes(path);

            if (attr.HasFlag(FileAttributes.Directory))
            {
                foreach(var file in Directory.EnumerateFiles(path))
                {
                    LoadModFromPath(file);
                }
            }
            else
            {
                LoadModFromPath(path);
            }
        }

        private void LoadModFromPath(string path)
        {
            if (File.Exists(path))
            {
                if (Path.GetExtension(path)?.ToLower() != ".dll")
                {
                    return;
                }

                try
                {
                    var symbolsPath = Path.ChangeExtension(path, ".pdb");
                    if (!File.Exists(symbolsPath))
                        symbolsPath = null;
                    LoggerInstance.Msg(coolCountThatMakesConsoleColorGoAlternate % 2 == 0 ? ConsoleColor.DarkMagenta : ConsoleColor.Magenta, $"Loading Mod from path \"{path}\" ...");
                    coolCountThatMakesConsoleColorGoAlternate++;
                    MelonHandler.LoadFromFile(path, symbolsPath);
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
