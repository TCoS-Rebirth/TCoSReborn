using Gameplay.Entities;

namespace Gameplay.Events
{
    public class AIIdle : Content_Event
    {
        public override bool CanExecute(Entity obj, Entity subject)
        {
            if (obj as NpcCharacter) return true;
            else return false;
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            var npcObj = obj as NpcCharacter;
            npcObj.DefaultBehaviour();
        }

    }
}