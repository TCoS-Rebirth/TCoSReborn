using Common;
using Gameplay.Entities;

namespace Gameplay.Skills.Events
{
    public class SkillEventBodySlot : SkillEventTarget
    {
        public override bool Execute(RunningSkillContext context)
        {
            if (HasDelayPassed(context))
            {
                //do things
                return true;
            }
            return false;
        }

        public override void Reset()
        {
            base.Reset();
        }

        public override void DeepClone()
        {
            base.DeepClone();
        }
    }
}