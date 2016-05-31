using System;
using Gameplay.Entities;

namespace Gameplay.Events
{
    public class EV_Swap : Content_Event
    {
        public Content_Event SwappedAction;

        public override bool CanExecute(Entity obj, Entity subject)
        {
            if (SwappedAction) return SwappedAction.CanExecute(subject, obj);
            else return false;
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            SwappedAction.TryExecute(subject, obj);
        }
    }
}