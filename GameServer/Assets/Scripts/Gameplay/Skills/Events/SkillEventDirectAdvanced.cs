using Gameplay.Entities;

namespace Gameplay.Skills.Events
{
    public class SkillEventDirectAdvanced : SkillEventDirect
    {
        [ReadOnly] public SkillEvent MissEvent;

        [ReadOnly] public SkillEvent ReactionEvent;

        [ReadOnly] public SkillEvent TriggerEvent;

        public override void Execute(SkillContext sInfo, Character triggerPawn)
        {
            base.Execute(sInfo, triggerPawn);
        }

        protected override void OnHitTarget(SkillContext sInfo, Character target)
        {
            base.OnHitTarget(sInfo, target);
            if (ReactionEvent != null)
            {
                ReactionEvent.Execute(sInfo, target);
            }
        }

        protected override void OnMissedTarget(SkillContext sInfo, Character target)
        {
            base.OnMissedTarget(sInfo, target);
            if (MissEvent != null)
            {
                MissEvent.Execute(sInfo, target);
            }
        }

        public override void DeepClone()
        {
            base.DeepClone();
            if (MissEvent != null)
            {
                MissEvent = Instantiate(MissEvent);
                MissEvent.DeepClone();
            }
            if (TriggerEvent != null)
            {
                TriggerEvent = Instantiate(TriggerEvent);
                TriggerEvent.DeepClone();
            }
            if (ReactionEvent != null)
            {
                ReactionEvent = Instantiate(ReactionEvent);
                ReactionEvent.DeepClone();
            }
        }

        public override void Reset()
        {
            base.Reset();
        }
    }
}