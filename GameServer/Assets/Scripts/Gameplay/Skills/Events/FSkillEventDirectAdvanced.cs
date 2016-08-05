using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Skills.Events
{
    public class FSkillEventDirectAdvanced : FSkillEventDirect
    {
        [ReadOnly] public FSkill_Event MissEvent;

        [ReadOnly] public FSkill_Event ReactionEvent;

        [ReadOnly] public FSkill_Event TriggerEvent;

        //protected override void OnHitTarget(RunningSkillContext sInfo, Character target)
        //{
        //    base.OnHitTarget(sInfo, target);
        //    if (ReactionEvent != null)
        //    {
        //        //Debug.Log(string.Format("[{0}] - executing OnHitTarget (reactionEvent) in {1} of {2}", Time.time, this, sInfo.ExecutingSkill));
        //        ReactionEvent.Execute(sInfo);
        //    }
        //}

        //protected override void OnMissedTarget(RunningSkillContext sInfo, Character target)
        //{
        //    base.OnMissedTarget(sInfo, target);
        //    if (MissEvent != null)
        //    {
        //        //Debug.Log(string.Format("[{0}] - executing OnMissedTarget (missevent) in {1} of {2}", Time.time, this, sInfo.ExecutingSkill));
        //        MissEvent.Execute(sInfo);
        //    }
        //}

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
            if (MissEvent != null)
            {
                MissEvent.Reset();
            }
            if (ReactionEvent != null)
            {
                ReactionEvent.Reset();
            }
            if (TriggerEvent != null)
            {
                TriggerEvent.Reset();
            }
        }
    }
}