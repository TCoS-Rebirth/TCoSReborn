using System;
using System.Collections.Generic;
using Common;
using Gameplay.Skills.Effects;

namespace Gameplay.Skills.Events
{
    public class FSkillEventFxAdvanced : FSkillEventFx
    {

        [ReadOnly] public List<AdvancedEmitter> Emitters = new List<AdvancedEmitter>();

        //TODO do these need to be synced?

        [Serializable]
        public class AdvancedEmitter
        {
            [ReadOnly] public float Delay;

            [ReadOnly] public AudioVisualSkillEffect Emitter;

            [ReadOnly] public EEmitterOverwrite Location;
        }

    }
}