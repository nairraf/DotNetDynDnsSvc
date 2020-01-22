using DotNetDynDnsSvc.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DotNetDynDnsSvc.Common
{
    public sealed class ConfigurationManagerSingleton
    {
        // used to track the instance of this singleton, so there is only ever 1 shared instance
        private static readonly ConfigurationManagerSingleton _instance = new ConfigurationManagerSingleton();

        // properties and fields for our yaml needs
        private string _configFile;
        private string _yaml;
        private IDeserializer _deserializer;
        public Settings Settings { get; }

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
            _configFile = String.Format(@"{0}\configuration.yaml", Path.GetFullPath(Path.Combine(HttpRuntime.AppDomainAppPath, @"..\Conf")));
            // read the yaml file without locking
            using (System.IO.FileStream fs = System.IO.File.Open(_configFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] bytes = new byte[fs.Length];
                int bytesToRead = (int)fs.Length;
                fs.Read(bytes, 0, bytesToRead);
                _yaml = Encoding.UTF8.GetString(bytes);
            }
            // deserialize our Yaml to our settings object for easy access
            _deserializer = new DeserializerBuilder().Build();
            Settings = _deserializer.Deserialize<Settings>(_yaml);
        }
    }
}