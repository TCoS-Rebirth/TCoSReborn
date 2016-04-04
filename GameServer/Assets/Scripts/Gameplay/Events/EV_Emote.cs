using System;
using Common;
using Gameplay.Entities;

namespace Gameplay.Events
{
    public class EV_Emote : Content_Event
    {
        public EContentEmote Emote;

        public override bool CanExecute(Entity obj, Entity subject)
        {
            if (obj as Character) return true;
            else return false;
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            var charObj = obj as Character;
            charObj.DoEmote(Emote);
        }
    }
}