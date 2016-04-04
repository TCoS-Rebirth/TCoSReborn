using UnityEngine;

namespace Gameplay.Entities.Interactives
{

    public class InteractionAnimation : InteractionComponent
    {
        [ReadOnly]
        public string AnimName;
        [ReadOnly]
        public float LoopDuration;
        [ReadOnly]
        public float Speed;

        [SerializeField]
        float timer;

        public override void onStart(PlayerCharacter instigator, bool reverse)
        {
            base.onStart(instigator, reverse);
            if (owner && (AnimName != ""))
            {
                if (!reverse)
                {
                    owner.StartClientSubAction();
                    timer = 0.0f;
                }
            }
        }

        public override void onEnd(PlayerCharacter instigator, bool reverse)
        {
            if (owner)
            {
                if (!reverse)
                {
                    timer = LoopDuration;
                    if (LoopDuration > 0.0f)
                    {
                        owner.EndClientSubAction();
                    }
                }
            }

            base.onEnd(instigator, reverse);
        }

        public override void Update()
        {
            base.Update();
            //TODO: Check exactly how this is timed, change if needed

            if (timer < LoopDuration)
            {
                timer += Time.deltaTime;
                if (timer >= LoopDuration)
                {
                    owner.NextSubAction();
                }
            }

        }
    }
}