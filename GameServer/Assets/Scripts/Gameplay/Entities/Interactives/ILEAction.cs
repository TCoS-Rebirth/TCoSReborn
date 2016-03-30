using Common;
using Gameplay.Events;
using Gameplay.RequirementSpecifier;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Entities.Interactives
{
    public class ILEAction : ScriptableObject
    {
        public List<Content_Event> Actions;
        public ERadialMenuOptions menuOption;
        public List<Content_Requirement> Requirements;
    }
}
