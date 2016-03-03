namespace Gameplay.Entities.NPCs.Behaviours
{
    public enum ENPCBehaviours
    {
        PathingBehaviour = 0,
        KillerBehaviour = 1,
        GroupBehaviour = 2
    }

    public static class Behaviours
    {
        public static void AddBehaviour<NPCBehaviour>(this NpcCharacter npc, ENPCBehaviours b)
        {
            switch (b)
            {
                case ENPCBehaviours.PathingBehaviour:
                    npc.gameObject.AddComponent<PathingBehaviour>();
                    break;
                case ENPCBehaviours.KillerBehaviour:
                    npc.gameObject.AddComponent<KillerBehaviour>();
                    break;
                case ENPCBehaviours.GroupBehaviour:
                    npc.gameObject.AddComponent<GroupBehaviour>();
                    break;
            }
        }
    }
}