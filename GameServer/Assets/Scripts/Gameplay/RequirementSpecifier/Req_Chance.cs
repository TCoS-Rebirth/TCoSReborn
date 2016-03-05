using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.RequirementSpecifier
{
    public class Req_Chance : Content_Requirement
    {
        public float Chance;

        public override bool isMet(PlayerCharacter p)
        {
            return isMet();
        }

        public override bool isMet(NpcCharacter n)
        {
            return isMet();
        }

        public bool isMet()
        {
            if (Chance == 0.0f)
                return false;
            if (Chance == 1.0f)
                return true;

            var rand = Random.Range(0.0f, 1.0f);
            Debug.Log("Req_Chance.isMet() : (0.0 - 1.0) - Rolled " + rand);
            if (rand <= Chance)
            {
                return true;
            }
            return false;
        }
    }
}