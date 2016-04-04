using System;
using Gameplay.Entities;

namespace Gameplay.Events
{
    public class EV_Other : Content_Event
    {
        public Content_Event OtherAction;

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