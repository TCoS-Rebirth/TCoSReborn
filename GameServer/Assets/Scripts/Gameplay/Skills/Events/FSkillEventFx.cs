using System;
using Common;
using Gameplay.Skills.Effects;

namespace Gameplay.Skills.Events
{
    public class FSkillEventFx : FSkill_Event
    {
        [ReadOnly] public bool ComboFinisherOnlyFX;

        [ReadOnly] public EEmitterOverwrite EmitterLocation;

        [ReadOnly] public Client_FX FX;

        public void RunClientEvents(FSkillEventFx ev)
        {
            switch (EmitterLocation)
            {
                case EEmitterOverwrite.EEO_Auto:
                    if (TargetPawn != null)
                        TargetPawn.Skills.sv2clrel_RunEvent(Skill, ev, CF_01_CONSTANT_VALUE, SkillPawn, TriggerPawn, TargetPawn);
                    break;
                case EEmitterOverwrite.EEO_SkillPawn:
                    SkillPawn.Skills.sv2clrel_RunEvent(Skill, ev, CF_01_CONSTANT_VALUE, SkillPawn, SkillPawn, TargetPawn);
                    break;
                case EEmitterOverwrite.EEO_PaintLocation:
                    SkillPawn.Skills.sv2clrel_RunEventL(Skill, ev, CF_01_CONSTANT_VALUE, SkillPawn, SkillPawn, Location, TargetPawn);
                    break;
            }
        }

        public override bool Execute()
        {
            RunClientEvents(this);
            return true;
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