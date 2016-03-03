namespace Gameplay.Events
{
    public class EV_QuestProgress : Content_Event
    {
        public int Progress;
        public string quest; //Quest_Type
        public int TargetNr;
    }
}