using Common;
using Gameplay.Entities;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDirectState : SkillEffectDirect
    {
        [ReadOnly] public ECharacterStateHealthType state;

        [ReadOnly] public ValueSpecifier value;

        public override bool Fire(SkillContext sInfo, Character target)
        {
            if (state == ECharacterStateHealthType.ECSTH_Health)
            {
                return false;
            }
            var sap = new SkillApplyResult(sInfo.Caster, target, sInfo.ExecutingSkill);
            sap.changedStat = state;
            sap.appliedEffect = this;
            if (target != null)
            {
                sap.statChange = target.SetCharacterStat(state, target.GetCharacterStat(state) + value.CalculateValue(sInfo));
                sInfo.Caster.OnStatChangeCaused(sap);
                return true;
            }
            return false;
        }
    }
}