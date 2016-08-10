using System;
using System.Collections.Generic;
using UnityEngine;

namespace World
{

    public class SBUniverse : ScriptableObject
    {
        public SBUniverseRules GameRules;
        public SBWorld EntryWorld;
        public SBPortal EntryPortal;
        public SBWorld LobbyWorld;
        public int MaxPlayers;
        public List<int> LocalizedInstanceNames;
        public List<SBWorld> Worlds;
    }

}
