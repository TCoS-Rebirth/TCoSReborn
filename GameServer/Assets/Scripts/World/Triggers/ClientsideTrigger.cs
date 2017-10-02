using Gameplay.Entities;
using Gameplay.RequirementSpecifier;
using Network;
using System.Collections.Generic;
using UnityEngine;
using World.Triggers;

namespace World
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class ClientsideTrigger : Trigger
    {
        const float defaultRadius = 25.0f;
        const float defaultHeight = 45.0f;

        [ReadOnly]
        string ClientEvent;
        bool TriggerAllPlayers = false;
        float Radius = 0.0f;

        void Start()
        {
            var col = GetComponent<CapsuleCollider>();

            col.isTrigger = true;
            if (Radius != 0.0f) col.radius = Radius;
            else col.radius = defaultRadius;

            col.height = defaultHeight;
        }

        protected override void OnEnteredTrigger(Character ch)
        {
            base.OnEnteredTrigger(ch);

            var instigator = ch as PlayerCharacter;
            if (!instigator) return;

            Message m = PacketCreator.S2C_GAME_PLAYERPAWN_SV2CL_CLIENTSIDETRIGGER(ClientEvent, instigator);

            if(TriggerAllPlayers)
            {
                foreach (var player in instigator.GetRelevantPlayers())
                {
                    player.ReceiveRelevanceMessage(instigator, m);
                }
            }
            instigator.SendToClient(m);

        }

        protected override void OnLeftTrigger(Character ch)
        {
            base.OnLeftTrigger(ch);
        }



    }
}
