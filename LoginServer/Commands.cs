using System;

namespace LoginServer
{
    internal static class Commands
    {
        public static void HandleCommand(string input)
        {
            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                Program.Exit();
            }
            if (input.Equals("restart", StringComparison.OrdinalIgnoreCase))
            {
                Program.StartServer(Program.Config);
                return;
            }
            if (input.Equals("connections", StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("Active Connections: " + Program.GetConnectionCount());
                return;
            }
            if (input.Equals("universes", StringComparison.OrdinalIgnoreCase))
            {
                var universes = Program.GetConnectedUniverses();
                Debug.Log("_____Connected_____");
                if (universes.Count == 0)
                {
                    Debug.Log("None");
                }
                for (var i = 0; i < universes.Count; i++)
                {
                    Debug.Log(universes[i].ToString());
                }
                Debug.Log("___________________");
            }
            if (input.Equals("message", StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("___________");
                Debug.Log(Program.Config.Message.Replace(@"\\n", Environment.NewLine));
                Debug.Log("___________");
                Debug.Log(@"(tip: '\\n' creates a linebreak)");
            }
            if (input.StartsWith("setmessage", StringComparison.OrdinalIgnoreCase))
            {
                var msg = input.Replace("setmessage ", string.Empty);
                Program.Config.Message = msg;
            }
            if (input.StartsWith("registeraccount", StringComparison.OrdinalIgnoreCase))
            {
                var parts = input.Split(' ');
                if (parts.Length == 5)
                {
                    var aName = parts[1];
                    var aPass = parts[2];
                    var aMail = parts[3];
                    var aLevel = 0;
                    int.TryParse(parts[4], out aLevel);
                    var res = DBAccess.RegisterAccont(aName, aPass, aMail, aLevel);
                    switch (res)
                    {
                        case 0:
                            Debug.Log("Account created", ConsoleColor.Green);
                            return;
                        case 1:
                            Debug.Log("Account already exists", ConsoleColor.Yellow);
                            return;
                        default:
                            Debug.Log("Unknown error while creating account");
                            break;
                    }
                }
                else
                {
                    Debug.Log("Format is: "+Environment.NewLine+"registeraccount Name Pass Email@provider.com [0-3] (0=player,2=GM,3=Admin)");
                }
            }
            if (input.Equals("saveconfig", StringComparison.OrdinalIgnoreCase))
            {
                Program.SaveConfig(Program.Config);
            }
        }

        static string _inputString = "";
        public static void HandleInput()
        {
            var info = Console.ReadKey(true);
            switch (info.Key)
            {
                case ConsoleKey.Enter:
                    if (_inputString.Length <= 0) break;
                    Console.WriteLine();
                    HandleCommand(_inputString);
                    _inputString = "";
                    break;
                case ConsoleKey.Backspace:
                    Console.Write(info.KeyChar);
                    Console.Write(" ");
                    Console.Write(info.KeyChar);
                    if (_inputString.Length > 0)
                    {
                        _inputString = _inputString.Remove(_inputString.Length - 1);
                    }
                    break;
                default:
                    Console.Write(info.KeyChar);
                    _inputString += info.KeyChar;
                    break;
            }
        }
    }
}
