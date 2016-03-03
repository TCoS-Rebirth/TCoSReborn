using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Common;

namespace LoginServer
{
    internal class Program
    {
        const string DefaultConfigFileName = "Config.xml";
        static LoginServer _server;
        public static LoginServerConfiguration Config;

        static void Main(string[] args)
        {
            if (!TryLoadConfig(args))
            {
                Debug.Log("Config file not accessible (create one? y/n):", ConsoleColor.Yellow);
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    Console.Clear();
                    CreateEmptyConfig();
                    Debug.Log("Config file created (" + DefaultConfigFileName + " inside executable folder; open that in a text editor and adjust it)", ConsoleColor.Green);
                    Exit(true);
                }
                else
                {
                    Exit();
                }
            }
            else
            {
                Debug.Log("Configuration file loaded", ConsoleColor.Green);
            }
            if (!DBAccess.IsDatabaseAccessible())
            {
                Debug.Log("Database not accessible! (check app config)", ConsoleColor.Red);
                Exit(true);
            }
            else
            {
                Debug.Log("Database is accessible.", ConsoleColor.Green);
            }
            Debug.Log("Supported client version: " + DBAccess.GetSupportedClientVersion());
            StartServer(Config);
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            while (true)
            {
                UpdateServerQueue();
                Thread.Sleep(1);
                if (!Console.KeyAvailable) continue;
                Commands.HandleInput();
            }
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            Exit();
        }

        public static int GetConnectionCount()
        {
            return _server == null ? 0 : _server.GetConnectionCount();
        }

        public static List<UniverseInfo> GetConnectedUniverses()
        {
            return _server == null ? new List<UniverseInfo>() : _server.GetConnectedUniverses();
        }

        public static void StartServer(LoginServerConfiguration config)
        {
            if (_server != null)
            {
                StopServer();
            }
            Debug.Log("Starting Server..");
            _server = new LoginServer();
            if (!_server.StartServer(Config))
            {
                Debug.Log("Couldn't start Server (check Ip/ListenPort parameters)");
                Exit(true);
                return;
            }
            Debug.Log("Universe/launcher interface running on Port: " + config.ProxyServerListenPort);
            Debug.Log("Login server running on Port: " + config.ListenPort);
        }

        static void UpdateServerQueue()
        {
            if (_server != null)
            {
                _server.UpdateMessageQueue();
            }
        }

        public static void StopServer()
        {
            Debug.Log("Stopping server..");
            if (_server != null)
            {
                _server.ShutDown();
            }
            Debug.Log("Server shut down");
        }

        static bool TryLoadConfig(string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (!args[i].StartsWith("config", StringComparison.OrdinalIgnoreCase)) continue;
                var parts = args[i].Split(':');
                if (parts.Length > 1)
                {
                    return LoadConfigFromFile(parts[1]);
                }
            }
            return LoadConfigFromFile(Path.Combine(Environment.CurrentDirectory, DefaultConfigFileName));
        }

        static bool LoadConfigFromFile(string fileName)
        {
            if (!File.Exists(fileName)) return false;
            var serializer = new XmlSerializer(typeof (LoginServerConfiguration));
            using (var reader = XmlReader.Create(fileName))
            {
                if (serializer.CanDeserialize(reader))
                {
                    Config = serializer.Deserialize(reader) as LoginServerConfiguration;
                }
            }
            return Config != null;
        }

        static void CreateEmptyConfig()
        {
            var serializer = new XmlSerializer(typeof (LoginServerConfiguration));
            using (var streamWriter = new StreamWriter(Path.Combine(Environment.CurrentDirectory, DefaultConfigFileName)))
            {
                serializer.Serialize(streamWriter, new LoginServerConfiguration());
                streamWriter.Flush();
            }
        }

        public static void SaveConfig(LoginServerConfiguration config)
        {
            var serializer = new XmlSerializer(typeof(LoginServerConfiguration));
            using (var streamWriter = new StreamWriter(Path.Combine(Environment.CurrentDirectory, DefaultConfigFileName)))
            {
                serializer.Serialize(streamWriter, config);
                streamWriter.Flush();
            }
        }

        public static void Exit(bool confirmation = false)
        {
            Debug.Log("..exiting");
            StopServer();
            if (confirmation)
            {
                Console.ReadKey();
            }
            Environment.Exit(0);
        }
    }
}