using Common;
using UnityEngine;

namespace Gameplay.Entities.Interactives
{
    /// <summary>
    /// An ILE subaction - ILEs step through a list of these upon interaction
    /// </summary>
    public class InteractionComponent : ScriptableObject
    {
        [ReadOnly]
        public InteractiveLevelElement owner;
        [ReadOnly]
        public ERadialMenuOptions activeOption;
        [ReadOnly]
        public bool Reverse;

        protected bool cancelled;

        public virtual void onStart(PlayerCharacter instigator, bool reverse = false)
        {
            cancelled = false;
        }

        public virtual void onCancel(PlayerCharacter instigator)
        {
            cancelled = true;
        }

        public virtual void onEnd(PlayerCharacter instigator, bool reverse) { }

        public virtual void OnZoneUpdate()
        {
            if (cancelled)
            {
                owner.CancelOptionActions();
            }
        }
    }
}
