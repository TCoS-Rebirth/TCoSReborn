using System;
using Gameplay.Entities;
using Gameplay.Entities.NPCs;

namespace Gameplay.Events
{
    public class EV_NPC : Content_Event
    {
        public NPC_Type NPC;
        public Content_Event NPCAction;
        public int npcID;
        public float Radius;
        public string temporaryNPCname;

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