namespace Gameplay.Events
{
    public class EV_EffectsRemove : Content_Event
    {
        public bool RemoveFromObject;
        public bool RemoveFromSubject;
        public string Tag;
    }
}