using System.Collections.Generic;
using Gameplay.Skills.Events;
using UnityEngine;

namespace Gameplay.Skills
{
    public class SkillEventGroup : ScriptableObject
    {
        public List<SkillEvent> events = new List<SkillEvent>();

        public void DeepClone()
        {
            for (var i = 0; i < events.Count; i++)
            {
                if (events[i] != null)
                {
                    events[i] = Instantiate(events[i]);
                    events[i].DeepClone();
                }
            }
        }

        public void Reset()
        {
            for (var i = 0; i < events.Count; i++)
            {
                if (events[i] != null)
                {
                    events[i].Reset();
                }
            }
        }
    }
}