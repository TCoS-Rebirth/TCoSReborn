using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Common;
using Database.Dynamic.Internal;
using Database.Static;
using Gameplay;
using Gameplay.Entities;
using Gameplay.Items;
using Gameplay.Skills;
using Mono.Data.Sqlite;
//using MySql.Data.MySqlClient;
using UnityEngine;
using Utility;
using World;

namespace Database.Dynamic
{
    public static class DB
    {

        //public static string EscapeString(string input)
        //{
            //return MySql.Data.MySqlClient.MySqlHelper.EscapeString(input);
            
        //}

        public static bool Initialize()
        {
            if (!Connect())
            {
                Debug.Log("couldn't connect to DB");
                return false;
            }
            if (!CharacterDB.Initialize())
            {
                Debug.Log("Character cache could not be loaded");
                return false;
            }
            return true;
        }

        public static void CloseConnection()
        {
            if (_cachedConnection != null)
            {
                Debug.Log("Closing Database connection");
                _cachedConnection.Close();
                if (_cachedConnection.State == ConnectionState.Closed)
                {
                    Debug.Log("Database connection closed");
                }
            }
        }

        #region UserAccount

        public static class AccountDB
        {
            public static UserAccount GetAccount(int sessionKey)
            {
                if (sessionKey == -1) return null;
                using (var command = new SqliteCommand("SELECT * FROM accounts WHERE (SessionKey=@skey) Limit 1", CachedConnection))
                {
                    command.Parameters.AddWithValue("@skey", sessionKey);
                    using (var reader = command.ExecuteReader())
                    {
                        UserAccount acc = null;
                        while (reader.Read())
                        {
                            var id = reader.GetInt32(0);
                            var accname = reader.GetString(1);
                            var passHash = reader.GetString(2);
                            var mail = reader.GetString(3);
                            var banned = reader.GetInt32(4) == 1;
                            var privilige = (AccountPrivilege) reader.GetInt32(5);
                            var dt = reader.GetString(6);
                            var lastlogin = DateTime.Parse(dt.Length > 0 ? dt : DateTime.Now.ToString());
                            var sessKey = reader.GetInt32(8);
                            
                            acc = new UserAccount(id, accname, passHash, mail, banned, privilige, lastlogin, false, sessKey, -1);
                        }
                        return acc;
                    }
                }
            }

            public static UserAccount GetAccount(string name)
            {
                if (string.IsNullOrEmpty(name)) return null;
                using (var command = new SqliteCommand("SELECT * FROM accounts WHERE (Name=@name) Limit 1", CachedConnection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    using (var reader = command.ExecuteReader())
                    {
                        UserAccount acc = null;
                        while (reader.Read())
                        {
                            var id = reader.GetInt32(0);
                            var accname = reader.GetString(1);
                            var passHash = reader.GetString(2);
                            var mail = reader.GetString(3);
                            var banned = reader.GetInt32(4) == 1;
                            var privilige = (AccountPrivilege)reader.GetInt32(5);
                            var dt = reader.GetString(6);
                            var lastlogin = DateTime.Parse(dt.Length > 0 ? dt : DateTime.Now.ToString());
                            var sessKey = reader.GetInt32(8);

                            acc = new UserAccount(id, accname, passHash, mail, banned, privilige, lastlogin, false, sessKey, -1);
                        }
                        return acc;
                    }
                }
            }

            /// <summary>
            ///     saves <see cref="UserAccount.Banned" /> and <see cref="UserAccount.IsOnline" /> and
            ///     <see cref="UserAccount.LastUniverse" /> to the DB
            ///     Checks the accounts current sessionkey and writes if allowed (key is active one)
            /// </summary>
            public static bool UpdateAccount(UserAccount acc)
            {
                using (var transaction = CachedConnection.BeginTransaction())
                using (
                    var cmd = new SqliteCommand("UPDATE accounts SET banned=@ban, IsOnline=@online, LastUniverse=@lastUni, SessionKey=@sKey WHERE ID=@id",
                        CachedConnection, transaction))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@id", acc.UID);
                        cmd.Parameters.AddWithValue("@sKey", acc.SessionKey);
                        cmd.Parameters.AddWithValue("@ban", acc.Banned ? 1 : 0);
                        cmd.Parameters.AddWithValue("@lastUni", acc.LastUniverse);
                        cmd.Parameters.AddWithValue("@online", acc.IsOnline ? 1 : 0);
                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            transaction.Commit();
                            return true;
                        }
                        transaction.Rollback();
                        return false;
                    }
                    catch (SqliteException)
                    {
                        Debug.LogError("Error updating account, changes revertd");
                        transaction.Rollback();
                        return false;
                    }
                }
            }

            public static int GetSessionKey(UserAccount acc)
            {
                using (var cmd = new SqliteCommand("SELECT SessionKey FROM accounts WHERE ID=@id", CachedConnection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@id", acc.UID);
                        var val = cmd.ExecuteScalar();
                        if (val != null)
                        {
                            return Convert.ToInt32(val);
                        }
                        Debug.LogError("SessionKey db retrieved value invalid");
                        return -1;
                    }
                    catch (SqliteException e)
                    {
                        Debug.LogError(e.Message);
                        return -1;
                    }
                }
            }
        }

        #endregion

        #region DevNotes

        public static class DevNoteDB
        {
            public static bool AddNote(PlayerCharacter p, string msg)
            {
                using (var transaction = CachedConnection.BeginTransaction())
                using (
                    var cmd = new SqliteCommand("INSERT INTO devnotes (CharacterID, Position, Note, ZoneID) VALUES (@charID, @pos, @note, @zone)", CachedConnection,
                        transaction))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@charID", p.dbRef.DBID);
                        cmd.Parameters.AddWithValue("@pos", DatabaseHelper.SerializeVector3(p.Position));
                        cmd.Parameters.AddWithValue("@note", msg);
                        cmd.Parameters.AddWithValue("@zone", (int) p.ActiveZone.ID);
                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            transaction.Commit();
                            return true;
                        }
                        transaction.Rollback();
                        return false;
                    }
                    catch (SqliteException)
                    {
                        Debug.LogError("Error inserting DevNote, no entries inserted");
                        transaction.Rollback();
                        return false;
                    }
                }
            }

            public static List<DevNoteLoader.DevNote> LoadAllNotes()
            {
                var notes = new List<DevNoteLoader.DevNote>();
                if (Application.isPlaying)
                {
                    Debug.Log("intended to be used at design time");
                    return notes;
                }
                var config = GameWorld.Instance.LoadConfigFile();
                if (config == null)
                {
                    Debug.Log("DB connection config file could not be loaded");
                    return notes;
                }
                var cmd = new SqliteCommand("SELECT * FROM devnotes");
                var path = Directory.GetCurrentDirectory() + @"\Database.sqlite";
                using ( var con = new SqliteConnection(string.Format("Data Source={0};Version=3", path)))
                {
                    con.Open();
                    if (con.State != ConnectionState.Open)
                    {
                        Debug.Log("can't open mysql connection");
                        return notes;
                    }
                    cmd.Connection = con;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var note = new DevNoteLoader.DevNote();
                            note.CharacterID = reader.GetInt32(0);
                            note.mapID = reader.GetInt32(1);
                            note.position = DatabaseHelper.DeSerializeVector3(reader.GetString(2));
                            note.note = reader.GetString(3);
                            notes.Add(note);
                        }
                    }
                    con.Close();
                }
                return notes;
            }
        }

        #endregion

        #region Character

        public static class CharacterDB
        {
            static readonly List<DBPlayerCharacter> _characterCache = new List<DBPlayerCharacter>();

            //public bool Initialize(SqliteConnection _cachedConnection)
            public static bool Initialize()
            {
                _characterCache.Clear();
                if (!LoadCharacterCache())
                {
                    return false;
                }
                if (!LoadAndAssignSkills())
                {
                    return false;
                }
                if (!LoadAndAssignItems())
                {
                    return false;
                }
                if (!LoadAndAssignQuests())
                {
                    return false;
                }
                if (!LoadAndAssignPerVars())
                {
                    return false;
                }
                return true;
            }

            static DBPlayerCharacter GetCharacter(int dbID)
            {
                for (var i = 0; i < _characterCache.Count; i++)
                {
                    if (_characterCache[i].DBID == dbID)
                    {
                        return _characterCache[i];
                    }
                }
                return null;
            }

            static bool CheckNameAvailable(DBPlayerCharacter character)
            {
                for (var i = _characterCache.Count; i-- > 0;)
                {
                    if (_characterCache[i].Name.Equals(character.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                return true;
            }

            public static List<DBPlayerCharacter> GetAccountCharacters(int accountID)
            {
                var accountCharacters = new List<DBPlayerCharacter>();
                for (var i = 0; i < _characterCache.Count; i++)
                {
                    if (_characterCache[i].AccountID == accountID)
                    {
                        accountCharacters.Add(_characterCache[i]);
                    }
                }
                return accountCharacters;
            }

            public static DBPlayerCharacter GetAccountCharacter(int dbID, int accountID)
            {
                for (var i = 0; i < _characterCache.Count; i++)
                {
                    if (_characterCache[i].AccountID == accountID && _characterCache[i].DBID == dbID)
                    {
                        return _characterCache[i];
                    }
                }
                return null;
            }

            public static bool CreateNewCharacter(int accountID, DBPlayerCharacter character)
            {
                //character.Name = EscapeString(character.Name);
                if (!CheckNameAvailable(character))
                {
                    return false;
                }
                character.DBID = AllocateDBID();
                foreach (var it in character.Items)
                {
                    it.DBID = AllocateDBID();
                }
                character.AccountID = accountID;
                if (SaveCharacterToDB(character))
                {
                    _characterCache.Add(character);
                    return true;
                }
                Debug.LogError("Error saving Character to DB");
                return false;
            }

            public static bool DeleteCharacter(int charID, UserAccount acc)
            {
                for (var i = 0; i < _characterCache.Count; i++)
                {
                    //remove cache
                    if (_characterCache[i].AccountID != acc.UID || _characterCache[i].DBID != charID) continue;
                    Debug.Log(string.Format("Player '{0}' deleted Character '{1}'", acc.Name, _characterCache[i].Name));
                    _characterCache.RemoveAt(i);
                    //remove mysql entry
                    using (var transaction = CachedConnection.BeginTransaction())
                    using (
                        var delCharacterCmd = new SqliteCommand("DELETE from PlayerCharacters WHERE CharacterID=@charID AND AccountID=@accID", CachedConnection,
                            transaction))
                    {
                        try
                        {
                            delCharacterCmd.Parameters.AddWithValue("@charID", charID);
                            delCharacterCmd.Parameters.AddWithValue("@accID", acc.UID);
                            delCharacterCmd.ExecuteNonQuery();
                            ReleaseDBID(charID);
                            using (var delSkillCmd = new SqliteCommand("DELETE from PlayerCharacterSkills WHERE CharacterID=@charID", CachedConnection, transaction))
                            {
                                delSkillCmd.ExecuteNonQuery();
                            }
                            using (var delItemsCmd = new SqliteCommand("DELETE from PlayerCharacterItems WHERE CharacterID=@charID", CachedConnection, transaction))
                            {
                                delItemsCmd.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                        catch (SqliteException e)
                        {
                            Debug.LogError("Error deleting character: " + e.Message);
                            transaction.Rollback();
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }

            static bool LoadCharacterCache()
            {
                _characterCache.Clear();
                using (var reader = DatabaseHelper.GetCharactersLoadAllCommand(CachedConnection).ExecuteReader())
                {
                    try
                    {
                        while (reader.Read())
                        {
                            var pc = new DBPlayerCharacter
                            {
                                AccountID = reader.GetInt32(0),
                                DBID = reader.GetInt32(1),
                                Name = reader.GetString(2),
                                LastZoneID = reader.GetInt32(4),
                                Faction = reader.GetInt32(5),
                                ArcheType = reader.GetInt32(6),
                                PawnState = reader.GetInt32(7),
                                Appearance = new DBPlayerCharacter.DBAppearance(DatabaseHelper.DeserializeIntList(reader.GetString(3), 11).ToArray()),
                                Position = DatabaseHelper.DeSerializeVector3(reader.GetString(8)),
                                Rotation = DatabaseHelper.DeSerializeVector3(reader.GetString(9)),
                                FamePep = DatabaseHelper.DeserializeIntList(reader.GetString(10), 2).ToArray(),
                                Health = reader.GetInt32(11),
                                Money = reader.GetInt32(12),
                                ExtraBodyMindFocusAttributePoints = DatabaseHelper.DeserializeIntList(reader.GetString(13), 4).ToArray(),
                                SerializedSkillDeck = reader.GetString(14)
                            };
                            pc.FameLevelCache = LevelProgression.Get.GetLevelbyFamepoints(pc.FamePep[0]).level;
                            _characterCache.Add(pc);
                            SetDBIDAllocated(pc.DBID);
                        }
                        Debug.Log(string.Format("{0} Characters loaded", _characterCache.Count));
                        return true;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[DBLoadCharacters]: " + e.Message);
                        _characterCache.Clear();
                        return false;
                    }
                }
            }

            static bool LoadAndAssignSkills()
            {
                using (var reader = DatabaseHelper.GetCharacterAllSkillsLoadCommand(CachedConnection).ExecuteReader())
                {
                    try
                    {
                        while (reader.Read())
                        {
                            var charID = reader.GetInt32(0);
                            var sID = reader.GetInt32(1);
                            var p = GetCharacter(charID);
                            if (p == null)
                            {
                                Debug.LogWarning(string.Format("Skill with ID {0} in DB has no matching player character (ID {1})", sID, charID));
                                continue;
                            }
                            var sigilSlots = reader.GetInt32(2);
                            var dbs = new DBSkill(sID, sigilSlots);
                            p.Skills.Add(dbs);
                        }
                        return true;
                    }
                    catch (SqliteException e)
                    {
                        Debug.LogError("[LoadAssignSkills]: " + e.Message);
                        return false;
                    }
                }
            }

            static bool LoadAndAssignItems()
            {
                using (var reader = DatabaseHelper.GetCharacterAllItemsLoadCommand(CachedConnection).ExecuteReader())
                {
                    try
                    {
                        while (reader.Read())
                        {
                            var dbid = reader.GetInt32(1);
                            var charID = reader.GetInt32(0);
                            var p = GetCharacter(charID);
                            if (p == null)
                            {
                                Debug.LogWarning(string.Format("Item with ID {0} in DB has no matching player character (ID {1})", dbid, charID));
                                continue;
                            }
                            var resID = reader.GetInt32(2);
                            var it = GameData.Get.itemDB.GetItemType(resID);
                            if (it == null)
                            {
                                Debug.LogWarning("couldn't find EquipItem: " + resID);
                                continue;
                            }
                            var git = ScriptableObject.CreateInstance<Game_Item>();
                            git.Type = it;
                            git.DBID = dbid;
                            SetDBIDAllocated(git.DBID);
                            git.StackSize = reader.GetInt32(3);
                            git.CharacterID = p.DBID;
                            git.LocationType = (EItemLocationType) reader.GetInt32(4);
                            git.LocationSlot = reader.GetInt32(5);
                            git.Attuned = (byte) reader.GetInt32(6);
                            git.Color1 = (byte) reader.GetInt32(7);
                            git.Color2 = (byte) reader.GetInt32(8);
                            p.Items.Add(git);
                        }
                        return true;
                    }
                    catch (SqliteException e)
                    {
                        Debug.LogError("[LoadAssignItems]: " + e.Message);
                        return false;
                    }
                }
            }

            static bool LoadAndAssignQuests()
            {
                using (var reader = DatabaseHelper.GetCharacterAllQuestsLoadCommand(CachedConnection).ExecuteReader())
                {
                    try
                    {
                        while (reader.Read())
                        {
                            var charID = reader.GetInt32(0);
                            var qID = reader.GetInt32(1);
                            var isComp = reader.GetBoolean(2);
                            var tInd = reader.GetInt32(3);
                            var tProg = reader.GetInt32(4);
                            var p = GetCharacter(charID);
                            if (p == null)
                            {
                                Debug.LogWarning(string.Format("Quest with ID {0} in DB has no matching player character (ID {1})", qID, charID));
                                continue;
                            }

                            var dbq = new DBQuestTarget(qID, isComp, tInd);
                            dbq.targetProgress = tProg;
                            p.QuestTargets.Add(dbq);
                        }
                        return true;
                    }
                    catch (SqliteException e)
                    {
                        Debug.LogError("[LoadAssignQuests]: " + e.Message);
                        return false;
                    }
                }
            }

            static bool LoadAndAssignPerVars()
            {
                using (var reader = DatabaseHelper.GetCharacterPerVarsLoadCommand(CachedConnection).ExecuteReader())
                {
                    try
                    {
                        while (reader.Read())
                        {
                            var charID = reader.GetInt32(0);
                            var contextID = reader.GetInt32(1);
                            var varID = reader.GetInt32(2);
                            var value = reader.GetInt32(3);
                            var p = GetCharacter(charID);
                            if (p == null)
                            {
                                Debug.LogWarning(string.Format("Persistent variable with ID context {0}, var {1} in DB has no matching player character (ID {2})", contextID, varID, charID));
                                continue;
                            }

                            var dbv = new DBPersistentVar(contextID, varID, value);
                            p.PersistentVars.Add(dbv);
                        }
                        return true;
                    }
                    catch (SqliteException e)
                    {
                        Debug.LogError("[LoadAssignPerVars]: " + e.Message);
                        return false;
                    }
                }
            }

            public static bool SaveCharacterLogout(PlayerCharacter pc)
            {
                var ch = pc.dbRef;
                if (ch == null)
                {
                    Debug.LogError("Playercharacter " + pc.Name + "'s DBEntry to use for saving to db is null");
                    return false;
                }
                //check sessionkey to abort if newer connection is active
                if (!pc.Owner.IsSessionValid())
                {
                    Debug.LogWarning("Sessionkey for " + pc.Name + " is not valid. not saving logout to DB");
                    return false;
                }
                var pStats = pc.Stats as Game_PlayerStats;
                ch.Appearance = (pc.Appearance as Game_PlayerAppearance).ToDBCache();
                ch.LastZoneID = (int) pc.LastZoneID;
                ch.PawnState = (int) pc.PawnState;
                ch.Position = pc.Position;
                ch.Rotation = pc.Rotation.eulerAngles;
                ch.FamePep = new int[2] {pStats.FamePoints, pStats.PepPoints };
                ch.Health = (int) pc.Stats.mRecord.CopyHealth;
                ch.Money = pc.Money;
                ch.ExtraBodyMindFocusAttributePoints = new int[4] {0, 0, 0, pStats.RemainingAttributePoints};
                ch.Skills.Clear();
                for (var i = 0; i < pc.Skills.CharacterSkills.Count; i++)
                {
                    ch.Skills.Add(new DBSkill(pc.Skills.CharacterSkills[i].resourceID, pc.Skills.GetTokenSlots(pc.Skills.CharacterSkills[i])));
                }
                ch.SerializedSkillDeck = (pc.Skills as Game_PlayerSkills).DBSerializeDeck();
                for (var i = 0; i < ch.Items.Count; i++)
                {
                    if (ch.Items[i].DBID <= 0)
                    {
                        ch.Items[i].DBID = AllocateDBID();
                    }
                }

                ch.QuestTargets = pc.questData.SaveForPlayer();
                ch.PersistentVars = pc.persistentVars.SaveForPlayer();

                return SaveCharacterToDB(ch);
            }

            static bool SaveCharacterToDB(DBPlayerCharacter pc)
            {
                using (var transaction = CachedConnection.BeginTransaction())
                using (var updateExistingCmd = DatabaseHelper.PrepareCharacterUpdateExistingCommand(CachedConnection, pc, transaction))
                {
                    try
                    {
                        SaveSkills(pc, transaction);
                        SaveItems(pc, transaction);
                        SaveQuests(pc, transaction);
                        SavePerVars(pc, transaction);
                        if (updateExistingCmd.ExecuteNonQuery() > 0)
                        {
                            transaction.Commit();
                            return true;
                        }
                        using (var saveNewCmd = DatabaseHelper.GetCharacterSaveNewCommand(CachedConnection, pc, transaction))
                        {
                            saveNewCmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        return true;
                    }
                    catch (SqliteException e)
                    {
                        transaction.Rollback();
                        Debug.LogError("[SaveCharacter]: " + e.Message);
                        return false;
                    }
                }
            }

            static void SaveSkills(DBPlayerCharacter pc, SqliteTransaction tr)
            {
                using (var clearCmd = DatabaseHelper.GetCharacterSkillsDeleteCommand(CachedConnection, pc, tr))
                {
                    clearCmd.ExecuteNonQuery();
                }
                for (var i = 0; i < pc.Skills.Count; i++)
                {
                    if (pc.Skills[i] == null) continue;
                    using (var skillSaveCommand = DatabaseHelper.GetCharacterSkillSaveCommand(CachedConnection, pc, pc.Skills[i], tr))
                    {
                        skillSaveCommand.ExecuteNonQuery();
                    }
                }
            }

            static void SaveItems(DBPlayerCharacter pc, SqliteTransaction tr)
            {
                using (var clearCmd = DatabaseHelper.GetCharacterItemsDeleteCommand(CachedConnection, pc, tr))
                {
                    clearCmd.ExecuteNonQuery();
                }
                for (var i = 0; i < pc.Items.Count; i++)
                {
                    if (pc.Items[i] == null) continue;
                    using (var saveCommand = DatabaseHelper.GetCharacterItemSaveCommand(CachedConnection, pc, pc.Items[i], tr))
                    {
                        saveCommand.ExecuteNonQuery();
                    }
                }
            }

            static void SaveQuests(DBPlayerCharacter pc, SqliteTransaction tr)
            {
                using (var clearCmd = DatabaseHelper.GetCharacterQuestsDeleteCommand(CachedConnection, pc, tr))
                {
                    clearCmd.ExecuteNonQuery();
                }
                for (var i = 0; i < pc.QuestTargets.Count; i++)
                {
                    if (pc.QuestTargets[i] == null) continue;
                    using (var questSaveCommand = DatabaseHelper.GetCharacterQuestTargetSaveCommand(CachedConnection, pc, pc.QuestTargets[i], tr))
                    {
                        questSaveCommand.ExecuteNonQuery();
                    }
                }
            }

            static void SavePerVars(DBPlayerCharacter pc, SqliteTransaction tr)
            {
                using (var clearCmd = DatabaseHelper.GetCharacterPerVarsDeleteCommand(CachedConnection, pc, tr))
                {
                    clearCmd.ExecuteNonQuery();
                }
                for (var i = 0; i < pc.PersistentVars.Count; i++)
                {
                    if (pc.PersistentVars[i] == null) continue;
                    using (var perVarSaveCommand = DatabaseHelper.GetCharacterPerVarSaveCommand(CachedConnection, pc, pc.PersistentVars[i], tr))
                    {
                        perVarSaveCommand.ExecuteNonQuery();
                    }
                }
            }

            #region DBIDs

            static readonly HashSet<int> allocatedDBIDs = new HashSet<int> {0};

            static int AllocateDBID()
            {
                var id = 0;
                while (allocatedDBIDs.Contains(id))
                {
                    id += 1;
                }
                allocatedDBIDs.Add(id);
                return id;
            }

            static void ReleaseDBID(int id)
            {
                allocatedDBIDs.Remove(id);
            }

            public static bool SetDBIDAllocated(int id)
            {
                if (!allocatedDBIDs.Contains(id))
                {
                    allocatedDBIDs.Add(id);
                    return true;
                }
                Debug.LogError("ID collision, this should not happen");
                return false;
            }

            #endregion
        }

        #endregion

        #region _cachedConnection

        static bool Connect()
        {
            if (_cachedConnection == null)
            {
                var path = Directory.GetCurrentDirectory()+@"\Database.sqlite";
                _cachedConnection = new SqliteConnection(string.Format("Data Source={0};Version=3", path));
            }
            try
            {
                _cachedConnection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        static SqliteConnection _cachedConnection;

        public static SqliteConnection CachedConnection
        {
            get
            {
                if (_cachedConnection == null || _cachedConnection.State == ConnectionState.Closed)
                {
                    Connect();
                }
                return _cachedConnection;
            }
        }

        #endregion
    }
}