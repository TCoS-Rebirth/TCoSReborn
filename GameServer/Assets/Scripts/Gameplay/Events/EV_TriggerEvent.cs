using Common;
using Gameplay.Entities;

namespace Gameplay.Events
{
    public class EV_TriggerEvent : Content_Event
    {
        public string EventTag;

        public override void Execute(PlayerCharacter p)
        {
            base.Execute(p);

            //TODO: Not yet implemented
            p.ReceiveChatMessage("", "Event with tag " + EventTag + " is here, but not yet implemented :(", EGameChatRanges.GCR_SYSTEM);

        }
    }
}