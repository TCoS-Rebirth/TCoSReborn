using UnityEngine;

namespace Gameplay.Entities.Interactives
{

    public class InteractionMove : InteractionComponent
    {
        [ReadOnly]
        public Vector3 Movement;

        [ReadOnly]
        public Quaternion Rotation;

        [ReadOnly]
        public float TimeSec;

        [SerializeField]
        Vector3 originalPosition;

        [SerializeField]
        Quaternion originalOrientation;

        [SerializeField]
        float timer;

        public override void onStart(PlayerCharacter instigator, bool reverse = false)
        {
            base.onStart(instigator, reverse);

            if (owner)
            {
                if (!reverse)
                {
                    originalPosition = owner.Position;
                    originalOrientation = owner.Rotation;
                    timer = 0.0f;
                }
                else
                {
                    owner.Move(originalPosition, originalOrientation);                    
                }
                owner.StartClientSubAction();

            }
        }

        public override void onEnd(PlayerCharacter instigator, bool reverse)
        {
            if (owner)
            {
                if (!reverse)
                {
                    owner.Move(originalPosition + Movement, originalOrientation * Rotation);
                    timer = TimeSec;
                    if (TimeSec > 0.0f) owner.EndClientSubAction();
                }
            }
            base.onEnd(instigator, reverse);
        }

        public override void Update()
        {
            base.Update();

            if (timer < TimeSec)
            {
                timer += Time.deltaTime;
                if (timer >= TimeSec) { owner.NextSubAction(); }
            }
        }
    }
}