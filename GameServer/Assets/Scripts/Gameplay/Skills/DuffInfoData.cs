using System;
using Gameplay.Entities;
using Gameplay.Skills.Events;

namespace Gameplay.Skills
{
    [Serializable]
    public class DuffInfoData
    {
        public float applyTime;
        public FSkillEventDuff duff;
        public float duration;
        public int stackCount;
        public bool visible;

        public DuffInfoData(FSkillEventDuff duff, float duration, bool visible)
        {
            this.duff = duff;
            this.duration = duration;
            this.visible = visible;
        }

        public void ApplyEffects(Character target)
        {
            //TODO: install hooks for conditionalEvents, apply statmods etc
        }

        public void RemoveEffects(Character target)
        {
            //TODO uninstall hooks, undo statmods etc
        }

        public void Update()
        {
            //TODO handle regular tick events (healing, damage etc)
        }
    }
}