using Common;
using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Skills.Effects
{
    public class SkillEffect : ScriptableObject
    {
        [ReadOnly] public float Aggro;

        [ReadOnly] public ESkillEffectCategory Category;

        [ReadOnly] public string referenceName;

        [ReadOnly] public int resourceID;

        public virtual bool Fire(RunningSkillContext sInfo, Character target)
        {
            return false;
        }
    }
}