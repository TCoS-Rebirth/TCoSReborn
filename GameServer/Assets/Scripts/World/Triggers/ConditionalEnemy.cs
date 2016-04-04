using Gameplay.Entities;
using Gameplay.RequirementSpecifier;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class ConditionalEnemy : Trigger
    {
        const float defaultRadius = 25.0f;
        const float defaultHeight = 45.0f;

        [ReadOnly]
        public NpcSpawner AttachedSpawner;       

        void Start()
        {
            var col = GetComponent<CapsuleCollider>();
            col.isTrigger = true;
            col.radius = defaultRadius;
            col.height = defaultHeight;
        }

        protected override void OnEnteredTrigger(Character ch)
        {
            base.OnEnteredTrigger(ch);

            var pc = ch as PlayerCharacter;
            if (pc == null) return;

            if (PlayerCount == 0)
                AttachedSpawner.TriggerSpawn(pc.ActiveZone);

            else if (AttachedSpawner.spawns.Count == 0)
            {
                //Handle other players attached, but spawner spawns are empty
                AttachedSpawner.TriggerSpawn(pc.ActiveZone);
            }

        }

        protected override void OnLeftTrigger(Character ch)
        {
            base.OnLeftTrigger(ch);

            var pc = ch as PlayerCharacter;
            if (pc == null) return;

            if (PlayerCount == 0)
            {
                AttachedSpawner.DespawnAll(pc.ActiveZone);
            }
        }        

    }
}
