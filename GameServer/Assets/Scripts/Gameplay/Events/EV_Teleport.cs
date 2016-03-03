namespace Gameplay.Events
{
    public class EV_Teleport : Content_Event
    {
        public bool IsInstance;
        public string Parameter;
        public int TargetWorld;
    }
}