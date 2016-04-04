using Gameplay.Entities;
using Database.Static;
using Network;
using Gameplay.Conversations;

namespace Gameplay.Events
{
    public class EV_Conversation : Content_Event
    {
        public ConversationTopic Conversation; //Conversation_Topic ref

        public override bool CanExecute(Entity obj, Entity subject)
        {
            if (obj as NpcCharacter && subject as PlayerCharacter && Conversation) return true;
            else return false;
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            var npcObj = obj as NpcCharacter;
            var playerSubj = subject as PlayerCharacter;
            npcObj.NewTopic(playerSubj, Conversation);
        }
    }
}