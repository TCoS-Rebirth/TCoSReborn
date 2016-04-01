using Database.Static;
using Gameplay.Entities;
using Network;

namespace Gameplay.Events
{
    public class EV_ShowTutorial : Content_Event
    {
        public SBResource Article; //Help_Article

        public override void Execute(PlayerCharacter p)
        {
            base.Execute(p);

            Message m = PacketCreator.S2C_GAME_GUI_SV2CL_SHOWTUTORIAL(Article);
            p.SendToClient(m);
        }
    }
}