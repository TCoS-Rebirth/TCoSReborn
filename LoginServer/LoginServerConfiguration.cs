using System.Xml.Serialization;

namespace LoginServer
{
    [XmlRoot]
    public class LoginServerConfiguration
    {
        [XmlElement] public string ListenIP = "127.0.0.1";
        [XmlElement] public int ListenPort = 22233;
        [XmlElement] public int PopulationRefreshInterval = 5000;

        [XmlElement] public int ProxyServerListenPort = 22232;
        
        [XmlElement] public string DatabaseAddress = "127.0.0.1";

        [XmlElement] public int DatabasePort = 3306;

        [XmlElement] public string DatabaseUsername = "root";

        [XmlElement] public string DatabasePassword = "root";

        [XmlElement] public string DatabaseName = "Spellborn";

        [XmlElement] public int ServerQueueUpdateInterval = 17;

        [XmlElement] public bool RegistrationAllowed = true;

        [XmlElement] public string Message = @"This is a Test. A message from the server.\\nReplace it by a changelog or similar. (use setmessage xxx in the console window, followed by saveconfig)";
    }
}