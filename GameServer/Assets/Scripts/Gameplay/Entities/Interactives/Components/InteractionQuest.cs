using Gameplay.Quests;

namespace Gameplay.Entities.Interactives
{

    public class InteractionQuest : InteractionComponent
    {
        [ReadOnly] public Quest_Type Quest;
        [ReadOnly] public int TarIndex;

        public override void onStart(PlayerCharacter instigator, bool reverse)
        {
            base.onStart(instigator);
            if (instigator)
            {
                if (!reverse)
                {
                    instigator.TryAdvanceTarget(Quest, Quest.targets[TarIndex]);
                    owner.NextSubAction();
                }
            }
            
        }
    }
}