namespace Gameplay.Entities.Interactives
{

    public class InteractionShow : InteractionComponent
    {
        [ReadOnly]
        public bool Show;

        public override void onStart(PlayerCharacter instigator, bool reverse)
        {
            base.onStart(instigator, reverse);
            if (owner)
            {
                if (!reverse)
                {
                    owner.SetShow(Show);
                    owner.NextSubAction();
                }
                else
                {
                    owner.SetShow(!Show);
                }
            }
        }
    }
}