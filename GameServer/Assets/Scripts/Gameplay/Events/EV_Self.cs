using System;
using Gameplay.Entities;

namespace Gameplay.Events
{
    public class EV_Self : Content_Event
    {
        public Content_Event SelfAction;

        public override bool CanExecute(Entity obj, Entity subject)
        {
            throw new NotImplementedException();
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            throw new NotImplementedException();
        }
        /*
function sv_Execute(Game_Pawn aObject, Game_Pawn aSubject)
{
SelfAction.sv_Execute(aObject, aObject);
}
*/
    }
}