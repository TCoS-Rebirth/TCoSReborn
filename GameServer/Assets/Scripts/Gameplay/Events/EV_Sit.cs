using Common;
using Gameplay.Entities;
using UnityEngine;
using Gameplay.Entities.Interactives;
using Network;

namespace Gameplay.Events
{
    public class EV_Sit : Content_Event
    {
        public Vector3 Offset;  //chair to character position offset

        public override bool CanExecute(Entity obj, Entity subject)
        {
            var charSubj = subject as Character;
            if (!charSubj) return false;

            //TODO: if not idle return false - EControllerState
            else return true;
        }

        protected override void Execute(Entity obj, Entity subject)
        {

            /*
            var playerObj = obj as PlayerCharacter;
            if (playerObj)
            {
                playerObj.ReceiveChatMessage("", "Interactive chairs are currently WIP", EGameChatRanges.GCR_SYSTEM);
            }
            */

            //Valshaaran - WIP
            //Interactive Chair objects mostly lack Rotation property
            //will need resolving somehow to enable most chairs
            
            var charObj = obj as Character;
            var ileSubj = subject as InteractiveLevelElement;

            if (ileSubj && ileSubj.ileType == EILECategory.ILE_Chair)
            {
                //Move to chair
                charObj.Move(ileSubj.Position + Offset, ileSubj.Rotation);
                charObj.Sit(true, true);                         
            }
            else
            {
                charObj.Sit(true, false);
            }
            
        }
        /*
* CanExecute() =>
local Actor chairActor;
if (string(Chair) != "None") {                                              //0000 : 07 36 00 7B 39 57 01 C0 60 FF 13 1F 4E 6F 6E 65 00 16
chairActor = FindClosestActor(Class'Actor',aObject,Chair);                //0012 : 0F 00 20 65 FF 13 1B DC 0F 00 00 20 F8 8B C1 00 00 F8 63 FF 13 01 C0 60 FF 13 16
return chairActor != None;                                                //002D : 04 77 00 20 65 FF 13 2A 16
}
if (!Game_Controller(aObject.Controller).IsIdle()) {                        //0036 : 07 5A 00 81 19 2E 10 0E 5B 01 19 00 F8 63 FF 13 05 00 04 01 00 6E 6C 0F 06 00 04 1B 9C 05 00 00 16 16
return False;                                                             //0058 : 04 28
}
return True;
*/
    }
}