using System;
using Gameplay.Entities;

namespace Gameplay.Events
{
    public class EV_EffectsRemove : Content_Event
    {
        public bool RemoveFromObject;
        public bool RemoveFromSubject;
        public string Tag;

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