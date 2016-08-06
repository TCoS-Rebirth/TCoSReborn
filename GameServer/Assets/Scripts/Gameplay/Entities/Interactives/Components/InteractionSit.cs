using Gameplay.Events;
using UnityEngine;

namespace Gameplay.Entities.Interactives
{
    public class InteractionSit : InteractionComponent
    {
        [ReadOnly]
        public Vector3 Offset;

        [ReadOnly]
        public float sitTimer;

        public override void onStart(PlayerCharacter instigator, bool reverse = false)
        {
            base.onStart(instigator, reverse);
            sitTimer = 0.0f;
            if (instigator)
            {
                instigator.Stats.FreezePosition = true;
                instigator.Stats.FreezeRotation = true;
                sitTimer = 1.20f;

                var sitEv = CreateInstance<EV_Sit>();
                sitEv.Offset = Offset;

                sitEv.TryExecute(instigator, owner);
                instigator.Sit(!reverse, true);
            }
        }

        public override void onCancel(PlayerCharacter instigator)
        {
            base.onCancel(instigator);
        }

        public override void onEnd(PlayerCharacter instigator, bool reverse)
        {

        }

        public override void OnZoneUpdate()
        {
            base.OnZoneUpdate();

            if (sitTimer > 0.0f)
            {
                sitTimer -= Time.deltaTime;
                
            }
            if (sitTimer <= 0.0f) {
                owner.NextSubAction();
            }
        }

    }
}
