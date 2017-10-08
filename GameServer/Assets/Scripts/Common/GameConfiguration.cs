using System;
using Gameplay;
using UnityEngine;
using World;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Common
{
    public class GameConfiguration : ScriptableObject
    {
        static GameConfiguration _instance;
        public CharacterDefaults character;
        public PlayerDefaults player;
        public ServerDefaults server;
        public WorldDefaults world;

        public int SupportedClientVersion = 28430;

        public static GameConfiguration Get
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameWorld.Instance.GameConfig;
#if UNITY_EDITOR
                    if (_instance == null)
                    {
                        Debug.LogError("GameSettings not present in GameWorld instance");
                        var results = AssetDatabase.FindAssets("t:GameConfiguration");
                        if (results.Length > 0)
                        {
                            _instance = AssetDatabase.LoadAssetAtPath<GameConfiguration>(results[0]);
                        }
                    }
#endif
                }
                return _instance;
            }
        }


        [Serializable]
        public class CharacterDefaults
        {
            public const int MinFame = 1;
            public const int MinPep = 0;

            public const int MaxFame = 50;
            public const int MaxPep = 5;
            public float StatsUpdateInterval = 0.5f;
        }

        [Serializable]
        public class PlayerDefaults
        {
            public int MaxCharactersPerWorld = 7;
            public string PlayerStartDestinationTag = "GAMESTART";
            public int StartBody = 10;
            public Taxonomy StartFaction;
            public int StartFocus = 10;
            public int StartHealth = 100;
            public int StartMind = 10;
            public int StartMoney = 10; //copper 1C*100=1Silver...
            public MapIDs StartZone = MapIDs.PT_HAWKSMOUTH;
        }

        [Serializable]
        public class WorldDefaults
        {
            public float ZoneRelevanceRadius = 4000;
        }

        [Serializable]
        public class ServerDefaults
        {
            public string ConfigFilePath = "/Config.xml";
        }
    }
}