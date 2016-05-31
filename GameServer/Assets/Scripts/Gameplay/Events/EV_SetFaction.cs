using System;
using Gameplay.Entities;
using Database.Static;

namespace Gameplay.Events
{
    public class EV_SetFaction : Content_Event
    {
        public SBResource Taxonomy;

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