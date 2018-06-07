using System;
using System.Security.Cryptography;
using System.Text;

namespace Common
{
    /// <summary>
    ///     Represents a User with its rights and access information
    /// </summary>
    public class UserAccount
    {
        public readonly string EMail;
        public readonly string Name;
        public readonly string PasswordHash;

        /// <summary>
        ///     A unique account ID
        /// </summary>
        public readonly int UID;

        /// <summary>
        ///     Whether this account is allowed to play on the game server
        /// </summary>
        public bool Banned;

        public bool IsOnline;

        public DateTime LastLogin;
        public int LastUniverse;
        public AccountPrivilege Level;
        public int SessionKey;

        public UserAccount(int id, string name, string pass, string mail, bool banned, AccountPrivilege accPrivileges, DateTime lastLogin, bool isOnline,
            int sessionKey, int lastUniverse)
        {
            UID = id;
            Name = name;
            PasswordHash = pass;
            EMail = mail;
            Banned = banned;
            Level = accPrivileges == AccountPrivilege.Error ? AccountPrivilege.Player : accPrivileges;
            LastLogin = lastLogin;
            IsOnline = isOnline;
            SessionKey = sessionKey;
            LastUniverse = lastUniverse;
        }

        public static string GetSha1Hash(string input)
        {
            return input;
            //not needed in sandbox mode
            //using (var sha1 = new SHA1Managed())
            //{
            //    var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            //    var sb = new StringBuilder(hash.Length*2);
            //    for (var i = 0; i < hash.Length; i++)
            //    {
            //        sb.Append(hash[i].ToString("X2"));
            //    }
            //    return sb.ToString();
            //}
        }
    }
}