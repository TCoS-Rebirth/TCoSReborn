using Gameplay.Entities;
using Gameplay.Entities.NPCs;
using Gameplay.Skills.Effects;

namespace Gameplay.Skills.Events
{
    public class SkillEventSummon : SkillEventFX
    {
        [ReadOnly] public NPC_Type NPC;

        [ReadOnly] public bool SpawnedPet;

        [ReadOnly] public AudioVisualSkillEffect SummonEmitter;

        [ReadOnly] public string temporaryNPCName;

        public override void DeepClone()
        {
            base.DeepClone();
        }

        public override bool Execute(RunningSkillContext context)
        {
            return base.Execute(context);
            //needs!
        }

        public override void Reset()
        {
            base.Reset();
        }
    }
}