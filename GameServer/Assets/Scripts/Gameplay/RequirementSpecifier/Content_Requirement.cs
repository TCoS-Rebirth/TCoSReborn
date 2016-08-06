using System;
using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.RequirementSpecifier
{
    [Serializable]
    public abstract class Content_Requirement : ScriptableObject
    {
        public int ControlLocationX;
        public int ControlLocationY;
        public bool ValidForPlayer;
        public bool ValidForRelevant;

        public abstract bool isMet(PlayerCharacter p);
        public abstract bool isMet(NpcCharacter n);

        public abstract bool CheckPawn(Character character);
    }
}