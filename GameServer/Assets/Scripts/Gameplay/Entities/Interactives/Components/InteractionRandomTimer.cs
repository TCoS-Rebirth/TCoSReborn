using UnityEngine;

namespace Gameplay.Entities.Interactives
{

    public class InteractionRandomTimer : InteractionComponent
    {
        [ReadOnly]
        public float MinTime;
        [ReadOnly]
        public float MaxTime;
        [SerializeField]
        float timer;

        public override void onStart(PlayerCharacter instigator, bool reverse)
        {
            base.onStart(instigator, reverse);

            if (!reverse)
            {
                timer = Random.Range(MinTime, MaxTime);
            }
        }

        public override void onCancel(PlayerCharacter instigator)
        {
            base.onCancel(instigator);
            timer = 0.0f;
        }

        public override void Update()
        {
            base.Update();

            if (timer > 0.0f)
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    owner.NextSubAction();
                }
            }

        }
    }
}