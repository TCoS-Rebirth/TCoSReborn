//==============================================================================
//  QT_BeDefeated
//==============================================================================

using System.Collections.Generic;
using Gameplay.Conversations;
using Gameplay.Entities.NPCs;

namespace Gameplay.Quests.QuestTargets
{
    public class QT_BeDefeated : QuestTarget
    {
        float DefeatFraction;
        List<Taxonomy> GroupedTargets;

        List<NPC_Type> NamedTargets;
        ConversationTopic VictorySpeech;
    }
}