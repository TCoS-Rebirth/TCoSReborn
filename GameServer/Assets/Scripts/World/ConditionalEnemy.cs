using Gameplay.Entities;
using Gameplay.RequirementSpecifier;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(BoxCollider))]
    public class ConditionalEnemy : Trigger
    {
        const float defaultSizeX = 50.0f;
        const float defaultSizeY = 45.0f;
        const float defaultSizeZ = 50.0f;

        [ReadOnly]
        public NpcSpawner AttachedSpawner;
        [ReadOnly]
        public List<Content_Requirement> Requirements;
        [ReadOnly]
        public List<PlayerCharacter> AttachedPlayers = new List<PlayerCharacter>();

        void Start()
        {
            var col = GetComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(defaultSizeX, defaultSizeY, defaultSizeZ);
        }

        protected override void OnEnteredTrigger(Character ch)
        {
            refreshPlayers();
            var pc = ch as PlayerCharacter;
            if (pc == null) return;

            if (reqsMet(pc)) AttachedPlayers.Add(pc);
            else return;

            if (AttachedPlayers.Count == 0 )
                AttachedSpawner.TriggerSpawn(pc.ActiveZone);
            else if (AttachedSpawner.spawns.Count == 0)
            {
                //Handle other players attached, but spawner spawns are empty
                AttachedSpawner.TriggerSpawn(pc.ActiveZone);
            }

        }

        protected override void OnLeftTrigger(Character ch)
        {
            refreshPlayers();
            var pc = ch as PlayerCharacter;
            if (pc == null) return;

            AttachedPlayers.Remove(pc);

            if (AttachedPlayers.Count == 0)
            {
                AttachedSpawner.DespawnAll(pc.ActiveZone);
            }
        }

        protected void refreshPlayers()
        {
            foreach (var player in AttachedPlayers)
            {   
                //TODO: handle player not in collider (teleported)?
                if (    player == null 
                    ||  !player.Owner.IsIngame 
                    ||  player.ActiveZone != owningZone) {
                    AttachedPlayers.Remove(player);
                }
            }
        }

        protected bool reqsMet(PlayerCharacter pc)
        {
            foreach (var req in Requirements)
            {
                if (!req.isMet(pc))
                {
                    return false;
                }
            }
            return true;
        }

    }
}
