using Common;

namespace Gameplay.Entities.Interactives
{

    public class InteractionEnableCollision : InteractionComponent
    {
        [ReadOnly]
        public bool EnableCollision;

        public override void onStart(PlayerCharacter instigator, bool reverse)
        {
            base.onStart(instigator, reverse);

            if (owner)
            {
                if (!reverse)
                {
                    owner.SetCollision( (ECollisionType) (EnableCollision ? 1 : 0));
                    owner.NextSubAction();
                }
                else
                {
                    owner.SetCollision((ECollisionType)(EnableCollision ? 0 : 1));
                }
            }            
        }
    }
}