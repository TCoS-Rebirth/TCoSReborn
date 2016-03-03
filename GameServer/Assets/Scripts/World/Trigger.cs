using Gameplay.Entities;
using UnityEngine;

namespace World
{
    public abstract class Trigger : MonoBehaviour
    {
        [SerializeField, ReadOnly] public Zone owningZone;

        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }

        protected virtual void OnEnteredTrigger(Character ch)
        {
        }

        protected virtual void OnLeftTrigger(Character ch)
        {
        }

        void OnTriggerEnter(Collider other)
        {
            var ch = owningZone.FindCharacter(other.gameObject);
            if (ch != null)
            {
                OnEnteredTrigger(ch);
            }
        }

        void OnTriggerExit(Collider other)
        {
            var ch = owningZone.FindCharacter(other.gameObject);
            if (ch != null)
            {
                OnLeftTrigger(ch);
            }
        }
    }
}