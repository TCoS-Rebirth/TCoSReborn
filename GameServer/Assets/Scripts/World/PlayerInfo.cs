using Common;
using Database.Dynamic;
using Gameplay.Entities;
using Network;

namespace World
{
    /// <summary>
    ///     Represents an ingame instance of information for a connected player
    /// </summary>
    public class PlayerInfo
    {
        public UserAccount Account;
        public PlayerCharacter ActiveCharacter;
        public ECharacterCreationState CharacterCreationState = ECharacterCreationState.CCS_PREPARE_UNIVERSE_ENTRY;
        public NetConnection Connection;
        public bool IsIngame;
        public MapIDs LoadedMapID = MapIDs.LOGIN;
        public MapIDs MapIdTransitionTo = MapIDs.LOGIN;

        public PlayerInfo(UserAccount acc, NetConnection con)
        {
            Account = acc;
            Connection = con;
            Connection.player = this;
            IsIngame = false;
        }

        /// <summary>
        ///     Instructs the player client to load a specific map
        /// </summary>
        /// <param name="newMap"></param>
        public void LoadClientMap(MapIDs newMap)
        {
            var m = PacketCreator.S2C_WORLD_PRE_LOGIN(newMap);
            Connection.SendMessage(m);
            MapIdTransitionTo = newMap;
        }

        public bool IsSessionValid()
        {
            var dbKey = DB.AccountDB.GetSessionKey(Account);
            return dbKey != -1 && dbKey == Account.SessionKey;
        }
    }
}