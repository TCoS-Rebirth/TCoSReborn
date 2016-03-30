using Gameplay.Quests;

namespace Gameplay.Entities.Interactives
{

    public class ILEQTAction : ILEAction
    {
        public Quest_Type Quest;
        public int TargetIndex;

        public void importBase(ILEAction action)
        {
            Actions = action.Actions;
            menuOption = action.menuOption;
            Requirements = action.Requirements;
        }
    }
}