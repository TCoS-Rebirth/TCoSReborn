using System;
using UnityEngine;

namespace Gameplay.Skills
{
    [Serializable]
    public class NPC_SkillDeck : ScriptableObject
    {
        [UnityEngine.Serialization.FormerlySerializedAs("tiers")]
        public SkillDeckTier[] Tiers = new SkillDeckTier[6];


        [Serializable]
        public class SkillDeckTier
        {
            [UnityEngine.Serialization.FormerlySerializedAs("skills")]
            public FSkill_Type[] Skills = new FSkill_Type[5];
        }
    }
}