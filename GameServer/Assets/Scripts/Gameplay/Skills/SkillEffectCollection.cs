using System;
using System.Collections.Generic;
using System.Linq;
using Common.Internal;
using Gameplay.Skills.Effects;
using UnityEngine;

namespace Gameplay.Skills
{
    public class SkillEffectCollection : ScriptableObject
    {
        [ReadOnly] public List<SkillEffect> effects = new List<SkillEffect>();

        public int GetEffectID(string packageFullName)
        {
            for (var i = 0; i < effects.Count; i++)
            {
                if (effects[i].referenceName.Equals(packageFullName, StringComparison.OrdinalIgnoreCase))
                {
                    return effects[i].resourceID;
                }
            }
            return -1;
        }

        public SkillEffect GetEffect(string packageFullName)
        {
            var partName = "";
            if (packageFullName.Contains("."))
            {
                var parts = packageFullName.Split('.');
                partName = parts[parts.Length - 1];
            }
            for (var i = 0; i < effects.Count; i++)
            {
                if (effects[i].referenceName.Equals(packageFullName, StringComparison.OrdinalIgnoreCase))
                {
                    return effects[i];
                }
                if (effects[i].referenceName.Equals(partName, StringComparison.OrdinalIgnoreCase))
                {
                    return effects[i];
                }
            }
            return null;
        }

        [ContextMenu("SortList")]
        void SortList()
        {
            effects = effects.OrderBy(x => x.name, new AlphanumComparer()).ToList();
        }
    }
}