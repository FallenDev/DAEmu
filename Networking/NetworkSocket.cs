using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace DAEmu.Networking
{
    public class NetworkSocket : Socket
    {
        protected static readonly int ProcessId 
            = Process.GetCurrentProcess().Id;

        private readonly byte[] _header = new byte[0x0003];
        private readonly byte[] _packet = new byte[0x4096];

        private int _headerLength = 3;
        private int _headerOffset;
        private int _packetLength;
        private int _packetOffset;

        public bool HeaderComplete => _headerOffset == _headerLength;
        public bool PacketComplete => _packetOffset == _packetLength;

        public NetworkSocket(Socket socket)
            : base(socket.DuplicateAndClose(ProcessId))
        {
        }

        public IAsyncResult BeginReceiveHeader(AsyncCallback callback, out SocketError error, object state)
        {
            return BeginReceive(
                _header,
                _headerOffset,
                _headerLength - _headerOffset,
                SocketFlags.None,
                out error,
                callback,
                state);
        }

        public IAsyncResult BeginReceivePacket(AsyncCallback callback, out SocketError error, object state)
        {
            return BeginReceive(
                _packet,
                _packetOffset,
                _packetLength - _packetOffset,
                SocketFlags.None,
                out error,
                callback,
                state);
        }

        public int EndReceiveHeader(IAsyncResult result, out SocketError error)
        {
            var bytes = EndReceive(result, out error);

            if (bytes == 0 ||
                error != SocketError.Success)
                return 0;

            _headerOffset += bytes;

            if (!HeaderComplete)
                return bytes;

            _packetLength = (_header[1] << 8) | _header[2];
            _packetOffset = 0;

            return bytes;
        }

        public int EndReceivePacket(IAsyncResult result, out SocketError error)
        {
            var bytes = EndReceive(result, out error);

            if (bytes == 0 ||
                error != SocketError.Success)
                return 0;

            _packetOffset += bytes;

            if (!PacketComplete)
                return bytes;

            _headerLength = 3;
            _headerOffset = 0;

            return bytes;
        }

        public NetworkPacket ToPacket()
        {
            return PacketComplete
                ? new NetworkPacket(_packet, 0, _packetLength)
                : null;
        }
    }
}