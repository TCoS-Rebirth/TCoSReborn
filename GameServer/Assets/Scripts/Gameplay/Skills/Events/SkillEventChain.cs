using Common;
using Gameplay.Entities;
using Gameplay.Skills.Effects;

namespace Gameplay.Skills.Events
{
    public class SkillEventChain : SkillEventTarget
    {
        [ReadOnly] public SkillEvent Event;

        [ReadOnly] public bool FairDistribution;

        [ReadOnly] public float IncreasePerJump;

        [ReadOnly] public float Interval;

        [ReadOnly] public int MaxHitsPerTarget;

        [ReadOnly] public int MaxJumps;

        [ReadOnly] public SkillEffectRange Range;

        //var transient int JumpsLeft;
        //var transient float NextJumpTime;
        //var transient array<Game_Pawn> JumpSources;
        //var transient int EventMode;
        [ReadOnly] public int TargetHitMap;

        [ReadOnly] public int TargetHitSet;

        [ReadOnly] public int TargetsPerJump;

        [ReadOnly] public bool TargetsPerJumpIsPerTarget;

        public override void DeepClone()
        {
            base.DeepClone();
            if (Event != null)
            {
                Event = Instantiate(Event);
                Event.DeepClone();
            }
        }

        public override bool Execute(RunningSkillContext context)
        {
            if (HasDelayPassed(context))
            {
                //DO things
                return true;
            }
            return false;
        }

        public override void Reset()
        {
            base.Reset();
            if (Event != null)
            {
                Event.Reset();
            }
        }
    }
}