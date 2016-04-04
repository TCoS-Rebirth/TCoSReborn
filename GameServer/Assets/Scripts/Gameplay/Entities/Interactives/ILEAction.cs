using Common;
using Gameplay.Events;
using Gameplay.RequirementSpecifier;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Entities.Interactives
{
    public class ILEAction : ScriptableObject
    {
        public List<InteractionComponent> StackedActions = new List<InteractionComponent>();
        public ERadialMenuOptions menuOption;
        public List<Content_Requirement> Requirements = new List<Content_Requirement>();

        public bool isEligible(Character c)
        {
            var pc = c as PlayerCharacter;
            var nc = c as NpcCharacter;

            foreach (var req in Requirements)
            {
                if (pc && !req.isMet(pc)) { return false; }
                if (nc && !req.isMet(nc)) { return false; }
            }
            return true;
            
        }
    }
}
