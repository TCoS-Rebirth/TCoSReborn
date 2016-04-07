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

                //TODO : move this to update() when scripted events implemented
                owner.NextSubAction();
            }
            else
            {
                owner.UntriggerScriptedEvent(EventTag, instigator);
            }



        }

        public override void OnZoneUpdate()
        {
            base.OnZoneUpdate();
            //TODO : Go to next subaction when triggered script is complete
        }
    }
}