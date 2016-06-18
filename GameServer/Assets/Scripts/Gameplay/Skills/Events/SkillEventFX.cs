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

        public virtual void FireClientFX(RunningSkillContext sInfo)
        {
            switch (EmitterLocation)
            {
                case EEmitterOverwrite.EEO_Auto:
                    if (sInfo.PreferedTarget != null)
                        sInfo.PreferedTarget.RunEvent(sInfo, this, sInfo.SkillPawn, sInfo.TriggerPawn, sInfo.PreferedTarget);
                    break;
                case EEmitterOverwrite.EEO_SkillPawn:
                    sInfo.SkillPawn.RunEvent(sInfo, this, sInfo.SkillPawn, sInfo.SkillPawn, sInfo.PreferedTarget);
                    break;
                case EEmitterOverwrite.EEO_PaintLocation:
                    sInfo.SkillPawn.RunEventL(sInfo, this, sInfo.SkillPawn, sInfo.SkillPawn, sInfo.TargetPosition, sInfo.PreferedTarget);
                    break;
            }
        }

        public override bool Execute(RunningSkillContext context)
        {
            if (!HasDelayPassed(context)) return false;
            if (!effectsFired)
            {
                FireClientFX(context);
                effectsFired = true;
            }
            return true;
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