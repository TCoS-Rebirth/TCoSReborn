using System;
using Gameplay.Entities;

namespace Gameplay.Events
{
    public class EV_RemoveItem : Content_Event
    {
        public Content_Inventory Items;

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