using Gameplay.Entities;
using Network;

namespace Gameplay.Events
{
    public class EV_PersistentValue : Content_Event
    {
        public int context; //Content_Type
        public int Value;
        public int VariableID;

        public override void Execute(PlayerCharacter p)
        {
            base.Execute(p);

            //Update server player
            p.persistentVars.SetVar(context, VariableID, Value);

            //Dispatch packet to client
            //Valshaaran - client handler gives empty error - looks to be unnecessary
            //Message m = PacketCreator.S2C_GAME_CONTROLLER_SV2CL_SETPERSISTENTVARIABLE(context, Value, VariableID);
            //p.SendToClient(m);
        }
    }
}