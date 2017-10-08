using System;
using System.Collections.Generic;
using System.Text;
using Common.UnrealTypes;
using Gameplay.Items;
using Mono.Data.Sqlite;
using UnityEngine;

namespace Database.Dynamic.Internal
{
    public static class DatabaseHelper
    {
        public static string SerializeVector3(Vector3 v)
        {
            return string.Format("{0},{1},{2}", (int) Math.Round(v.x*10), (int) Math.Round(v.y*10), (int) Math.Round(v.z*10));
        }

        public static Vector3 DeSerializeVector3(string sv)
        {
            var v = Vector3.zero;
            var s = sv.Split(',');
            if (s.Length != 3) return v;
            int num;
            int.TryParse(s[0], out num);
            v.x = num*0.1f;
            int.TryParse(s[1], out num);
            v.y = num*0.1f;
            int.TryParse(s[2], out num);
            v.z = num*0.1f;
            return v;
        }

        public static string SerializeRotator(Rotator r)
        {
            return string.Format("{0},{1},{2}", r.Pitch, r.Yaw, r.Roll);
        }

        public static Rotator DeSerializeRotator(string sRot)
        {
            var s = sRot.Split(',');
            var r = new Rotator();
            if (s.Length != 3) return r;
            int.TryParse(s[0], out r.Pitch);
            int.TryParse(s[1], out r.Yaw);
            int.TryParse(s[2], out r.Roll);
            return r;
        }

        public static string SerializeIntList(params int[] ints)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < ints.Length; i++)
            {
                if (i == 0)
                {
                    sb.Append(ints[i].ToString());
                }
                else
                {
                    sb.Append(",");
                    sb.Append(ints[i].ToString());
                }
            }
            return sb.ToString();
        }

        public static List<int> DeserializeIntList(string sInts, int expectedCount)
        {
            var ints = new List<int>();
            var s = sInts.Split(',');
            for (var i = 0; i < s.Length; i++)
            {
                int num;
                int.TryParse(s[i], out num);
                ints.Add(num);
            }
            if (expectedCount > 0)
            {
                while (ints.Count < expectedCount)
                {
                    Debug.Log("[DatabaseHelper]DeserializeIntlist: missing value in DB, adding zero");
                    ints.Add(0);
                }
            }
            return ints;
        }

        public static string SerializeFloatList(params float[] floats)
        {
            var intConverted = new List<int>();
            for (var i = 0; i < floats.Length; i++)
            {
                intConverted.Add((int) Math.Round(floats[i]*10));
            }
            return SerializeIntList(intConverted.ToArray());
        }

        public static List<float> DeserializeFloatList(string sF, int expectedCount)
        {
            var intConverted = DeserializeIntList(sF, expectedCount);
            var floats = new List<float>();
            for (var i = 0; i < intConverted.Count; i++)
            {
                floats.Add(intConverted[i]*0.1f);
            }
            return floats;
        }

        #region Character saving

        [NonSerialized] static SqliteCommand _updateExistingCharacterCommand;

        public static SqliteCommand PrepareCharacterUpdateExistingCommand(SqliteConnection connection, DBPlayerCharacter pc, SqliteTransaction transaction)
        {
            if (_updateExistingCharacterCommand == null)
            {
                _updateExistingCharacterCommand =
                    new SqliteCommand(
                        "UPDATE playercharacters SET Appearance=@app, LastMapID=@lm, Faction=@fac, ArcheType=@arch, PawnState=@state, Position=@pos, Rotation=@rot, FamePep=@fp, Health=@health, Money=@money, BMFAttributeExtraPoints=@bmfaep, SkillDeck=@skilldeck WHERE AccountID = @accID AND CharacterID = @charID;",
                        connection);
                AddCharacterCommandParameters(_updateExistingCharacterCommand, pc);
                _updateExistingCharacterCommand.Prepare();
            }
            else
            {
                FillCharacterCommandParameters(_updateExistingCharacterCommand, pc);
            }
            _updateExistingCharacterCommand.Transaction = transaction;
            return _updateExistingCharacterCommand;
        }

        [NonSerialized] static SqliteCommand _saveNewCharacterCommand;

        public static SqliteCommand GetCharacterSaveNewCommand(SqliteConnection connection, DBPlayerCharacter pc, SqliteTransaction transaction)
        {
            if (_saveNewCharacterCommand == null)
            {
                _saveNewCharacterCommand =
                    new SqliteCommand(
                        "INSERT INTO playercharacters (AccountID,CharacterID,Name,Appearance,LastMapID,Faction,ArcheType,PawnState,Position,Rotation,FamePep,Health,Money,BMFAttributeExtraPoints,SkillDeck) " +
                        "VALUES (@accID,@charID,@name,@app,@lm,@fac,@arch,@state,@pos,@rot,@fp,@health,@money,@bmfaep,@skilldeck);",
                        connection);
                AddCharacterCommandParameters(_saveNewCharacterCommand, pc);
                _saveNewCharacterCommand.Prepare();
            }
            else
            {
                FillCharacterCommandParameters(_saveNewCharacterCommand, pc);
            }
            _saveNewCharacterCommand.Transaction = transaction;
            return _saveNewCharacterCommand;
        }

        static void AddCharacterCommandParameters(SqliteCommand cmd, DBPlayerCharacter pc)
        {
            cmd.Parameters.AddWithValue("@accID", pc.AccountID);
            cmd.Parameters.AddWithValue("@charID", pc.DBID);
            cmd.Parameters.AddWithValue("@name", pc.Name);
            cmd.Parameters.AddWithValue("@app", SerializeIntList(pc.Appearance.Race,
                (int) pc.Appearance.Gender, pc.Appearance.Body, pc.Appearance.Head,
                pc.Appearance.BodyColor, pc.Appearance.TattooChest, pc.Appearance.TattooLeft,
                pc.Appearance.TattooRight, pc.Appearance.Hair, pc.Appearance.HairColor, pc.Appearance.Voice));
            cmd.Parameters.AddWithValue("@lm", pc.LastZoneID);
            cmd.Parameters.AddWithValue("@fac", pc.Faction);
            cmd.Parameters.AddWithValue("@arch", pc.ArcheType);
            cmd.Parameters.AddWithValue("@state", pc.PawnState);
            cmd.Parameters.AddWithValue("@pos", SerializeVector3(pc.Position));
            cmd.Parameters.AddWithValue("@rot", SerializeVector3(pc.Rotation));
            cmd.Parameters.AddWithValue("@fp", SerializeIntList(pc.FamePep));
            cmd.Parameters.AddWithValue("@health", pc.Health);
            cmd.Parameters.AddWithValue("@money", pc.Money);
            cmd.Parameters.AddWithValue("@bmfaep", SerializeIntList(pc.ExtraBodyMindFocusAttributePoints));
            cmd.Parameters.AddWithValue("@skilldeck", pc.SerializedSkillDeck);
        }

        static void FillCharacterCommandParameters(SqliteCommand cmd, DBPlayerCharacter pc)
        {
            cmd.Parameters["@accID"].Value = pc.AccountID;
            cmd.Parameters["@charID"].Value = pc.DBID;
            cmd.Parameters["@name"].Value = pc.Name;
            cmd.Parameters["@app"].Value = SerializeIntList(pc.Appearance.Race,
                (int) pc.Appearance.Gender, pc.Appearance.Body, pc.Appearance.Head,
                pc.Appearance.BodyColor, pc.Appearance.TattooChest, pc.Appearance.TattooLeft,
                pc.Appearance.TattooRight, pc.Appearance.Hair, pc.Appearance.HairColor, pc.Appearance.Voice);
            cmd.Parameters["@lm"].Value = pc.LastZoneID;
            cmd.Parameters["@fac"].Value = pc.Faction;
            cmd.Parameters["@arch"].Value = pc.ArcheType;
            cmd.Parameters["@state"].Value = pc.PawnState;
            cmd.Parameters["@pos"].Value = SerializeVector3(pc.Position);
            cmd.Parameters["@rot"].Value = SerializeVector3(pc.Rotation);
            cmd.Parameters["@fp"].Value = SerializeIntList(pc.FamePep);
            cmd.Parameters["@health"].Value = pc.Health;
            cmd.Parameters["@money"].Value = pc.Money;
            cmd.Parameters["@bmfaep"].Value =
                SerializeIntList(pc.ExtraBodyMindFocusAttributePoints);
            cmd.Parameters["@skilldeck"].Value = pc.SerializedSkillDeck;
        }

        #endregion

        #region Loading

        [NonSerialized] static SqliteCommand _characterLoadCommand;

        public static SqliteCommand GetCharactersLoadAllCommand(SqliteConnection connection)
        {
            if (_characterLoadCommand == null)
            {
                _characterLoadCommand = new SqliteCommand("SELECT * FROM playercharacters", connection);
                _characterLoadCommand.Prepare();
            }
            return _characterLoadCommand;
        }

        [NonSerialized] static SqliteCommand _skillLoadCommand;

        public static SqliteCommand GetCharacterAllSkillsLoadCommand(SqliteConnection connection)
        {
            if (_skillLoadCommand == null)
            {
                _skillLoadCommand = new SqliteCommand("SELECT * FROM playercharacterskills", connection);
                _skillLoadCommand.Prepare();
            }
            return _skillLoadCommand;
        }

        [NonSerialized] static SqliteCommand _itemsLoadCommand;

        public static SqliteCommand GetCharacterAllItemsLoadCommand(SqliteConnection connection)
        {
            if (_itemsLoadCommand == null)
            {
                _itemsLoadCommand = new SqliteCommand("SELECT * FROM playercharacteritems", connection);
                _itemsLoadCommand.Prepare();
            }
            return _itemsLoadCommand;
        }

        [NonSerialized] static SqliteCommand _questsLoadCommand;

        public static SqliteCommand GetCharacterAllQuestsLoadCommand(SqliteConnection connection)
        {
            if (_questsLoadCommand == null)
            {
                _questsLoadCommand = new SqliteCommand("SELECT * FROM playercharacterquests", connection);
                _questsLoadCommand.Prepare();
            }
            return _questsLoadCommand;
        }

        [NonSerialized] static SqliteCommand _perVarsLoadCommand;

        public static SqliteCommand GetCharacterPerVarsLoadCommand(SqliteConnection connection)
        {
            if (_perVarsLoadCommand == null)
            {
                _perVarsLoadCommand = new SqliteCommand("SELECT * FROM playercharacterpersistentvars", connection);
                _perVarsLoadCommand.Prepare();
            }
            return _perVarsLoadCommand;
        }

        #endregion

        #region Saving

        [NonSerialized] static SqliteCommand _characterSkillSaveCommand;

        public static SqliteCommand GetCharacterSkillSaveCommand(SqliteConnection connection, DBPlayerCharacter pc, DBSkill skill, SqliteTransaction transaction)
        {
            if (_characterSkillSaveCommand == null)
            {
                const string cmd = "INSERT INTO playercharacterskills (CharacterID,SkillID,SigilSlots) VALUES (@charID,@skillID,@Slots);";
                _characterSkillSaveCommand = new SqliteCommand(cmd, connection);
                _characterSkillSaveCommand.Parameters.AddWithValue("@charID", pc.DBID);
                _characterSkillSaveCommand.Parameters.AddWithValue("@skillID", skill != null ? skill.ResourceId : -1);
                _characterSkillSaveCommand.Parameters.AddWithValue("@Slots", skill != null ? skill.SigilSlots : 0);
                _characterSkillSaveCommand.Prepare();
            }
            else
            {
                _characterSkillSaveCommand.Parameters["@charID"].Value = pc.DBID;
                _characterSkillSaveCommand.Parameters["@skillID"].Value = skill != null ? skill.ResourceId : -1;
                _characterSkillSaveCommand.Parameters["@Slots"].Value = skill != null ? skill.SigilSlots : 0;
            }
            _characterSkillSaveCommand.Transaction = transaction;
            return _characterSkillSaveCommand;
        }

        [NonSerialized] static SqliteCommand _characterSkillsDeleteCommand;

        public static SqliteCommand GetCharacterSkillsDeleteCommand(SqliteConnection connection, DBPlayerCharacter pc, SqliteTransaction transaction)
        {
            if (_characterSkillsDeleteCommand == null)
            {
                _characterSkillsDeleteCommand = new SqliteCommand("DELETE FROM playercharacterskills WHERE CharacterID=@charid", connection);
                _characterSkillsDeleteCommand.Parameters.AddWithValue("@charid", pc.DBID);
                _characterSkillsDeleteCommand.Prepare();
            }
            else
            {
                _characterSkillsDeleteCommand.Parameters["@charid"].Value = pc.DBID;
            }
            _characterSkillsDeleteCommand.Transaction = transaction;
            return _characterSkillsDeleteCommand;
        }

        [NonSerialized] static SqliteCommand _characterItemsSaveCommand;

        public static SqliteCommand GetCharacterItemSaveCommand(SqliteConnection connection, DBPlayerCharacter pc, Game_Item item, SqliteTransaction transaction)
        {
            if (_characterItemsSaveCommand == null)
            {
                _characterItemsSaveCommand =
                    new SqliteCommand(
                        "INSERT INTO playercharacteritems (ID,CharacterID,Stacks,ResourceID,LocationType,LocationSlot,Attuned,Color1,Color2,Serial) VALUES (@id,@charID,@stacks,@resID,@locType,@locSlot,@att,@col1,@col2,@serial);",
                        connection);
                _characterItemsSaveCommand.Parameters.AddWithValue("@id", item != null ? item.DBID : -1);
                _characterItemsSaveCommand.Parameters.AddWithValue("@charID", pc.DBID);
                _characterItemsSaveCommand.Parameters.AddWithValue("@stacks", item != null ? item.StackSize : 0);
                _characterItemsSaveCommand.Parameters.AddWithValue("@resID", item != null ? item.Type.resourceID : -1);
                _characterItemsSaveCommand.Parameters.AddWithValue("@locType", item != null ? item.LocationType : 0);
                _characterItemsSaveCommand.Parameters.AddWithValue("@locSlot", item != null ? item.LocationSlot : 0);
                _characterItemsSaveCommand.Parameters.AddWithValue("@att", item != null ? (int) item.Attuned : 0);
                _characterItemsSaveCommand.Parameters.AddWithValue("@col1", item != null ? (int) item.Color1 : 0);
                _characterItemsSaveCommand.Parameters.AddWithValue("@col2", item != null ? (int) item.Color2 : 0);
                _characterItemsSaveCommand.Parameters.AddWithValue("@serial", 0);
                _characterItemsSaveCommand.Prepare();
            }
            else
            {
                _characterItemsSaveCommand.Parameters["@id"].Value = item != null ? item.DBID : -1;
                _characterItemsSaveCommand.Parameters["@charID"].Value = pc.DBID;
                _characterItemsSaveCommand.Parameters["@stacks"].Value = item != null ? item.StackSize : 0;
                _characterItemsSaveCommand.Parameters["@resID"].Value = item != null ? item.Type.resourceID : -1;
                _characterItemsSaveCommand.Parameters["@locType"].Value = item != null ? item.LocationType : 0;
                _characterItemsSaveCommand.Parameters["@locSlot"].Value = item != null ? item.LocationSlot : 0;
                _characterItemsSaveCommand.Parameters["@att"].Value = item != null ? (int) item.Attuned : 0;
                _characterItemsSaveCommand.Parameters["@col1"].Value = item != null ? (int) item.Color1 : 0;
                _characterItemsSaveCommand.Parameters["@col2"].Value = item != null ? (int) item.Color2 : 0;
                _characterItemsSaveCommand.Parameters["@serial"].Value = 0;
            }
            _characterItemsSaveCommand.Transaction = transaction;
            return _characterItemsSaveCommand;
        }

        [NonSerialized] static SqliteCommand _characterItemsDeleteCommand;

        public static SqliteCommand GetCharacterItemsDeleteCommand(SqliteConnection connection, DBPlayerCharacter pc, SqliteTransaction transaction)
        {
            if (_characterItemsDeleteCommand == null)
            {
                _characterItemsDeleteCommand = new SqliteCommand("DELETE FROM playercharacteritems WHERE CharacterID=@charID", connection);
                _characterItemsDeleteCommand.Parameters.AddWithValue("@charID", pc.DBID);
            }
            else
            {
                _characterItemsDeleteCommand.Parameters["@charID"].Value = pc.DBID;
            }
            _characterItemsDeleteCommand.Transaction = transaction;
            return _characterItemsDeleteCommand;
        }

        [NonSerialized] static SqliteCommand _characterQuestTargetSaveCommand;

        public static SqliteCommand GetCharacterQuestTargetSaveCommand(SqliteConnection connection, DBPlayerCharacter pc, DBQuestTarget target, SqliteTransaction transaction)
        {
            if (_characterQuestTargetSaveCommand == null)
            {
                const string cmd = "INSERT INTO playercharacterquests (CharacterID,QuestID,IsComplete,TargetIndex,TargetProgress) VALUES (@charID,@questID,@isComplete,@tarIndex,@tarProg);";
                _characterQuestTargetSaveCommand = new SqliteCommand(cmd, connection);
                _characterQuestTargetSaveCommand.Parameters.AddWithValue("@charID", pc.DBID);
                _characterQuestTargetSaveCommand.Parameters.AddWithValue("@questID", target != null? target.ResourceId : -1);
                _characterQuestTargetSaveCommand.Parameters.AddWithValue("@isComplete", target != null && target.isCompleted);
                _characterQuestTargetSaveCommand.Parameters.AddWithValue("@tarIndex", target != null ? target.targetIndex : 0);
                _characterQuestTargetSaveCommand.Parameters.AddWithValue("@tarProg", target != null ? target.targetProgress : 0);
                _characterQuestTargetSaveCommand.Prepare();
            }
            else
            {
                _characterQuestTargetSaveCommand.Parameters["@charID"].Value = pc.DBID;
                _characterQuestTargetSaveCommand.Parameters["@questID"].Value = target != null ? target.ResourceId : -1;
                _characterQuestTargetSaveCommand.Parameters["@isComplete"].Value = target != null && target.isCompleted;
                _characterQuestTargetSaveCommand.Parameters["@tarIndex"].Value = target != null ? target.targetIndex : 0;
                _characterQuestTargetSaveCommand.Parameters["@tarProg"].Value = target != null ? target.targetProgress : 0;
            }
            _characterQuestTargetSaveCommand.Transaction = transaction;
            return _characterQuestTargetSaveCommand;
        }

        [NonSerialized] static SqliteCommand _characterQuestsDeleteCommand;

        public static SqliteCommand GetCharacterQuestsDeleteCommand(SqliteConnection connection, DBPlayerCharacter pc, SqliteTransaction transaction)
        {
            if (_characterQuestsDeleteCommand == null)
            {
                _characterQuestsDeleteCommand = new SqliteCommand("DELETE FROM playercharacterquests WHERE CharacterID=@charID", connection);
                _characterQuestsDeleteCommand.Parameters.AddWithValue("@charID", pc.DBID);
            }
            else
            {
                _characterQuestsDeleteCommand.Parameters["@charID"].Value = pc.DBID;
            }
            _characterQuestsDeleteCommand.Transaction = transaction;
            return _characterQuestsDeleteCommand;
        }

        [NonSerialized] static SqliteCommand _characterPerVarSaveCommand;

        public static SqliteCommand GetCharacterPerVarSaveCommand(SqliteConnection connection, DBPlayerCharacter pc, DBPersistentVar var, SqliteTransaction transaction)
        {
            if (_characterPerVarSaveCommand == null)
            {
                const string cmd = "INSERT INTO playercharacterpersistentvars (CharacterID,ContextID,VarID,Value) VALUES (@charID,@contextId,@varID,@value);";
                _characterPerVarSaveCommand = new SqliteCommand(cmd, connection);
                _characterPerVarSaveCommand.Parameters.AddWithValue("@charID", pc.DBID);
                _characterPerVarSaveCommand.Parameters.AddWithValue("@contextID", var != null ? var.ContextId : -1);
                _characterPerVarSaveCommand.Parameters.AddWithValue("@varID", var != null ? var.VarId : -1);
                _characterPerVarSaveCommand.Parameters.AddWithValue("@value", var != null ? var.Value : 0);
                _characterPerVarSaveCommand.Prepare();
            }
            else
            {
                _characterPerVarSaveCommand.Parameters["@charID"].Value = pc.DBID;
                _characterPerVarSaveCommand.Parameters["@contextID"].Value = var != null ? var.ContextId : -1;
                _characterPerVarSaveCommand.Parameters["@varID"].Value = var != null ? var.VarId : -1;
                _characterPerVarSaveCommand.Parameters["@value"].Value = var != null ? var.Value : 0;
            }
            _characterPerVarSaveCommand.Transaction = transaction;
            return _characterPerVarSaveCommand;
        }

        [NonSerialized] static SqliteCommand _characterPerVarsDeleteCommand;

        public static SqliteCommand GetCharacterPerVarsDeleteCommand(SqliteConnection connection, DBPlayerCharacter pc, SqliteTransaction transaction)
        {
            if (_characterPerVarsDeleteCommand == null)
            {
                _characterPerVarsDeleteCommand = new SqliteCommand("DELETE FROM playercharacterpersistentvars WHERE CharacterID=@charID", connection);
                _characterPerVarsDeleteCommand.Parameters.AddWithValue("@charID", pc.DBID);
            }
            else
            {
                _characterPerVarsDeleteCommand.Parameters["@charID"].Value = pc.DBID;
            }
            _characterPerVarsDeleteCommand.Transaction = transaction;
            return _characterPerVarsDeleteCommand;
        }

        #endregion
    }
}