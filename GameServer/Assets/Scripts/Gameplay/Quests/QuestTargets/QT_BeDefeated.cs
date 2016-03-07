//==============================================================================
//  QT_BeDefeated
//==============================================================================

using System.Collections.Generic;
using Gameplay.Conversations;
using Gameplay.Entities.NPCs;
using Database.Static;

namespace Gameplay.Quests.QuestTargets
{
    public class QT_BeDefeated : QuestTarget
    {
        public float DefeatFraction;
        public List<SBResource> FactionsGroupedTargetIDs;
        public List<SBResource> NpcsNamedTargetIDs;
        public SBResource VictoryConvID;
    }
}