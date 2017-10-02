using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class SBWorld : ScriptableObject
    {
        public int worldID;
        public string WorldName;
        public string WorldFile;
        public Base_GameInfo GameInfo;
        public SBWorldRules GameRules;
        public byte WorldType;
        public int InstanceMaxPlayers;
        public int InstanceMaxInstances;
        public int InstanceLingerTime;
        public bool InstanceAutoDestroy;
        public bool FreeToPlayAllowed;
        public List<SBRoute> ExitRoutes = new List<SBRoute>();
        public List<SBPortal> EntryPortals = new List<SBPortal>();
        public List<SBTravel> TravelNPCs = new List<SBTravel>();
        public string LoadingScreenTex;
        public string ExteriorMesh;
        public float WorldWeight;
    }
}