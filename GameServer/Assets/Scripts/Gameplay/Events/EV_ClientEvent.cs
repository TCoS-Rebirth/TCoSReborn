using System;
using Gameplay.Entities;
using Network;

namespace Gameplay.Events
{
    public class EV_ClientEvent : Content_Event
    {
        public string EventTag;

        public override bool CanExecute(Entity obj, Entity subject)
        {
            if (obj as Character || subject as Character) return true;
            else return false;
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            var playerSubj = subject as PlayerCharacter;
            if (playerSubj)
            {
                Message m = PacketCreator.S2C_GAME_PLAYERPAWN_SV2CL_CLIENTSIDETRIGGER(EventTag, playerSubj);
                playerSubj.SendToClient(m);
            }
            else
            {
                var playerObj = obj as PlayerCharacter;
                if (playerObj)
                {
                    Message m = PacketCreator.S2C_GAME_PLAYERPAWN_SV2CL_CLIENTSIDETRIGGER(EventTag, playerObj);
                    playerObj.SendToClient(m);
                }
            }
        }
    }
}