using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Skills
{
    public class SkillContext
    {
        Vector3 cameraPosition;
        Character caster;

        public float currentSkillTime;
        Character preferedTarget;
        FSkill skill;
        int sourceItemID;
        float startTime;
        Vector3 targetPosition;

        public SkillContext(FSkill s, Character caster, Vector3 position, Character focusedTarget, float time, int itemID = -1)
        {
            this.caster = caster;
            skill = s;
            startTime = time;
            targetPosition = position;
            preferedTarget = focusedTarget;
            sourceItemID = itemID;
        }

        public SkillContext(FSkill s, Character caster, Vector3 position, Vector3 camPos, Character focusedTarget, float time, int itemID = -1)
        {
            this.caster = caster;
            skill = s;
            startTime = time;
            cameraPosition = camPos;
            targetPosition = position;
            preferedTarget = focusedTarget;
            sourceItemID = itemID;
        }

        public Character Caster
        {
            get { return caster; }
        }

        public FSkill ExecutingSkill
        {
            get { return skill; }
        }

        public float StartTime
        {
            get { return startTime; }
        }

        public Vector3 TargetPosition
        {
            get { return targetPosition; }
        }

        public Character PreferedTarget
        {
            get { return preferedTarget; }
        }

        public int SourceItemID
        {
            get { return sourceItemID; }
        }

        public Vector3 CameraPosition
        {
            get { return cameraPosition; }
        }

        public bool IsInCurrentTimeSpan(float time)
        {
            return time <= currentSkillTime;
        }

        public void Cleanup()
        {
            skill = null;
        }
    }
}