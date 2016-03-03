using System.Xml.Serialization;
using Common;

namespace Network
{
    [XmlRoot]
    public class ServerConfiguration
    {
        [XmlElement] public string ServerType = "PVE";
        [XmlElement] public AccountPrivilege AccessRestriction = AccountPrivilege.Player;
        [XmlElement] public string DatabaseIP = "127.0.0.1";

        [XmlElement] public int DatabasePort = 3306;
        [XmlElement] public string DatabaseUsername = "root";
        [XmlElement] public string DatabasePassword = "root";
        [XmlElement] public string DatabaseName = "Spellborn";


        [XmlElement] public string LoginServerIp = "127.0.0.1";

        [XmlElement] public int LoginServerPort = 22232;

        #region GameServer

        [XmlElement] public string ListenIP = "127.0.0.1";

        [XmlElement] public string PublicIP = "127.0.0.1";

        [XmlElement] public int ServerPort = 22234;

        [XmlElement] public string ServerName = "TCoS-Reborn";

        [XmlElement] public string ServerLanguage = "English";

        #endregion
    }
}