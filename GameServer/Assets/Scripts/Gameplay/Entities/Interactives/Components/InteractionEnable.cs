namespace Gameplay.Entities.Interactives
{

    public class InteractionEnable : InteractionComponent
    {
        [ReadOnly]
        public bool Enabled;

        public override void onStart(PlayerCharacter instigator, bool reverse)
        {
            base.onStart(instigator, reverse);

            if (owner)
            {
                if (!reverse)
                {
                    owner.SetEnabled(Enabled);
                    owner.NextSubAction();
                }
                else
                {
                    owner.SetEnabled(!Enabled);
                }
            }
        }
    }
}