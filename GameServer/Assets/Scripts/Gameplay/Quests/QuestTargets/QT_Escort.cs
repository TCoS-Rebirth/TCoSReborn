//==============================================================================
//  QT_Escort
//==============================================================================

namespace Gameplay.Quests.QuestTargets
{
    public class QT_Escort : QuestTarget
    {
        public string ScriptTag;

        public override int GetCompletedProgressValue() { return 1; }
    }
}