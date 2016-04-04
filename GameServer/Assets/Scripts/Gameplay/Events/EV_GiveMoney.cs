using System;
using Gameplay.Entities;

namespace Gameplay.Events
{
    public class EV_GiveMoney : Content_Event
    {
        public int Amount;

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