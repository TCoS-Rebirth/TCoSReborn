using UnityEngine;

namespace World
{
    public class SBRoute : MonoBehaviour
    {
        public bool AllowRentACabin;

        public int CabinCost;

        public int CrewCost;

        public SBWorldPortal DestinationPortal;

        public Zone DestinationWorld;
        public string ShardName;

        public SBWorldPortal TravelPortal;

        public Zone TravelWorld;

        public string WorldPortalTag;
    }
}