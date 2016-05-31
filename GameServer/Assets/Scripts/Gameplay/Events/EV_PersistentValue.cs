using System;
using Gameplay.Entities;
using Network;

namespace Gameplay.Events
{
    public class EV_PersistentValue : Content_Event
    {
        public int context; //Content_Type
        public int Value;
        public int VariableID;

        public override bool CanExecute(Entity obj, Entity subject)
        {
            if (VariableID < 0) return false;
            if (!(subject as PlayerCharacter)) return false;
            return true;
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            var playerSubj = subject as PlayerCharacter;

            //Update server player     
            //Valshaaran - experimental map ID as context ID (works for tutorial triggers)     
            playerSubj.persistentVars.SetVar((int)playerSubj.ActiveZone.ID, VariableID, Value);

            //Dispatch packet to client
            //Valshaaran - client packet handler gives empty error - looks to be unnecessary
            //Message m = PacketCreator.S2C_GAME_CONTROLLER_SV2CL_SETPERSISTENTVARIABLE(context, Value, VariableID);
            //p.SendToClient(m);
            //TODO: Investigate further
        }
    }
}