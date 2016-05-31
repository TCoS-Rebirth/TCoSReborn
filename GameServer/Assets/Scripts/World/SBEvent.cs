using Gameplay.Entities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class SBEvent : ScriptableObject
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
        public float _timer;

        //Flags
        [ReadOnly]
        public bool _timing;
        [ReadOnly]
        public bool _isComplete;
        [ReadOnly]
        public bool _unTriggered;

        //Functions
        [ReadOnly]
        public List<Func<bool>> conditions;
        [ReadOnly]
        public List<Func<bool>> untriggerIf;
        [ReadOnly]
        public Func<bool> onFinish;
        [ReadOnly]
        public Func<bool> onUntrigger;

        public bool IsComplete
        {
            get { return _isComplete; }
        }

        public bool UnTriggered
        {
            get { return _unTriggered; }
        }

        public bool Trigger(Entity e, Character c, string tag = "")
        {
            if (!e || !c) return false;
            if (e.ActiveZone != c.ActiveZone) return false;
            eventZone = c.ActiveZone;
            if (eventZone == null) return false;

            if (tag != null) EventTag = tag;
            other = e;
            instigator = c;
            _isComplete = false;
            _unTriggered = false;
            eventZone.StartEvent(this);
            return true;
        }

        /// <summary>
        /// Stop event without doing onFinish()
        /// </summary>
        public void UnTrigger()
        {
            onUntrigger();
            _unTriggered = true;
            eventZone.StopEvent(this);
        }


        /// <summary>
        /// Do onFinish() if timer up or all conditions met, then stop
        /// </summary>
        public void onZoneUpdate()
        {
            if (instigator.ActiveZone != eventZone)
            {
                UnTrigger();
            }

            if (untriggerIf != null)
            {
                foreach(var cond in untriggerIf)
                {
                    if (cond()) {
                        UnTrigger();
                        return;
                    }
                }
            }

                if (_timing)
            {
                _timer -= Time.deltaTime;
                if (_timer < 0.0f) _timing = false;
            }

            if (conditions != null)
            {   
                //If all conditions met, onFinish()
                foreach (var condition in conditions) {
                    if (!condition()) return;
                }                
                onFinish();
                _isComplete = true;
                eventZone.StopEvent(this);
            }
        }

        public void addCondition(Func<bool> cond)
        {
            if (conditions == null)
            {
                conditions = new List<Func<bool>>();
            }
            conditions.Add(cond);
        }

        public void addUntrigger(Func<bool> cond)
        {
            if (untriggerIf == null)
            {
                untriggerIf = new List<Func<bool>>();
            }
            untriggerIf.Add(cond);
        }

        public void addTimerCondition(float t)
        {
            _timer = t;
            _timing = true;
            addCondition(timerExpired);
        }

        protected bool timerExpired()
        {
            if (!_timing) return true;
            else return false;
        }
    }
}
