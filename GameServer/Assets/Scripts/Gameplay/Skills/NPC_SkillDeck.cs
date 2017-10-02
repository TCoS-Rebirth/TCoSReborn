using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Skills
{
    [Serializable]
    public class NPC_SkillDeck : ScriptableObject
    {
        [UnityEngine.Serialization.FormerlySerializedAs("tiers")]
        public List<SkillDeckTier> Tiers = new List<SkillDeckTier>();


        [Serializable]
        public class SkillDeckTier
        {
            [UnityEngine.Serialization.FormerlySerializedAs("skills")]
            public FSkill_Type[] Skills = new FSkill_Type[5];
        }
    }
}