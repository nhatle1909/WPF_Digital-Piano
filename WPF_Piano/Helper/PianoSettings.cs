using Microsoft.Extensions.Configuration;
using System.IO;
using WPF_Piano.Model;
namespace WPF_Piano.Helper
{
    public class PianoSettings
    {
        private static PianoSettings _Instance;
        public static PianoSettings Instance => _Instance ??= new PianoSettings();

        // 1. Declare the event that other view models will listen to
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

        // 2. Trigger the event when updating configuration layers
        public void UpdateMapping(Dictionary<string, string> newMapping)
        {
            PianoMapping = newMapping;

            // Notify the entire app that the layout changed!
            MappingUpdated?.Invoke();
        }

        private void SetPianoMapping()
        {
            var pianoMapping = Configuration.GetRequiredSection("RealMappingSettings").Get<List<PianoKey>>();
            if (pianoMapping != null)
            {
                PianoMapping = pianoMapping.ToDictionary(k => k.Key, k => k.Note);
                MappingUpdated?.Invoke(); // Fire on initial startup load
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
        public bool CheckNote(string note)
        {
            return PianoMapping.ContainsValue(note);
        }
        public bool CheckKey(string key)
        {
            return PianoMapping.ContainsKey(key);
        }
        //public void UpdateKeyMapping(string key, string note)
        //{
        //    var pianoMapping = Configuration.GetRequiredSection("RealMappingSettings").Get<List<PianoKey>>();
        //    if (pianoMapping != null)
        //    {
        //        var keyToUpdate = pianoMapping.FirstOrDefault(k => k.Key == key);
        //        if (keyToUpdate != null)
        //        {
        //            keyToUpdate.Note = note;
        //            var json = System.Text.Json.JsonSerializer.Serialize(new { RealMappingSettings = pianoMapping }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        //            File.WriteAllText("appsettings.json", json);
        //        }
        //    }
        //}
        //public void CreateKeyMappingProfile(string profileName, Dictionary<string, string> keyMappings)
        //{
        //    var profiles = Configuration.GetRequiredSection("KeyMappingProfiles").Get<Dictionary<string, Dictionary<string, string>>>() ?? new Dictionary<string, Dictionary<string, string>>();
        //    profiles[profileName] = keyMappings;
        //    var json = System.Text.Json.JsonSerializer.Serialize(new { RealMappingSettings = Configuration.GetRequiredSection("RealMappingSettings").Get<List<PianoKey>>(), KeyMappingProfiles = profiles }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        //    File.WriteAllText($"{profileName}.json", json);
        //}
    }
}
