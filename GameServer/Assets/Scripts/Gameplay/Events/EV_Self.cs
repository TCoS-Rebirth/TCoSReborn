namespace Gameplay.Events
{
    public class EV_Self : Content_Event
    {
        public Content_Event SelfAction;
        /*
        function sv_Execute(Game_Pawn aObject, Game_Pawn aSubject)
        {
        SelfAction.sv_Execute(aObject, aObject);
        }
        */
    }
}