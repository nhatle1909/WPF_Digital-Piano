using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace WPF_Piano
{
    public class PianoSettings
    {
        public IConfiguration Configuration { get; set; }
        public PianoSettings()
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            Configuration = builder.Build();
        }
        public Dictionary<string, string> ReturnPianoMapping()
        {
            var pianoMapping = Configuration.GetRequiredSection("RealMappingSettings").Get<List<PianoKey>>();
            return pianoMapping.ToDictionary(x=>x.Key,x=>x.Note);
        }
        public void UpdateKeyMapping(string key, string note)
        {
            var pianoMapping = Configuration.GetRequiredSection("RealMappingSettings").Get<List<PianoKey>>();
            var keyToUpdate = pianoMapping.FirstOrDefault(k => k.Key == key);
            if (keyToUpdate != null)
            {
                keyToUpdate.Note = note;
                var json = System.Text.Json.JsonSerializer.Serialize(new { RealMappingSettings = pianoMapping }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("appsettings.json", json);
            }
        }
        public void CreateKeyMappingProfile(string profileName, Dictionary<string, string> keyMappings)
        {
            var profiles = Configuration.GetRequiredSection("KeyMappingProfiles").Get<Dictionary<string, Dictionary<string, string>>>() ?? new Dictionary<string, Dictionary<string, string>>();
            profiles[profileName] = keyMappings;
            var json = System.Text.Json.JsonSerializer.Serialize(new { RealMappingSettings = Configuration.GetRequiredSection("RealMappingSettings").Get<List<PianoKey>>(), KeyMappingProfiles = profiles }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText($"{profileName}.json", json);
        }
    }
}
    public class PianoKey
    {
        public string Key { get; set; }
        public string Note { get; set; }
    }
  
