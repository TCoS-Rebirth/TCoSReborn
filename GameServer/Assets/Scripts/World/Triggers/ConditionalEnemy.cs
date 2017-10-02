using Gameplay.Entities;
using Gameplay.RequirementSpecifier;
using System.Collections.Generic;
using UnityEngine;
using World.Triggers;

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
            refreshChars();
            if (reqsMet(ch))
            {
                CharsInside.Add(ch);
                if (CharsInside.Count == 1)
                    AttachedSpawner.TriggerSpawn(ch.ActiveZone);
                else if (AttachedSpawner.SpawnsAlive == 0)
                {
                    //Handle other players attached, but spawner spawns are empty
                    AttachedSpawner.TriggerSpawn(ch.ActiveZone);
                }
            }            
        }

        protected override void OnLeftTrigger(Character ch)
        {
            base.OnLeftTrigger(ch);

            if (CharsInside.Count == 0)
            {
                AttachedSpawner.DespawnAll(ch.ActiveZone);
            }
        }        

    }
}
