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
    }
    public class PianoKey
    {
        public string Key { get; set; }
        public string Note { get; set; }
    }
}
