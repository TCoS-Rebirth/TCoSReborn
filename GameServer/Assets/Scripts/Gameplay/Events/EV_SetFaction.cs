namespace Gameplay.Events
{
    public class EV_SetFaction : Content_Event
    {
        public Taxonomy DesiredFaction;
        public int taxonomyID;
        public string temporaryFactionName;
    }
}