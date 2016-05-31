
using Gameplay.Entities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class SBScriptedEvent : ScriptableObject
    {
        [ReadOnly]
        public string EventTag;
        [ReadOnly]
        public Zone eventZone;
        [ReadOnly]
        public Entity other;
        [ReadOnly]
        public Character instigator;

        [ReadOnly]
        public List<SBEvent> Stages = new List<SBEvent>();

        [ReadOnly]
        public int _stageInd;

        public Func<bool> onUntrigger;
       
        public bool Trigger(Entity e, Character c)
        {
            if (!e || !c) return false;
            if (e.ActiveZone != c.ActiveZone) return false;
            if (EventTag == "") return false;

            _stageInd = 0;
            other = e;
            instigator = c;
            eventZone.StartScriptedEvent(EventTag, other, instigator);
            Stages[_stageInd].Trigger(other, instigator, EventTag);
            return true;
        }

        public void Untrigger()
        {
            onUntrigger();
            Stages[_stageInd].UnTrigger();
        }

        public void onZoneUpdate()
        {
            var curStage = Stages[_stageInd];
            if (curStage.UnTriggered)
            {
                Untrigger();
                return;
            }

            if (curStage.IsComplete)
            {
                if (_stageInd >= (Stages.Count-1))  //if index at maximum
                {
                    eventZone.StopScriptedEvent(EventTag); //Stop scripted event
                }
                else {  //Otherwise increase index, trigger next subevent
                    _stageInd++;
                    Stages[_stageInd].Trigger(other, instigator, EventTag);
                }    
            }
        }
    }
}
