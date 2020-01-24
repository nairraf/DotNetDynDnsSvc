using System;
using System.IO;
using System.Text;
using System.Web;
using DotNetDynDnsSvc.Model;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DotNetDynDnsSvc.Server
{
    public sealed class ConfigurationManagerSingleton
    {
        // used to track the instance of this singleton, so there is only ever 1 shared instance
        private static readonly ConfigurationManagerSingleton _instance = new ConfigurationManagerSingleton();

        // properties and fields for our yaml needs
        public string ConfigFilePath { get; private set; }
        public string Yaml { get; private set; }

        public ConfigSettings Settings { get; private set; }

        // static property to access the Instance
        public static ConfigurationManagerSingleton Instance
        {
            get { return _instance; }
        }

        // static constructor to help c# make it lazy-ish
        static ConfigurationManagerSingleton() {}

        // the constructor is called only once with the Singleton creates itself
        // this way we load the settings into memory only once for all sessions to use
        // until the app pool is restarted, and then it re-loads.
        private ConfigurationManagerSingleton()
        {
            ConfigFilePath = String.Format(@"{0}\configuration.yaml", Path.GetFullPath(Path.Combine(HttpRuntime.AppDomainAppPath, @"..\Conf")));
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