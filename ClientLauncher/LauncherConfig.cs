using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace ClientLauncher
{
    [XmlRoot]
    public class LauncherConfig
    {
        const string FileName = "Launcher.config";

        [XmlElement] public ServerEntry server = new ServerEntry();

        public static bool IsValidAdress(string adress)
        {
            return adress.Length > 0 && adress.Split(':').Length == 2;
        }

        public static LauncherConfig Load()
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, FileName);
            if (!File.Exists(filePath)) return new LauncherConfig();
            var serializer = new XmlSerializer(typeof (LauncherConfig));
            using (var reader = XmlReader.Create(filePath))
            {
                if (serializer.CanDeserialize(reader))
                {
                    return serializer.Deserialize(reader) as LauncherConfig;
                }
            }
            return new LauncherConfig();
        }

        public void Save()
        {
            var serializer = new XmlSerializer(typeof (LauncherConfig));
            using (var streamWriter = new StreamWriter(Path.Combine(Environment.CurrentDirectory, FileName)))
            {
                serializer.Serialize(streamWriter, this);
                streamWriter.Flush();
            }
        }

        public class ServerEntry
        {
            public enum OnlineState
            {
                Untested,
                Online,
                Offline
            }
            [XmlElement] public string Name = "Local";
            [XmlElement] public string Address = "127.0.0.1";
            [XmlElement] public int Port = 22233;
            [XmlElement] public string AdditionalArgs = "";
            [XmlElement] public int InfoPort = -1;
        
            [XmlIgnore]
            public OnlineState State = OnlineState.Untested;

            public override string ToString()
            {
                return string.Format("{0} {1}", Name, AdditionalArgs.Length > 0?" (CmdArgs)":"");
            }
        }
    }
}