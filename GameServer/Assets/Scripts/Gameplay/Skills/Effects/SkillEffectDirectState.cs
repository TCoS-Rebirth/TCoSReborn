using Common;
using Gameplay.Entities;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDirectState : SkillEffectDirect
    {
        [ReadOnly] public ECharacterStateHealthType state;

        [ReadOnly] public ValueSpecifier value;

        public override bool Apply(FSkill_Type skill, Character skillPawn, Character targetPawn)
        {
            if (state == ECharacterStateHealthType.ECSTH_Health)
            {
                return false;
            }
            var sap = new SkillApplyResult(skillPawn, targetPawn, skill)
            {
                changedStat = state,
                appliedEffect = this
            };
            if (targetPawn != null)
            {
                sap.statChange = targetPawn.Stats.SetCharacterStat(state, targetPawn.Stats.GetCharacterStat(state) + value.CalculateValue(skill, skillPawn, targetPawn));
                skillPawn.OnStatChangeCaused(sap);
                return true;
            }
            return false;
        }
    }
}