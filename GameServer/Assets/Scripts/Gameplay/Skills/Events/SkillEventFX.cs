using System;
using Common;
using Gameplay.Entities;
using Gameplay.Skills.Effects;

namespace Gameplay.Skills.Events
{
    public class SkillEventFX : SkillEvent
    {
        [ReadOnly] public bool ComboFinisherOnlyFX;

        bool effectsFired;

        [ReadOnly] public EEmitterOverwrite EmitterLocation;

        [ReadOnly] public Client_FX FX;

        public virtual void FireClientFX(SkillContext sInfo, Character target)
        {
            switch (EmitterLocation)
            {
                case EEmitterOverwrite.EEO_Auto:
                    target.RunEvent(sInfo, this, sInfo.Caster, target, target);
                    break;
                case EEmitterOverwrite.EEO_SkillPawn:
                    sInfo.Caster.RunEvent(sInfo, this, sInfo.Caster, sInfo.Caster, target);
                    break;
                case EEmitterOverwrite.EEO_PaintLocation:
                    sInfo.Caster.RunEventL(sInfo, this, sInfo.Caster, sInfo.Caster, sInfo.TargetPosition, target);
                    break;
            }
        }

        public override void Execute(SkillContext sInfo, Character triggerPawn)
        {
            //if (!sInfo.IsInCurrentTimeSpan(Delay)) { return; }
            if (!effectsFired)
            {
                FireClientFX(sInfo, triggerPawn);
                effectsFired = true;
            }
        }

        public override void DeepClone()
        {
            base.DeepClone();
        }

        public override void Reset()
        {
            base.Reset();
            effectsFired = false;
        }

        [Serializable]
        public class Client_FX
        {
            [ReadOnly] public AudioVisualSkillEffect CameraShake;

            [ReadOnly] public AudioVisualSkillEffect Emitter;

            [ReadOnly] public AudioVisualSkillEffect Sound;
        }
    }
}