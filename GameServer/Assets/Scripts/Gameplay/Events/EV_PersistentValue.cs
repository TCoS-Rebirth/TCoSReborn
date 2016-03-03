namespace Gameplay.Events
{
    public class EV_PersistentValue : Content_Event
    {
        public string context; //Content_Type
        public int Value;
        public int VariableID;
    }
}