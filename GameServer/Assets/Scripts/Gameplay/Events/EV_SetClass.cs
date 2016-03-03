using Common;

namespace Gameplay.Events
{
    public class EV_SetClass : Content_Event
    {
        public EContentClass DesiredClass;
        /*
        function sv_Execute(Game_Pawn aObject,Game_Pawn aSubject) {
        local export editinline Game_PlayerStats playerStats;
        playerStats = Game_PlayerStats(aSubject.CharacterStats);
        playerStats.sv_SetClass(DesiredClass);
        }
        */
    }
}