using Gameplay.Entities;
using Common;

namespace Gameplay.Events
{
    public class EV_UntriggerEvent : Content_Event
    {
        public string EventTag;

        public override bool CanExecute(Entity obj, Entity subject)
        {
            if (obj || subject) return true;
            else return false;
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            if (obj)
            {
                obj.UntriggerScriptedEvent(EventTag, obj);
            }
            else if (subject)
            {

                subject.UntriggerScriptedEvent(EventTag, subject);
            }
        }
    }
}