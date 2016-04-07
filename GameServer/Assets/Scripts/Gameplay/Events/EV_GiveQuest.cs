using System;
using Gameplay.Entities;
using Database.Static;

namespace Gameplay.Events
{
    public class EV_GiveQuest : Content_Event
    {
        public SBResource quest; //Quest_Type

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