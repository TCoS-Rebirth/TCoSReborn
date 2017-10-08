using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
#if UNITY
using UnityEngine;
#endif

namespace Network
{
    public class NetConnector
    {
        public delegate void ConnectionDelegate(NetConnection connection);

        readonly List<NetConnection> _connections = new List<NetConnection>();

        readonly Queue<Message> _messageQueueRef;
        readonly int _port;

        readonly ManualResetEvent _waitHandler = new ManualResetEvent(false);

        Socket _listener;

        BackgroundWorker _queueWorker;

        bool _shutDownRequested;

        Thread _thread;

        public NetConnector(int loginPort, Queue<Message> incomingMessages)
        {
            _messageQueueRef = incomingMessages;
            try
            {
                _port = loginPort;
            }
            catch (FormatException)
            {
                throw new ArgumentException("ListenIP adress was not in the right format");
            }
        }

        public bool IsRunning
        {
            get { return _thread != null & _listener.IsBound; }
        }

        public int GetConnectionCount()
        {
            lock (_connections)
            {
                return _connections.Count;
            }
        }

        public NetConnection GetConnection(Predicate<NetConnection> condition)
        {
            lock (_connections)
            {
                for (var i = 0; i < _connections.Count; i++)
                {
                    if (condition(_connections[i]))
                    {
                        return _connections[i];
                    }
                }
            }
            return null;
        }

        public event ConnectionDelegate OnConnected;
        public event ConnectionDelegate OnDisconnected;

        public bool Start()
        {
            _thread = new Thread(Listen) {Name = string.Format("TcpServer:{0}", _port)};
            _thread.Start();
            _queueWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
            _queueWorker.DoWork += ResolveSendMessageQueues;
            _queueWorker.RunWorkerAsync();
            return true;
        }

        public void Shutdown()
        {
            if (_queueWorker != null)
            {
                _queueWorker.CancelAsync();
            }
            if (_thread != null)
            {
                _shutDownRequested = true;
                if (_listener.Connected)
                {
                    _listener.Shutdown(SocketShutdown.Both);
                }
                _listener.Close();
                _waitHandler.Set();
                lock (_connections)
                {
                    for (var i = 0; i < _connections.Count; i++)
                    {
                        HandleDisconnect(_connections[i]);
                    }
                }
                _thread.Abort();
                if (_thread != null)
                {
                    _thread.Join(1000);
                }
            }
            lock (_connections)
            {
                _connections.Clear();
            }
        }

        void Listen()
        {
            var localEndPoint = new IPEndPoint(IPAddress.Any, _port);
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _listener.Bind(localEndPoint);
                _listener.Listen(100);
                while (!_shutDownRequested)
                {
                    if (Thread.CurrentThread.ThreadState == ThreadState.StopRequested || Thread.CurrentThread.ThreadState == ThreadState.Aborted)
                    {
                        break;
                    }
                    if (_shutDownRequested) break;
                    _waitHandler.Reset();
                    try
                    {
                        _listener.BeginAccept(AcceptConnection, _listener);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("NetConnector (acceptRoutine): "+e.Message);
                        break;
                    }
                    _waitHandler.WaitOne();
                }
            }
            catch (Exception e)
            {
                Debug.Log("NetConnector: "+e.Message);
            }
        }

        void AcceptConnection(IAsyncResult res)
        {
            _waitHandler.Set();
            var listenerSocket = (Socket) res.AsyncState;
            lock (_connections)
            {
                try
                {
                    var clientSocket = listenerSocket.EndAccept(res);
                    if (clientSocket != null)
                    {
                        var nc = new NetConnection(clientSocket);
                        clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                        _connections.Add(nc);
                        nc.SetupReceiveState(NetConnection.ReceiveState.Header);
                        clientSocket.BeginReceive(nc.incomingReadBuffer, 0, 4, SocketFlags.None, ReadMessageCallback, nc);
                    }
                }
                catch (Exception e)
                {
                    if (!(e is ObjectDisposedException))
                    {
                        Debug.Log("NetConnector (accepting): " + e.Message);
                    }
                }
            }
        }

        void ReadMessageCallback(IAsyncResult ar)
        {
            var connection = ar.AsyncState as NetConnection;
            if (connection == null || !connection.ClientSocket.Connected)
            {
                if (connection != null)
                {
                    HandleDisconnect(connection);
                }
                return;
            }
            try
            {
                var bytesRead = connection.ClientSocket.EndReceive(ar);
                if (bytesRead == 0)
                {
                    HandleDisconnect(connection);
                    return;
                }
            }
            catch (Exception)
            {
                HandleDisconnect(connection);
                return;
            }
            if (connection.State == NetConnection.ReceiveState.Header)
            {
                connection.SetupReceiveState(NetConnection.ReceiveState.Body);
                if (connection.incomingMessageSize > 0)
                {
                    connection.ClientSocket.BeginReceive(connection.incomingReadBuffer, 0, connection.incomingMessageSize, SocketFlags.None, ReadMessageCallback,
                        connection);
                }
                else
                {
                    var m = connection.ReceiveMessage();
                    DispatchReceivedMessage(m);
                }
            }
            else
            {
                var m = connection.ReceiveMessage();
                DispatchReceivedMessage(m);
            }
        }

        void DispatchReceivedMessage(Message message)
        {
            switch (message.Header)
            {
                case 0xFFFD:
                    HandleConnect(message);
                    break;
                case 0xFFFE:
                    HandleDisconnect(message.Connection);
                    break;
                default:
                    HandleUserData(message);
                    break;
            }
            if (!message.Connection.ClientSocket.Connected)
            {
                return;
            }
            //Start listening for next message
            try
            {
                message.Connection.SetupReceiveState(NetConnection.ReceiveState.Header);
                message.Connection.ClientSocket.BeginReceive(message.Connection.incomingReadBuffer, 0, 4, 0, ReadMessageCallback, message.Connection);
            }
            catch (Exception e)
            {
                Debug.Log("NetConnector (dispatchMessage): "+ e.Message);
            }
        }

        void HandleConnect(Message m)
        {
            if (OnConnected != null)
            {
                OnConnected(m.Connection);
            }
        }

        void HandleDisconnect(NetConnection connection)
        {
            if (connection.ClientSocket != null)
            {
                if (connection.ClientSocket.Connected)
                {
                    connection.ClientSocket.Shutdown(SocketShutdown.Both);
                }
                connection.ClientSocket.Close();
            }
            if (OnDisconnected != null)
            {
                OnDisconnected(connection);
            }
            lock (_connections)
            {
                _connections.Remove(connection);
            }
#if UNITY
            connection.player = null;
#endif
        }

        void HandleUserData(Message m)
        {
            lock (_messageQueueRef)
            {
                _messageQueueRef.Enqueue(m);
            }
        }

        void ResolveSendMessageQueues(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "NetconnectorSendQueue";
            }
            while (!worker.CancellationPending & !_shutDownRequested)
            {
                lock (_connections)
                {
                    for (var i = _connections.Count; i-- > 0;)
                    {
                        if (_connections[i].MessageQueue.Count <= 0) continue;
                        var messageBytes = new List<byte>();
                        lock (_connections[i].MessageQueue)
                        {
                            while (_connections[i].MessageQueue.Count > 0)
                            {
                                if (worker.CancellationPending || _shutDownRequested)
                                {
                                    return;
                                }
                                var m = _connections[i].MessageQueue.Dequeue();
                                if (m != null)
                                {
                                    messageBytes.AddRange(m.FinalizeForSending());
                                }
                            }
                        }//message queue lock
                        _connections[i].ClientSocket.Send(messageBytes.ToArray());
                    } //loop
                } // connections lock
                Thread.Sleep(100);
            } //while
        }
    }
}