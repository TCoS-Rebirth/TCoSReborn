using System;
using Database.Static;
using Gameplay.Entities;
using Network;

namespace Gameplay.Events
{
    public class EV_ShowTutorial : Content_Event
    {
        public SBResource Article; //Help_Article

        public override bool CanExecute(Entity obj, Entity subject)
        {
            if (!(subject as PlayerCharacter)) return false;
            else return true;

        }

        protected override void Execute(Entity obj, Entity subject)
        {
            var playerSubj = subject as PlayerCharacter;
            Message m = PacketCreator.S2C_GAME_GUI_SV2CL_SHOWTUTORIAL(Article);
            playerSubj.SendToClient(m);
        }
    }
}