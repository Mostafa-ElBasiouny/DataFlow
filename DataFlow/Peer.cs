// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;

namespace DataFlow;

/// <summary>
///     Provides base functionality for network communications.
/// </summary>
public class Peer
{
    /// <summary>
    ///     The peer GUID.
    /// </summary>
    private readonly Guid _guid;

    /// <summary>
    ///     The peer TCP network service and stream.
    /// </summary>
    private readonly (TcpClient tcpClient, NetworkStream networkStream) _peer;

    /// <summary>
    ///     Packet handlers accessible by their identifier.
    /// </summary>
    private Dictionary<ushort, PacketHandler> _packetHandlers = new();

    /// <summary>
    ///     The peer asynchronous operations.
    /// </summary>
    private (Task task, CancellationTokenSource cancellationTokenSource) _task;

    /// <summary>
    ///     Initializes a new peer using the provided TCP client.
    /// </summary>
    /// <param name="tcpClient"> The peer TCP client. </param>
    /// <param name="guid"> The peer GUID. </param>
    public Peer(TcpClient tcpClient, Guid guid = default)
    {
        _guid = guid;
        _peer.tcpClient = tcpClient;
        _peer.networkStream = _peer.tcpClient.GetStream();

        FindPacketHandlers();
    }

    /// <summary>
    ///     Invoked when the peer gets connected.
    /// </summary>
    public static event EventHandler<Guid>? Connected;

    /// <summary>
    ///     Invoked when the peer gets disconnected.
    /// </summary>
    public static event EventHandler<Guid>? Disconnected;

    /// <summary>
    ///     Invoked when the peer receives a packet.
    /// </summary>
    public static event EventHandler<Guid>? PacketReceived;

    /// <summary>
    ///     Connects the peer.
    /// </summary>
    public void Connect()
    {
        _task.cancellationTokenSource = new CancellationTokenSource();
        _task.task = ReceiveAsync(_task.cancellationTokenSource.Token);

        Trace.TraceInformation($"Connection established with {_peer.tcpClient.Client.RemoteEndPoint}.");
        Connected?.Invoke(this, _guid);
    }

    /// <summary>
    ///     Disconnects the peer.
    /// </summary>
    public void Disconnect()
    {
        _task.cancellationTokenSource.Cancel();
        _task.task.Wait();

        Trace.TraceInformation($"Disconnected from {_peer.tcpClient.Client.RemoteEndPoint}.");
        Disconnected?.Invoke(this, _guid);
    }

    /// <summary>
    ///     Sends a packet to the connected peer.
    /// </summary>
    /// <param name="packet"> The packet to be sent. </param>
    public async Task SendAsync(Packet packet)
    {
        var buffer = packet.Serialize();

        if (buffer.Length > Packet.Size)
        {
            Trace.TraceError($"Dismissing packet with identifier {packet.ReadIdentifier()} due to size.");
            return;
        }

        await _peer.networkStream.WriteAsync(BitConverter.GetBytes(buffer.Length)).ConfigureAwait(false);
        await _peer.networkStream.WriteAsync(buffer).ConfigureAwait(false);
    }

    /// <summary>
    ///     Receives packets from connected peers.
    /// </summary>
    /// <param name="cancellationToken"> The operation cancellation token. </param>
    private async Task ReceiveAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[Packet.Size];

        try
        {
            while (true)
            {
                var readAsync = await _peer.networkStream.ReadAsync(buffer.AsMemory(0, sizeof(uint)), cancellationToken).ConfigureAwait(false);
                var length = BitConverter.ToUInt32(buffer, 0);
                var offset = 0;

                if (readAsync == 0) throw new Exception();

                do
                {
                    readAsync = await _peer.networkStream.ReadAsync(buffer.AsMemory(offset, (int)(length - offset)), cancellationToken)
                        .ConfigureAwait(false);

                    if (readAsync == 0) throw new Exception();

                    offset += readAsync;
                } while (offset < length);

                var receivedPacket = new Packet(buffer);
                if (_packetHandlers.TryGetValue(receivedPacket.ReadIdentifier(), out var packetHandler)) packetHandler(receivedPacket, _guid);

                Trace.TraceInformation($"Packet received from {_peer.tcpClient.Client.RemoteEndPoint}.");
                PacketReceived?.Invoke(this, _guid);
            }
        }
        catch (OperationCanceledException)
        {
            if (_peer.tcpClient.Connected)
            {
                _peer.tcpClient.Close();

                Trace.TraceError($"Peer {_peer.tcpClient.Client.RemoteEndPoint} ended connection.");
                Disconnected?.Invoke(this, _guid);
            }
        }
        catch (Exception)
        {
            Trace.TraceError($"Peer {_peer.tcpClient.Client.RemoteEndPoint} disconnected.");
            Disconnected?.Invoke(this, _guid);
        }
    }

    /// <summary>
    ///     Retrieves methods that has the <see cref="PacketHandlerAttribute" /> applied.
    /// </summary>
    private void FindPacketHandlers()
    {
        var methodInfos = PacketHandlerAttribute.FindPacketHandlers();

        _packetHandlers = new Dictionary<ushort, PacketHandler>(methodInfos.Length);

        foreach (var methodInfo in methodInfos)
        {
            var packetHandlerAttribute = methodInfo.GetCustomAttribute<PacketHandlerAttribute>();
            var packetHandler = Delegate.CreateDelegate(typeof(PacketHandler), methodInfo, false);

            if (packetHandlerAttribute == null || packetHandler == null) continue;
            if (_packetHandlers.ContainsKey(packetHandlerAttribute.Identifier)) continue;

            _packetHandlers.Add(packetHandlerAttribute.Identifier, (PacketHandler)packetHandler);
            Trace.TraceInformation($"Packet handler method for packet identifier {packetHandlerAttribute.Identifier} found.");
        }
    }

    /// <summary>
    ///     Encapsulates a packet handler method.
    /// </summary>
    /// <param name="packet"> The packet received. </param>
    /// <param name="guid"> The sender GUID. </param>
    private delegate void PacketHandler(Packet packet, Guid guid = default);
}