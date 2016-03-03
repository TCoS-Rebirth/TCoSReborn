using System;
using Common;
using Gameplay.Entities;
using Network;
using World;

namespace Utility
{
    /// <summary>
    ///     This class handles the Chat (currently only player chat and even that not very well) TODO improve
    /// </summary>
    public static class ChatHandler
    {
        public static void HandleChatMessage(PlayerCharacter sender, EGameChatRanges channelID, string target, string message)
        {
            var senderPrivilege = sender.Owner.Account.Level;
            if (message.StartsWith(".", StringComparison.Ordinal))
            {
                ChatCommandHandler.HandleCommand(sender.Owner, message, senderPrivilege);
            }
            else
            {
                switch (channelID)
                {
                    case EGameChatRanges.GCR_LOCAL:
                        Say(sender, message);
                        break;
                    case EGameChatRanges.GCR_WORLD:
                        TellZone(sender, message);
                        break;
                    case EGameChatRanges.GCR_TRADE:
                        TellTrade(sender, message);
                        break;
                    case EGameChatRanges.GCR_PRIVATE:
                        Whisper(sender, target, message);
                        break;
                    case EGameChatRanges.GCR_TEAM:
                        TellTeam(sender, message);
                        break;
                }
            }
        }

        static void Say(PlayerCharacter pc, string message)
        {
            var receivers = pc.GetRelevantPlayers();
            if (receivers.Count > 0)
            {
                var m = PacketCreator.S2C_GAME_CHAT_SV2CL_ONMESSAGE(pc.Name, message, EGameChatRanges.GCR_LOCAL);
                //not sure if this is good for garbage collection, could possibly never leave scope (if left over in a send queue of a disabled npc or something)
                for (var i = 0; i < receivers.Count; i++)
                {
                    receivers[i].ReceiveRelevanceMessage(pc, m);
                }
            }
        }

        static void Whisper(PlayerCharacter pc, string target, string message)
        {
            var targetCharacter = GameWorld.Instance.FindPlayerCharacter(target);
            if (targetCharacter != null)
            {
                targetCharacter.ReceiveChatMessage(pc.name, message, EGameChatRanges.GCR_PRIVATE);
            }
        }

        static void TellZone(PlayerCharacter pc, string message)
        {
            var z = pc.ActiveZone;
            if (z == null) return;
            for (var i = 0; i < z.Players.Count; i++)
            {
                z.Players[i].ReceiveChatMessage(pc.name, message, EGameChatRanges.GCR_WORLD);
            }
        }

        static void TellTrade(PlayerCharacter pc, string message)
        {
            TellZone(pc, "[TODO: use Trade correctly] " + message);
        }

        static void TellTeam(PlayerCharacter pc, string message)
        {
            var t = pc.Team;
            if (t != null)
            {
                t.teamMessage(pc, message);
            }
        }
    }
}