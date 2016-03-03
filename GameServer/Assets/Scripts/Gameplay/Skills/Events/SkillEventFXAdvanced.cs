using System;
using System.Collections.Generic;
using Common;
using Gameplay.Entities;
using Gameplay.Skills.Effects;

namespace Gameplay.Skills.Events
{
    public class SkillEventFXAdvanced : SkillEventFX
    {
        bool effectFired;

        [ReadOnly] public List<AdvancedEmitter> Emitters = new List<AdvancedEmitter>();

        public override void Execute(SkillContext sInfo, Character triggerPawn)
        {
            if (!effectFired)
            {
                FireClientFX(sInfo, triggerPawn);
                effectFired = true;
            }
        }

        public override void DeepClone()
        {
            base.DeepClone();
        }

        public override void Reset()
        {
            base.Reset();
            effectFired = false;
        }

        [Serializable]
        public class AdvancedEmitter
        {
            [ReadOnly] public float Delay;

            [ReadOnly] public AudioVisualSkillEffect Emitter;

            [ReadOnly] public EEmitterOverwrite Location;
        }
    }
}