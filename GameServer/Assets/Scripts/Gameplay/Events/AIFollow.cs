using Gameplay.Entities;
using Gameplay.Entities.NPCs.Behaviours;

namespace Gameplay.Events
{
    public class AIFollow : Content_Event
    {
        public override bool CanExecute(Entity obj, Entity subject)
        {
            if (obj as NpcCharacter && subject as Character) return true;
            else return false;
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            var npcObj = obj as NpcCharacter;
            var charSbj = subject as Character;

            FollowBehaviour follow = npcObj.gameObject.AddComponent<FollowBehaviour>();
            follow.setTarget(charSbj);
            npcObj.AttachBehaviour(follow);
        }
    }
}