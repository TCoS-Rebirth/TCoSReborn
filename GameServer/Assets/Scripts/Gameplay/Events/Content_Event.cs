using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Events
{
    public abstract class Content_Event : ScriptableObject
    {
        protected abstract void Execute(Entity obj, Entity subject);
        public abstract bool CanExecute(Entity obj, Entity subject);
        public bool TryExecute(Entity obj, Entity subject)
        {
            if (!CanExecute(obj, subject)) return false;
            else
            {
                Execute(obj, subject);
                return true;
            }
        }
    }
}