using System.Xml.Serialization;
using Common;

namespace Network
{
    [XmlRoot]
    public class ServerConfiguration
    {
        [XmlElement] public string PublicIP = "127.0.0.1";

        [XmlElement] public int LoginServerPort = 22233;

        [XmlElement] public int GameServerPort = 22234;

        [XmlElement] public string ServerName = "TCoS-Reborn";

        [XmlElement] public string ServerLanguage = "Any";

        [XmlElement] public string LoginMessage = "Welcome to the TCoSReborn Sandbox. Type '.commands' to get a list of available commands. Expect bugs. Enjoy the exploration.";
    }
}