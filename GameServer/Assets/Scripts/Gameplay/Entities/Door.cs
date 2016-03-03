using UnityEngine;

namespace Gameplay.Entities
{
    public class Door : InteractiveLevelElement
    {
        public Transform destination;

        public Transform getDestination()
        {
            return destination;
        }

        public void setDestination(Transform dest)
        {
            destination = dest;
        }
    }
}