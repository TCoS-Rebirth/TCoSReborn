using System;
using Gameplay.Entities;

namespace Gameplay.Events
{
    public class EV_GiveItem : Content_Event
    {
        public Content_Inventory Items;

        public override bool CanExecute(Entity obj, Entity subject)
        {
            var playerSubj = subject as PlayerCharacter;
            if (playerSubj)
            {
                return playerSubj.ItemManager.hasFreeSpace(Items.Items.Count);
            }
            else return false;
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            var playerSubj = subject as PlayerCharacter;
            playerSubj.GiveInventory(Items);
        }
    }
}