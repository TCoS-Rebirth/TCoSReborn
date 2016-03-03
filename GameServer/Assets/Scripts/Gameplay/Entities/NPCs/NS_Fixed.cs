using UnityEngine;

namespace Gameplay.Entities.NPCs
{
    public class NS_Fixed : NPC_StatTable
    {
        [Header("Stats")] public int Body;

        public int Focus;
        public int Hitpoints;
        public int Mind;

        public override int GetFocus(int aLevel)
        {
            return Focus;
        }

        public override int GetMind(int aLevel)
        {
            return Mind;
        }

        public override int GetBody(int aLevel)
        {
            return Body;
        }

        public override int GetHitpointsPerLevel(int aLevel)
        {
            return 0;
        }

        public override int GetBaseHitpoints(int aLevel)
        {
            return Hitpoints;
        }
    }
}