using Common;
using Gameplay.Entities;
using Gameplay.Skills.Effects;

namespace Gameplay.Skills
{
    public class SkillApplyResult
    {
        public SkillEffect appliedEffect;
        public FSkill appliedSkill;
        public ECharacterStateHealthType changedStat;
        public int damageCaused;
        public int damageResisted;
        public int healCaused;

        public Character skillSource;
        public Character skillTarget;
        public int statChange;

        public SkillApplyResult(Character skillSource, Character skillTarget, FSkill skill)
        {
            this.skillSource = skillSource;
            this.skillTarget = skillTarget;
            appliedSkill = skill;
        }
    }
}