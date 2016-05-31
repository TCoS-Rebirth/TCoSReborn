using System;
using Gameplay.Entities;
using Gameplay.Skills.Events;

namespace Gameplay.Events
{
    public class EV_SkillEvent : Content_Event
    {
        public SkillEventDuff duffEvent;
        public int duffEventID;
        public string temporaryDuffEventName;

        public override bool CanExecute(Entity obj, Entity subject)
        {
            throw new NotImplementedException();
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            throw new NotImplementedException();
        }
    }
}