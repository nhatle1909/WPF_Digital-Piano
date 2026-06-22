using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO;
using WPF_Piano.Model;
namespace WPF_Piano.Helper
{
    public class PianoSettings
    {
        private static PianoSettings _Instance;
        public static PianoSettings Instance => _Instance ??= new PianoSettings();

        public event Action? MappingUpdated;

        public IConfiguration Configuration { get; set; }
        public Dictionary<string, string> PianoMapping = new();

        public PianoSettings()
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            Configuration = builder.Build();
            SetPianoMapping();
        }



        #region GetterSetter
        private void SetPianoMapping()
        {
            var pianoMapping = Configuration.GetRequiredSection("RealMappingSettings").Get<List<PianoKey>>();
            if (pianoMapping != null)
            {
                PianoMapping = pianoMapping.ToDictionary(k => k.Key, k => k.Note);
                MappingUpdated?.Invoke(); 
            }
        }
        public List<PianoKey> GetPianoMapping()
        {
            return PianoMapping.Select(kvp => new PianoKey { Key = kvp.Key, Note = kvp.Value }).ToList();
        }
        public string GetNote(string key)
        {
            return PianoMapping[key];
        }
        public string GetKey(string note)
        {
            return PianoMapping.FirstOrDefault(kvp => kvp.Value == note).Key;
        }
        public bool CheckNote(string note)
        {
            return PianoMapping.ContainsValue(note);
        }
        public bool CheckKey(string key)
        {
            return PianoMapping.ContainsKey(key);
        }
        #endregion
        #region SettingsBuilder
        public void SaveConfig()
        {
            string configFilePath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            string projectDir = System.IO.Path.Combine(configFilePath, "..", "..", "..");
            string sourceConfigPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(projectDir, "appsettings.json"));
            var json = System.Text.Json.JsonSerializer.Serialize(
                new
                {
                    RealMappingSettings = PianoMapping.Select(kvp => new PianoKey { Key = kvp.Key, Note = kvp.Value }).ToList()
                },
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
                );
            File.WriteAllText(configFilePath, json);
            File.WriteAllText(sourceConfigPath, json);
        }

        public PianoSettings UpdateMapping(Dictionary<string, string> newMapping)
        {
            PianoMapping = newMapping;

            // Notify the entire app that the layout changed!
            MappingUpdated?.Invoke();

            return this;
        }
        #endregion

    }
}
