namespace Gameplay.Entities.Interactives
{

    public class InteractionEvent : InteractionComponent
    {
        [ReadOnly]
        public string EventTag;

        public override void onStart(PlayerCharacter instigator, bool reverse)
        {
            base.onStart(instigator, reverse);

            if (!reverse)
            {
                owner.TriggerScriptedEvent(EventTag, owner, instigator);

                //TODO: should the event be waited for?
                owner.NextSubAction();
            }
            else
            {
                owner.UntriggerScriptedEvent(EventTag,instigator);
            }

            

        }
    }

}