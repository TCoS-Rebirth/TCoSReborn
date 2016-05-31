using System;
using Gameplay.Entities;
using Gameplay.Entities.NPCs;
using Database.Static;

namespace Gameplay.Events
{
    public class EV_NPC : Content_Event
    {
        public SBResource NPCRes;
        public Content_Event Action;
        public float Radius;

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