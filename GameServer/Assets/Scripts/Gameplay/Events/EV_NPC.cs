using Gameplay.Entities.NPCs;

namespace Gameplay.Events
{
    public class EV_NPC : Content_Event
    {
        public NPC_Type NPC;
        public Content_Event NPCAction;
        public int npcID;
        public float Radius;
        public string temporaryNPCname;
    }
}