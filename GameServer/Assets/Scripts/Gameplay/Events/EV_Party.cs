using System.Collections.Generic;
using Gameplay.RequirementSpecifier;

namespace Gameplay.Events
{
    public class EV_Party : Content_Event
    {
        public Content_Event PartyAction;
        public float Range;
        public List<Content_Requirement> Requirements;
    }
}