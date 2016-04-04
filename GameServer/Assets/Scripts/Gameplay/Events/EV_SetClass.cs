using System;
using Common;
using Gameplay.Entities;

namespace Gameplay.Events
{
    public class EV_SetClass : Content_Event
    {
        public EContentClass DesiredClass;

        public override bool CanExecute(Entity obj, Entity subject)
        {
            throw new NotImplementedException();
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            throw new NotImplementedException();
        }
        /*
function sv_Execute(Game_Pawn aObject,Game_Pawn aSubject) {
local export editinline Game_PlayerStats playerStats;
playerStats = Game_PlayerStats(aSubject.CharacterStats);
playerStats.sv_SetClass(DesiredClass);
}
*/
    }
}