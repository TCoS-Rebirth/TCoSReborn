using Gameplay.Entities;

namespace Gameplay.Events
{
    public class EV_TriggerEvent : Content_Event
    {
        public string EventTag;

        public override bool CanExecute(Entity obj, Entity subject)
        {
            if (obj as Character || subject as Character) return true;
            else return false;
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            var charObj = obj as Character;
            var charSubj = subject as Character;   
            if (charObj)
            {
                obj.TriggerScriptedEvent(EventTag, obj, charObj);
            }
            else if (charSubj)
            {
                subject.TriggerScriptedEvent(EventTag, subject, charSubj);
            }
        }
    }
}