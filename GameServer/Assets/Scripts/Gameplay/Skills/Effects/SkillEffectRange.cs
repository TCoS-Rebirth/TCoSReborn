using Common;
using Gameplay.Entities;
using UnityEngine;
using Utility;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectRange : SkillEffect
    {
        public const int MAX_SKILLSIZE = 200;
        public const int MAX_SPEED = 1000;

        [ReadOnly] public float angle;

        [ReadOnly] public Vector3 locationOffset;

        [ReadOnly] public float maxRadius;

        [ReadOnly] public float minRadius;

        [ReadOnly] public int rotationOffset;

        [ReadOnly] public ETargetSortingMethod sortingMethod;

        public bool IsInRadius(Vector3 sourcePoint, Character target)
        {
            if (target == null)
            {
                return false;
            }
            var minR = minRadius*UnitConversion.UnrUnitsToMeters;
            var maxR = maxRadius*UnitConversion.UnrUnitsToMeters;
            var dist = Vector3.SqrMagnitude(target.Position - sourcePoint);
            return dist >= minR*minR && dist <= maxR*maxR;
        }
    }
}