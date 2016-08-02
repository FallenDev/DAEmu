using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace DAEmu.Networking
{
    public abstract class Server<TClient>
        where TClient : GameClient, new()
    {
        public Random Rnd { get; } = new Random();
        private readonly Dictionary<uint, TClient> _clients = new Dictionary<uint, TClient>();
        private Socket _listener;
        private bool _listening;

        public void Start(int port)
        {
            _listening = true;
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(IPAddress.Any, port));
            _listener.Listen(100);
            _listener.BeginAccept(EndConnectClient, null);
        }

        public abstract void OnClientConnected(TClient client);
        public abstract void OnClientDisconnected(TClient client);

        private void EndConnectClient(IAsyncResult result)
        {
            var client = new TClient
            {
                Socket = new NetworkSocket(_listener.EndAccept(result)),
                Serial = (uint) Rnd.Next()%uint.MaxValue
            };

            if (client.Socket.Connected)
            {
                if (AddClient(client))
                {
                    ClientConnected(client);

                    SocketError error;

                    client.Socket.BeginReceiveHeader(EndReceiveHeader, out error, client);

                    if (error != SocketError.Success)
                    {
                        ClientDisconnected(client);
                    }
                }
                else
                {
                    ClientDisconnected(client);
                }
            }

            if (_listening)
            {
                _listener.BeginAccept(EndConnectClient, null);
            }
        }

        private bool AddClient(TClient prmClient)
        {
            return !_clients.ContainsKey(prmClient.Serial);
        }

        private void ClientConnected(TClient prmCllent)
        {
            _clients[prmCllent.Serial] = prmCllent;

            OnClientConnected(prmCllent);
        }

        private void ClientDisconnected(TClient prmClient)
        {
            if (prmClient.Socket == null || prmClient.Socket.Connected)
                return;

            prmClient.Socket.Disconnect(false);

            List<TClient> copy;
            lock (_clients)
            {
                copy = new List<TClient>(_clients.Values).ToList();
            }

            foreach (var client in copy.FindAll(
                i => i.Socket == null || !i.Socket.Connected))
            {
                _clients.Remove(client.Serial);
                OnClientDisconnected(client);
            }
        }

        private void EndReceiveHeader(IAsyncResult result)
        {
            var client = (TClient) result.AsyncState;
            SocketError error;
            var bytes = client.Socket.EndReceiveHeader(result, out error);

            if (bytes == 0 ||
                error != SocketError.Success)
            {
                ClientDisconnected(client);
                return;
            }

            if (client.Socket.HeaderComplete)
            {
                client.Socket.BeginReceivePacket(EndReceivePacket, out error, client);
            }
            else
            {
                client.Socket.BeginReceiveHeader(EndReceiveHeader, out error, client);
            }
        }

        private void EndReceivePacket(IAsyncResult result)
        {
            var client = (TClient) result.AsyncState;
            var error = SocketError.Success;
            var bytes = client.Socket.EndReceivePacket(result, out error);

            if (bytes == 0 ||
                error != SocketError.Success)
            {
                ClientDisconnected(client);
                return;
            }

            if (client.Socket.PacketComplete)
            {
                ClientDataReceived(client, client.Socket.ToPacket());

                client.Socket.BeginReceiveHeader(EndReceiveHeader, out error, client);
            }
            else
            {
                client.Socket.BeginReceivePacket(EndReceivePacket, out error, client);
            }
        }

        public abstract void ClientDataReceived(TClient client, NetworkPacket packet);
    }
}