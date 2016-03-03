using System;

namespace Common
{
    [Serializable]
    public class UniverseInfo
    {
        public readonly string Ip;
        public readonly string Name;
        public readonly int Port;
        public int Id;
        public string Language;
        public AccountPrivilege MinPrivilege;
        public int Population;
        public string Type;

        public UniverseInfo(int uid, string uname, string ulanguage, string utype, int population, AccountPrivilege minPrivilege, string ip, int port)
        {
            Id = uid;
            Name = uname;
            Language = ulanguage;
            Type = utype;
            Ip = ip;
            Port = port;
            Population = population;
            MinPrivilege = minPrivilege;
            Population = 0;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1} (language:{2}, type:{3}, address:{4}:{5}, access restriction: {6})", Id, Name, Language, Type, Ip, Port, MinPrivilege);
        }
    }
}