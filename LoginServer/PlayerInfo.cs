using Common;
using Network;

public class PlayerInfo
{
    public PlayerInfo(UserAccount acc, NetConnection con)
    {
        Connection = con;
        Account = acc;
    }

    public NetConnection Connection { get; set; }
    public UserAccount Account { get; set; }
}