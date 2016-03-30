using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Common;
using Database.Dynamic;
using Database.Static;
using Gameplay.Entities;
using Network;
using UnityEditor;
using UnityEngine;
using Gameplay.Entities.Interactives;

namespace World
{
    /// <summary>
    ///     This class acts as a central hub for all World related access points and should only ever be instantiated once
    ///     (kind-of-singleton)
    /// </summary>
    public sealed class GameWorld : MonoBehaviour
    {
        static GameWorld _instance;

#pragma warning disable 649
        [SerializeField] GameConfiguration _gameConfiguration;
#pragma warning restore 649

        ServerConfiguration _serverConfiguration;

        /// <summary>
        ///     The ID of this Universe, assigned by the LoginServer
        /// </summary>
        [ReadOnly] public int UniverseID;

        public static GameWorld Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameWorld>();
                }
                return _instance;
            }
        }

        /// <summary>
        ///     Universe specific settings
        /// </summary>
        public GameConfiguration GameConfig
        {
            get { return _gameConfiguration; }
        }

        public ServerConfiguration ServerConfig
        {
            get { return _serverConfiguration; }
        }

        /// <summary>
        ///     Shortcut to <see cref="WorldServer.Players" /> .Count
        /// </summary>
        public int PlayerPopulation
        {
            get { return _worldServer.Players.Count; }
        }

        /// <summary>
        ///     Tries to get a zone specified by LoadedMapID and returns a fallback if not found (Hawksmouth per default) (
        ///     <see cref="ZoneHandler.DefaultZone" />)
        /// </summary>
        /// <returns>The requested zone or null if not found/the default Zone if <see cref="defaultOnMissing" /> is true</returns>
        public Zone GetZone(MapIDs mapID, bool defaultOnMissing = false)
        {
            return defaultOnMissing ? _zoneHandler.GetZoneOrDefault(mapID) : _zoneHandler.GetZone(mapID);
        }

        /// <summary>
        ///     Shortcut for <see cref="ZoneHandler.Zones" />. Returns a list of all known zones
        /// </summary>
        public ReadOnlyCollection<Zone> GetZones()
        {
            return _zoneHandler.Zones;
        }

        /// <summary>
        ///     Shortcut for <see cref="ZoneHandler.DefaultZone" />
        /// </summary>
        public Zone GetDefaultZone()
        {
            return _zoneHandler.DefaultZone;
        }

        /// <summary>
        ///     Shortcut for <see cref="ZoneHandler.FindPlayerCharacter" />
        /// </summary>
        public PlayerCharacter FindPlayerCharacter(string characterName)
        {
            return _zoneHandler.FindPlayerCharacter(characterName);
        }

        /// <summary>
        ///     Shortcut for <see cref="ZoneHandler.FindTravelDestination" />
        /// </summary>
        public PlayerStart FindTravelDestination(string destinationTag)
        {
            return _zoneHandler.FindTravelDestination(destinationTag);
        }

        /// <summary>
        ///     Shortcut for <see cref="ZoneHandler.TravelPlayer(PlayerCharacter, PlayerStart)" />
        /// </summary>
        public void TravelPlayer(PlayerCharacter p, PlayerStart destination)
        {
            _zoneHandler.TravelPlayer(p, destination);
        }

        /// <summary>
        ///     Shortcut for <see cref="ZoneHandler.InsertPlayer" />
        /// </summary>
        public bool InsertPlayerCharacter(PlayerCharacter p, MapIDs mapID)
        {
            return _zoneHandler.InsertPlayer(p, mapID, true);
        }

        void Start()
        {
            if ((_serverConfiguration = LoadConfigFile()) == null)
            {
                Debug.LogError("Config file not found (default file created");
                Application.Quit();
                return;
            }
            GameData.Get.Initialize(OnGameDataInitialized);
        }

        /// <summary>
        ///     Loads and returns (or null) the ServerConfiguration from the file given by
        ///     <see cref="GameConfiguration.ServerDefaults.ConfigFilePath" />
        ///     Creates a default file, if it doesn't exist.
        /// </summary>
        public ServerConfiguration LoadConfigFile()
        {
            var path = Path.GetFullPath(Environment.CurrentDirectory + GameConfig.server.ConfigFilePath);
            if (!File.Exists(path))
            {
                CreateDefaultConfigFile(path);
                return null;
            }
            var serializer = new XmlSerializer(typeof (ServerConfiguration));
            using (var reader = XmlReader.Create(path))
            {
                if (serializer.CanDeserialize(reader))
                {
                    return serializer.Deserialize(reader) as ServerConfiguration;
                }
            }
            return null;
        }

        void CreateDefaultConfigFile(string path)
        {
            var serializer = new XmlSerializer(typeof (ServerConfiguration));
            using (var streamWriter = new StreamWriter(path))
            {
                serializer.Serialize(streamWriter, new ServerConfiguration());
                streamWriter.Flush();
            }
        }

        void OnGameDataInitialized(bool success)
        {
            if (!success)
            {
                Debug.LogError("Not all Data could be initialized properly, canceling start");
                Application.Quit();
            }
            if (!MysqlDb.Initialize())
            {
                Debug.LogError("Database initialization failed. Canceling start");
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            _zoneHandler.Initialize(OnZonesInitialized);
        }

        void OnZonesInitialized()
        {
            _worldServer.StartServer(_serverConfiguration, HandlePlayerLogout);
            Debug.Log("Server running");
        }

        void OnApplicationQuit()
        {
            ShutDown();
        }

        void ShutDown()
        {
            _zoneHandler.ShutDown();
            if (_worldServer != null)
            {
                _worldServer.ShutDown();
                Destroy(_worldServer);
            }
            MysqlDb.CloseConnection();
        }

        void HandlePlayerLogout(PlayerInfo p)
        {
            Debug.Log("Player logged out: " + p.Account.Name + ". Cleaning up");
            //TODO: Handle unable to logout (e.g. check combat state?)
            p.Account.IsOnline = false;
            MysqlDb.AccountDB.UpdateAccount(p.Account);
            if (p.IsIngame && p.ActiveCharacter != null)
            {
                SaveCharacter(p.ActiveCharacter);
                //Notify team of disconnect
                if (p.ActiveCharacter.Team != null)
                {
                    p.ActiveCharacter.Team.Disconnect(p.ActiveCharacter);
                }
                if (p.ActiveCharacter.ActiveZone != null)
                {
                    p.ActiveCharacter.ActiveZone.RemoveFromZone(p.ActiveCharacter);
                    Destroy(p.ActiveCharacter.gameObject);
                }
            }
            p.IsIngame = false;
        }

        void SaveCharacter(PlayerCharacter playerCharacter)
        {
            MysqlDb.CharacterDB.SaveCharacterLogout(playerCharacter);
        }

        /// <summary>
        ///     Reserved for later use (saves all playercharacters to the db cache)
        /// </summary>
        [ContextMenu("Save All PCs")]
        public void SaveAllChars()
        {
            var count = 0;
            foreach (var z in _zoneHandler.Zones)
            {
                foreach (var p in z.Players)
                {
                    SaveCharacter(p);
                    count++;
                }
            }
            Debug.Log("GameWorld.SaveAllChars : saved " + count + " player characters to DB");
        }
#pragma warning disable 649
        [SerializeField] WorldServer _worldServer;

        [SerializeField] ZoneHandler _zoneHandler;
#pragma warning restore 649

#if UNITY_EDITOR
        [ContextMenu("Count Objects")]
        void EditorDebugCountObjects()
        {
            var sb = new StringBuilder();
            sb.AppendLine(FindObjectsOfType<NpcCharacter>().Length + " npcs currently spawned");
            sb.AppendLine(FindObjectsOfType<Transform>().Length + " GameObjects");
            sb.AppendLine(FindObjectsOfType<InteractiveLevelElement>().Length + " InteractiveLevelElements");
            sb.AppendLine(FindObjectsOfType<PlayerCharacter>().Length + " Players");
            Debug.Log(sb.ToString());
        }

        [ContextMenu("Create Default Config file")]
        void EditorCreateDefaultConfigFile()
        {
            var path = Path.GetFullPath(Environment.CurrentDirectory + GameConfig.server.ConfigFilePath);
            CreateDefaultConfigFile(path);
            Debug.Log("Config file created at path: " + path);
        }
#endif

#region RelevanceIDs

        readonly HashSet<int> _allocatedEntityIDs = new HashSet<int>();

        /// <summary>
        ///     Returns a unique Entity ID
        /// </summary>
        public int GetUniqueEntityID()
        {
            var id = 0;
            while (_allocatedEntityIDs.Contains(id))
            {
                id += 1;
            }
            _allocatedEntityIDs.Add(id);
            return id;
        }

        /// <summary>
        ///     Call this for entities that are destroyed to give the ID back to the pool of available IDs
        /// </summary>
        public void ReleaseEntityID(int id)
        {
            _allocatedEntityIDs.Remove(id);
        }

#endregion
    }
}