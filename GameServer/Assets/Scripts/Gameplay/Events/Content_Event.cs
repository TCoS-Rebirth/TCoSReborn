using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Events
{
    public class Content_Event : ScriptableObject
    {

        public virtual void Execute(PlayerCharacter p) { }
        public virtual void Execute(NpcCharacter n) { }
        public virtual void Execute(Character c) { }
    }
}