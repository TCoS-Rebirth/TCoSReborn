using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Events
{
    public class Content_Event : ScriptableObject
    {

        public virtual void Execute(PlayerCharacter p) { }
        //public abstract void Execute(NpcCharacter n);
    }
}