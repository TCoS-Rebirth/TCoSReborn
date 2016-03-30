using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using MySql.Data.MySqlClient;
using Common;

namespace LoginServer
{
    public static class DBAccess
    {

        public static bool IsDatabaseAccessible()
        {
            return GetConnection().State == ConnectionState.Open;
        }

        static MySqlConnection _cacheConnection;
        static MySqlConnection GetConnection()
        {
            if (_cacheConnection == null || _cacheConnection.State == ConnectionState.Closed)
            {
                var c = Program.Config;
                var conString = string.Format("Server = {0}; Port = {1}; Database = {2}; Uid = {3}; Pwd = {4};", c.DatabaseAddress, c.DatabasePort, c.DatabaseName, c.DatabaseUsername, c.DatabasePassword);
                _cacheConnection = new MySqlConnection(conString);
                try
                {
                    _cacheConnection.Open();
                }
                catch (MySqlException e)
                {
                    Debug.Log(e.Message, ConsoleColor.Yellow);
                }
            }
            return _cacheConnection;
        }

        public static int GetSupportedClientVersion()
        {
            using (var connection = GetConnection())
            using (var cmd = new MySqlCommand("SELECT SupportedClientVersion FROM Server LIMIT 1", connection))
            {
                var ret = cmd.ExecuteScalar();
                if (ret != null)
                {
                    return (int)ret;
                }
                return -1;
            }
        }

        public static UserAccount GetAccount(string name)
        {
            using (var connection = GetConnection())
            using (var cmd = new MySqlCommand("SELECT * FROM Accounts where Name=@name", connection))
            {
                cmd.Parameters.AddWithValue("@name", name);
                using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    while (reader.Read())
                    {
                        return new UserAccount(
                            reader.GetInt32("ID"),
                            reader.GetString("Name"),
                            reader.GetString("Pass"),
                            reader.GetString("Email"),
                            reader.GetInt32("banned") == 1,
                            (AccountPrivilege)reader.GetInt32("Level"),
                            DateTime.ParseExact(reader.GetString("LastLogin"),"MM/dd/yyyy HH:mm:ss", null),
                            reader.GetInt32("IsOnline") == 1, 
                            reader.GetInt32("SessionKey"), 
                            reader.GetInt32("LastUniverse"));
                    }
                }
            }
            return null;
        }

        public static List<UniverseInfo> GetKnownUniverses()
        {
            var universes = new List<UniverseInfo>();
            using (var connection = GetConnection())
            using (var cmd = new MySqlCommand("SELECT * FROM KnownUniverses", connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var uInfo = new UniverseInfo(reader.GetInt32("ID"), reader.GetString("Name"), "NotYetSet", "NotYetSet", 0, AccountPrivilege.Player,
                            "127.0.0.1", -1);
                        universes.Add(uInfo);
                    }
                }
            }
            return universes;
        }

        public static void UpdateKnownUniverses(List<UniverseInfo> universes)
        {
            using (var connection = GetConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var deleteCmd = new MySqlCommand("DELETE FROM KnownUniverses", connection, transaction))
                    {
                        deleteCmd.ExecuteNonQuery();
                    }
                    using (var cmd = new MySqlCommand("INSERT INTO KnownUniverses (ID, Name) VALUES (@id, @name)", connection, transaction))
                    {
                        for (var i = 0; i < universes.Count; i++)
                        {
                            cmd.Parameters.AddWithValue("@id", universes[i].Id);
                            cmd.Parameters.AddWithValue("@name", universes[i].Name);
                            cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                    }
                    transaction.Commit();
                }
                catch (MySqlException e)
                {
                    Debug.Log(e.Message, ConsoleColor.Red);
                    transaction.Rollback();
                }
            }
        }

        public static bool SaveAccount(UserAccount acc)
        {
            using (var connection = GetConnection())
            using (var cmd = new MySqlCommand("UPDATE Accounts SET banned=@banned, Level=@level, LastLogin=@nowTime, IsOnline=@isOnline, SessionKey=@sessKey, LastUniverse=@lastUni WHERE ID=@id", connection))
            {
                cmd.Parameters.AddWithValue("@id", acc.UID);
                cmd.Parameters.AddWithValue("@banned", acc.Banned);
                cmd.Parameters.AddWithValue("@level", (int)acc.Level);
                cmd.Parameters.AddWithValue("@isOnline", acc.IsOnline);
                cmd.Parameters.AddWithValue("@nowTime", acc.LastLogin.ToString(CultureInfo.InvariantCulture));
                cmd.Parameters.AddWithValue("@sessKey", acc.SessionKey);
                cmd.Parameters.AddWithValue("@lastUni", acc.LastUniverse);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public static int RegisterAccont(string name, string pass, string email, int level)
        {
            using (var connection = GetConnection())
            using (var cmd = new MySqlCommand("SELECT count(*) FROM Accounts where Name=@name OR Email=@mail", connection))
            {
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@mail", email);
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0)
                {
                    return 1;
                }
            }
            using (var connection = GetConnection())
            using (var transaction = connection.BeginTransaction())
            using (var cmd = new MySqlCommand("INSERT INTO accounts (ID, Name, Pass, Email, banned, Level, LastLogin, IsOnline, SessionKey, LastUniverse)" +
                                                           " VALUES (0, @name, @pass, @mail, @banned, @level, @nowTime, @isOnline, @sessKey, @lastUni)", connection, transaction))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@pass", UserAccount.GetSha1Hash(pass));
                    cmd.Parameters.AddWithValue("@mail", email);
                    cmd.Parameters.AddWithValue("@banned", 0);
                    cmd.Parameters.AddWithValue("@level", level);
                    cmd.Parameters.AddWithValue("@isOnline", 0);
                    cmd.Parameters.AddWithValue("@nowTime", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                    cmd.Parameters.AddWithValue("@sessKey", 0);
                    cmd.Parameters.AddWithValue("@lastUni", 0);
                    var count = cmd.ExecuteNonQuery();
                    if (count == 1)
                    {
                        transaction.Commit();
                        return 0;
                    }
                    Debug.Log("Error creating account", ConsoleColor.Red);
                    transaction.Rollback();
                    return 2;
                }
                catch (MySqlException e)
                {
                    Debug.Log("Error creating account: " + e.Message, ConsoleColor.Red);
                    transaction.Rollback();
                    return 2;
                }
            }
        }
    }
}