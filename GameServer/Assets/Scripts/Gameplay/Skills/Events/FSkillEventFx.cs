using System;
using Common;
using Gameplay.Entities;
using Gameplay.Skills.Effects;

namespace Gameplay.Skills.Events
{
    public class FSkillEventFx : FSkill_Event
    {
        [ReadOnly] public bool ComboFinisherOnlyFX;

        bool effectsFired;

        [ReadOnly] public EEmitterOverwrite EmitterLocation;

        [ReadOnly] public Client_FX FX;

        public virtual void FireClientFX()
        {
            switch (EmitterLocation)
            {
                case EEmitterOverwrite.EEO_Auto:
                    if (TargetPawn != null)
                        TargetPawn.skills.RunEvent(Skill, this, 0, SkillPawn, TriggerPawn, TargetPawn);
                    break;
                case EEmitterOverwrite.EEO_SkillPawn:
                    SkillPawn.skills.RunEvent(Skill, this, 0, SkillPawn, SkillPawn, TargetPawn);
                    break;
                case EEmitterOverwrite.EEO_PaintLocation:
                    SkillPawn.skills.RunEventL(Skill, this, 0, SkillPawn, SkillPawn, Location, TargetPawn);
                    break;
            }
        }

        public override bool Execute()
        {
            if (ElapsedTime > Delay)
            {
                FireClientFX();
                return true;
            }
            return false;
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