using System.Collections.Generic;
using Gameplay.Skills.Effects;

namespace Gameplay.Events
{
    public class EV_EffectsApply : Content_Event
    {
        public bool ApplyToObject;
        public bool ApplyToSubject;
        public List<int> effectIDs = new List<int>();
        public List<AudioVisualSkillEffect> Effects = new List<AudioVisualSkillEffect>();
        public bool SubjectEffectIsPermanent;
        public string Tag;
        public List<string> temporaryEffectsNames = new List<string>();
    }
}