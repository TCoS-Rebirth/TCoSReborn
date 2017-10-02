using System;
using System.Collections.Generic;
using System.Text;
using Common;
using Database.Dynamic;
using Gameplay.Entities;
using UnityEngine;
using World;

namespace Utility
{
    /// <summary>
    ///     Handles player chat input to be interpreted as commands TODO implement help text
    /// </summary>
    public static class ChatCommandHandler
    {
        static readonly List<ChatCommand> RegisteredCommands = new List<ChatCommand>
        {
            new ChatCommand(AccountPrivilege.Player, "Prints available commands to your account type", Cmd_ListAllowedCommands, ".cmd", ".commands"),
            new ChatCommand(AccountPrivilege.Player, "Revives your character at the nearest respawn point", Cmd_Respawn, ".respawn", ".revive"),
            new ChatCommand(AccountPrivilege.Player, "Prints your position in the chat", Cmd_PrintPosition, ".pos", ".position", ".location"),
            new ChatCommand(AccountPrivilege.Player, "Leaves a note for the devs. Saves your characters location and zone: Usage: .note YourMessageHere", Cmd_LeaveNote,
                ".note", ".message"),
            new ChatCommand(AccountPrivilege.Player, "Prints help for a given command. Usage: .help command", Cmd_PrintHelp, ".help"),
            new ChatCommand(AccountPrivilege.GM, "Teleports your character to the given position. Usage: .tele x y z (no fractions)", Cmd_Teleport, ".tele", ".port",
                ".teleport"),
            new ChatCommand(AccountPrivilege.GM, "Moves your character to the given Shard ('random' location). Usage: .travel PREFIX_SHARDNAME (use .shards for list)",
                Cmd_Travel, ".travel", ".shardtravel"),
            new ChatCommand(AccountPrivilege.GM, "Lists all available shards with their internal ID", Cmd_ListShards, ".shards", ".listshards", ".destinations"),
            new ChatCommand(AccountPrivilege.GM, "Toggles DebugMode (enables certain chat notifications). usage .debug on/off", Cmd_SetDebugMode, ".debug", ".debugmode"),
            new ChatCommand(AccountPrivilege.GM, "Sets your characters physique to the given value (-5 to +5). Usage: .physique X", Cmd_SetPhysique, ".physique",
                ".setphysique"),
            new ChatCommand(AccountPrivilege.GM, "Adds the given amount of fame points to this character. Usage: .addfame X", Cmd_AddFame, ".addfame",
                ".givefame")
        };

        public static void HandleCommand(PlayerInfo p, string fullMessage, AccountPrivilege cmdPrivilege)
        {
            for (var i = 0; i < RegisteredCommands.Count; i++)
            {
                if (RegisteredCommands[i].HandleCommand(p.ActiveCharacter, p.Account.Level, fullMessage))
                {
                    return;
                }
            }
            ResponseMessage(p.ActiveCharacter, "Invalid command");
        }

        static void ResponseMessage(PlayerCharacter p, string msg)
        {
            p.ReceiveChatMessage("", msg, EGameChatRanges.GCR_SYSTEM);
        }

        static void Cmd_PrintHelp(PlayerCharacter p, string fmsg)
        {
            var args = fmsg.Split(' ');
            if (args.Length == 1)
            {
                ResponseMessage(p, "Prints help for a given Command. example: .help .respawn");
                return;
            }
            if (args.Length >= 2)
            {
                var cmdArg = args[1];
                if (!cmdArg.StartsWith(".", StringComparison.OrdinalIgnoreCase))
                {
                    cmdArg = "." + cmdArg;
                }
                foreach (var command in RegisteredCommands)
                {
                    if (command.ContainsHandle(cmdArg))
                    {
                        ResponseMessage(p, command.HelpText);
                    }
                }
            }
        }

        static void Cmd_ListAllowedCommands(PlayerCharacter p, string fmsg)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Available commands:");
            for (var i = 0; i < RegisteredCommands.Count; i++)
            {
                if (RegisteredCommands[i].IsAllowed(p.Owner.Account.Level))
                {
                    sb.AppendLine(string.Format("{0} ({1})", RegisteredCommands[i].GetPreviewQualifier(), RegisteredCommands[i].GetAliases()));
                }
            }
            ResponseMessage(p, sb.ToString());
        }

        static void Cmd_Respawn(PlayerCharacter p, string fmsg)
        {
            if (p.PawnState == EPawnStates.PS_DEAD)
            {
                p.Resurrect();
            }
            else
            {
                if (p.ActiveZone == null) return;
                if (!p.ActiveZone.TeleportToNearestRespawnLocation(p))
                {
                    ResponseMessage(p, "No respawn location found");
                }
            }
        }

        static void Cmd_PrintPosition(PlayerCharacter p, string fmsg)
        {
            ResponseMessage(p, string.Format("Position: x:{0:0.0#} y:{1:0.0#}, z:{2:0.0#}", p.Position.x, p.Position.y, p.Position.z));
        }

        static void Cmd_Teleport(PlayerCharacter p, string fmsg)
        {
            var arg = fmsg.Split(' ');
            if (arg.Length == 4)
            {
                int x, y, z;
                if (int.TryParse(arg[1], out x) &&
                    int.TryParse(arg[2], out y) &&
                    int.TryParse(arg[3], out z))
                {
                    p.TeleportTo(new Vector3(x, y, z), p.Rotation);
                }
                else
                {
                    ResponseMessage(p, "Error parsing position");
                }
                return;
            }
            else if (arg.Length == 2)
            {
                var z = p.ActiveZone;
                var td = z.FindTravelDestination(arg[1]);
                if (td != null)
                {
                    GameWorld.Instance.TravelPlayer(p, td);
                }

                //Find NPC by name
                var npc = z.FindNpcCharacter(arg[1]);
                if (npc != null)
                {
                    Vector3 teleTarget = npc.Position;
                    teleTarget.y += 1;
                    ResponseMessage(p, "Teleporting to " + npc.Name);
                    p.TeleportTo(teleTarget, npc.Rotation);
                }
                else
                {
                    ResponseMessage(p, "Error finding teleport target");
                }
            }
            else if (arg.Length == 3)
            {
                //Find NPC by name
                int npcIndex;
                if (int.TryParse(arg[2], out npcIndex))
                {
                    var z = p.ActiveZone;
                    var npc = z.FindNpcCharacter(arg[1], npcIndex);
                    if (npc != null)
                    {
                        Vector3 teleTarget = npc.Position;
                        teleTarget.y += 1;
                        ResponseMessage(p, "Teleporting to " + npc.Name);
                        p.TeleportTo(teleTarget, npc.Rotation);
                        return;
                    }
                }
                ResponseMessage(p, "Error finding teleport target");
            }
        }

        static void Cmd_Travel(PlayerCharacter p, string fmsg)
        {
            var args = fmsg.Split(' ');
            if (args.Length == 2)
            {
                var m = p.LastZoneID;
                if (Helper.EnumTryParse(args[1].ToUpper(), out m))
                {
                    var z = GameWorld.Instance.GetZone(m);
                    if (z.IsEnabled)
                    {
                        p.Owner.LoadClientMap(m);
                        return;
                    }
                    ResponseMessage(p, "Zone disabled");
                    return;
                }
                int num;
                if (int.TryParse(args[1], out num))
                {
                    var z = GameWorld.Instance.GetZone((MapIDs) num);
                    if (z && z.IsEnabled)
                    {
                        p.Owner.LoadClientMap((MapIDs) num);
                        return;
                    }
                    ResponseMessage(p, "Zone not found or disabled");
                    return;
                }
                var zones = GameWorld.Instance.GetZones();
                for (var i = 0; i < zones.Count; i++)
                {
                    if (zones[i].ReadableName.Equals(args[1], StringComparison.OrdinalIgnoreCase))
                    {
                        if (zones[i].IsEnabled)
                        {
                            p.Owner.LoadClientMap(zones[i].ID);
                            return;
                        }
                        ResponseMessage(p, "Zone disabled");
                        return;
                    }
                }
            }
            ResponseMessage(p, "Couldn't find destination");
        }

        static void Cmd_ListShards(PlayerCharacter p, string fmsg)
        {
            var sb = new StringBuilder();
            foreach (MapIDs m in Enum.GetValues(typeof (MapIDs)))
            {
                var z = GameWorld.Instance.GetZone(m);
                if (z && z.IsEnabled)
                {
                    sb.AppendLine(m.ToString());
                }
            }
            ResponseMessage(p, sb.ToString());
        }

        static void Cmd_SetDebugMode(PlayerCharacter p, string fmsg)
        {
            if (fmsg.TrimEnd(' ').EndsWith("on", StringComparison.OrdinalIgnoreCase))
            {
                p.DebugMode = true;
                ResponseMessage(p, "Debug is now On");
            }
            if (fmsg.TrimEnd(' ').EndsWith("off", StringComparison.OrdinalIgnoreCase))
            {
                p.DebugMode = false;
                ResponseMessage(p, "Debug is now Off");
            }
        }

        static void Cmd_SetPhysique(PlayerCharacter p, string fmsg)
        {
            var parts = fmsg.Split(' ');
            if (parts.Length == 2)
            {
                int val;
                if (int.TryParse(parts[1], out val) && val >= -5 && val <= 5)
                {
                    p.Stats.SetCharacterStat(ECharacterStateHealthType.ECSTH_Physique, val);
                }
                else
                {
                    ResponseMessage(p, "Invalid value");
                }
            }
        }

        static void Cmd_LeaveNote(PlayerCharacter p, string fmsg)
        {
            var parts = fmsg.Split(' ');
            if (parts.Length >= 2)
            {
                var msg = fmsg.Substring(fmsg.IndexOf(' '), fmsg.Length - fmsg.IndexOf(' '));
                MysqlDb.DevNoteDB.AddNote(p, msg);
                ResponseMessage(p, "Message saved");
            }
            else
            {
                ResponseMessage(p, "Message too short");
            }
        }

        static void Cmd_AddFame(PlayerCharacter p, string fmsg)
        {
            var arg = fmsg.Split(' ');
            if (arg.Length == 2)
            {
                int points;
                if (int.TryParse(arg[1], out points)) {
                    p.Stats.GiveFame(points);
                    ResponseMessage(p, points + " fame points added");
                }
                else ResponseMessage(p, "Invalid value");
            }
        }

        class ChatCommand
        {
            public delegate void ChatCommandHandler(PlayerCharacter p, string fullChatMessage);

            readonly List<string> _dotHandles;
            readonly ChatCommandHandler _handler;
            readonly AccountPrivilege _minRequiredPrivilege;

            public readonly string HelpText;

            public ChatCommand(AccountPrivilege minRequiredPrivilege, string helpText, ChatCommandHandler handler, params string[] dotHandles)
            {
                _dotHandles = new List<string>(dotHandles);
                _handler = handler;
                _minRequiredPrivilege = minRequiredPrivilege;
                HelpText = helpText;
            }

            public string GetAliases()
            {
                return string.Join(", ", _dotHandles.ToArray());
            }

            public bool ContainsHandle(string handle)
            {
                for (var i = 0; i < _dotHandles.Count; i++)
                {
                    if (_dotHandles[i].Equals(handle, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }

            public bool IsAllowed(AccountPrivilege accLevel)
            {
                return (int) accLevel >= (int) _minRequiredPrivilege;
            }

            public bool HandleCommand(PlayerCharacter p, AccountPrivilege privilege, string fullChatMessage)
            {
                if (!IsAllowed(privilege)) return false;
                for (var i = 0; i < _dotHandles.Count; i++)
                {
                    if (!fullChatMessage.StartsWith(_dotHandles[i], StringComparison.OrdinalIgnoreCase)) continue;
                    _handler(p, fullChatMessage);
                    return true;
                }
                return false;
            }

            public string GetPreviewQualifier()
            {
                if (_dotHandles.Count > 0)
                {
                    return _dotHandles[0];
                }
                return "[invalid Command]";
            }
        }
    }
}