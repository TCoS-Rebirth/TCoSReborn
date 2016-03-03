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

        [ReadOnly] public List<FSkill> skills = new List<FSkill>();

        [ReadOnly] public SkillCollectionType type;

        [ContextMenu("SortList")]
        void SortList()
        {
            skills = skills.OrderBy(x => x.name, new AlphanumComparer()).ToList();
        }

        public FSkill FindSkill(string skillName)
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

        public FSkill GetSkill(int id)
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