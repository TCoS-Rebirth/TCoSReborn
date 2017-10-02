using Gameplay.Entities;
using Gameplay.Entities.NPCs;
using Gameplay.Skills.Effects;

namespace Gameplay.Skills.Events
{
    public class FSkillEventSummon : FSkillEventFx
    {
        [ReadOnly] public NPC_Type NPC;

        [ReadOnly] public bool SpawnedPet;

        [ReadOnly] public AudioVisualSkillEffect SummonEmitter;

        [ReadOnly] public string temporaryNPCName;

    }
}