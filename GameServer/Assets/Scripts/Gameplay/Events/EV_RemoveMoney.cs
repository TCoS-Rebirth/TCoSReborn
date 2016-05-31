using System;
using Gameplay.Entities;

namespace Gameplay.Events
{
    public class EV_RemoveMoney : Content_Event
    {
        public int Amount;

        public override bool CanExecute(Entity obj, Entity subject)
        {
            var playerSubj = subject as PlayerCharacter;
            if (!playerSubj) return false;
            if (Amount < 0) return false;
            if (playerSubj.Money < Amount) return false;
            return true;
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            var playerSubj = subject as PlayerCharacter;
            playerSubj.TakeMoney(Amount);
        }
    }
}