using Gameplay.Entities;
using Gameplay.Entities.NPCs.Behaviours;
using UnityEngine;

namespace Gameplay.Events
{
    public class AIAggro : Content_Event
    {
        public override bool CanExecute(Entity obj, Entity subject)
        {
            if (obj as NpcCharacter && subject as Character) return true;
            else return false;
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            var npcObj = obj as NpcCharacter;
            var charSbj = subject as Character;

            var killerBehaviour = npcObj.gameObject.GetComponent<KillerBehaviour>();
            if (killerBehaviour)
            {
                npcObj.DefaultBehaviour();
            }
            else {
                killerBehaviour = npcObj.gameObject.AddComponent<KillerBehaviour>();
                npcObj.AttachBehaviour(killerBehaviour);
            }
            killerBehaviour.ForceAggro(charSbj);
        }
    }
}