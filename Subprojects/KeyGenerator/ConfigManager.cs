using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetDynDnsSvc.Model;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KeyGenerator
{
    public class ConfigManager
    {
        // properties and fields for our yaml needs
        public string ConfigFilePath { get; private set; }
        public string Yaml { get; private set; }

        public ConfigSettings Settings { get; private set; }


        public ConfigManager(string configFilePath)
        {
            ConfigFilePath = configFilePath;
            if (!File.Exists(ConfigFilePath))
                throw new Exception("YAML Config File Not Found");
            ReloadConfiguration();
        }

        public void ReloadConfiguration()
        {
            using (System.IO.FileStream fs = System.IO.File.Open(ConfigFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] bytes = new byte[fs.Length];
                int bytesToRead = (int)fs.Length;
                fs.Read(bytes, 0, bytesToRead);
                Yaml = Encoding.UTF8.GetString(bytes);
            }
            // deserialize our Yaml to our settings object for easy access
            var deserializer = new DeserializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
            Settings = deserializer.Deserialize<ConfigSettings>(Yaml);
        }
    }
}
