using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Skills
{
    public class RunningSkillContext
    {
        public readonly Vector3 CameraPosition;
        public readonly Character SkillPawn;

        public float CurrentEventStart;
        public readonly float Duration;
        public Character PreferedTarget;
        public Character TriggerPawn;
        public readonly FSkill_Type ExecutingSkill;
        public readonly int SourceItemID;
        public readonly float StartTime;
        public readonly Vector3 TargetPosition;

        public bool Aborted;

        public RunningSkillContext(FSkill_Type s, float duration, Character skillPawn, Vector3 position, Character focusedTarget, float time, int itemID = -1)
        {
            SkillPawn = skillPawn;
            ExecutingSkill = s;
            Duration = duration;
            StartTime = time;
            TargetPosition = position;
            PreferedTarget = focusedTarget;
            SourceItemID = itemID;
            Aborted = false;
        }

        public RunningSkillContext(FSkill_Type s, float duration, Character skillPawn, Vector3 position, Vector3 camPos, Character focusedTarget, float time, int itemID = -1)
        {
            SkillPawn = skillPawn;
            ExecutingSkill = s;
            Duration = duration;
            StartTime = time;
            CameraPosition = camPos;
            TargetPosition = position;
            PreferedTarget = focusedTarget;
            SourceItemID = itemID;
            Aborted = false;
        }

        public bool IsDelayDone(float delay)
        {
            return Time.time - CurrentEventStart > delay;
        }

        public float GetCurrentSkillTime()
        {
            return Time.time - StartTime;
        }

    }
}