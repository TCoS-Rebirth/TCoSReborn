using System;
using Gameplay.Entities;

namespace Gameplay.Events
{
    public class EV_SetFaction : Content_Event
    {
        public Taxonomy DesiredFaction;
        public int taxonomyID;
        public string temporaryFactionName;

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