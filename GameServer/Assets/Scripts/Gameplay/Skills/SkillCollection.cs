using System;
using System.Collections.Generic;
using System.Linq;
using Common.Internal;
using UnityEngine;

namespace Gameplay.Skills
{
    public class SkillCollection : ScriptableObject
    {
        public enum SkillCollectionType
        {
            Player,
            NPC,
            Event,
            Combo,
            Item,
            Test
        }

        [ReadOnly] public List<FSkill_Type> skills = new List<FSkill_Type>();

        [ReadOnly] public SkillCollectionType type;

        [ContextMenu("SortList")]
        void SortList()
        {
            skills = skills.OrderBy(x => x.name, new AlphanumComparer()).ToList();
        }

        public FSkill_Type FindSkill(string skillName)
        {
            for (var i = 0; i < skills.Count; i++)
            {
                if (skills[i].name.Equals(skillName, StringComparison.OrdinalIgnoreCase))
                {
                    return skills[i];
                }
            }
            return null;
        }

        public FSkill_Type GetSkill(int id)
        {
            for (var i = 0; i < skills.Count; i++)
            {
                if (skills[i].resourceID == id)
                {
                    return skills[i];
                }
            }
            return null;
        }
    }
}