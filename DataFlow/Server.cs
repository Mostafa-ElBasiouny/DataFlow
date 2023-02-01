// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using System.Net;
using System.Net.Sockets;

namespace DataFlow;

/// <summary>
///     Provides functionality for managing a server.
/// </summary>
public class Server
{
    /// <summary>
    ///     Connected clients accessible by their GUID.
    /// </summary>
    private static readonly Dictionary<Guid, Peer> Clients = new();

    /// <summary>
    ///     The server TCP network listener.
    /// </summary>
    private readonly TcpListener _tcpListener;

    /// <summary>
    ///     Initializes a new server using the provided port.
    /// </summary>
    /// <param name="port"> The port number. </param>
    public Server(ushort port)
    {
        _tcpListener = new TcpListener(IPAddress.Any, port);

        Peer.Disconnected += (_, guid) => { Clients.Remove(guid); };
    }

    /// <summary>
    ///     Invoked when a client gets connected.
    /// </summary>
    public event EventHandler<Guid>? Connected;

    /// <summary>
    ///     Invoked when a client gets disconnected.
    /// </summary>
    public event EventHandler<Guid>? Disconnected;

    /// <summary>
    ///     Invoked when a packet is received.
    /// </summary>
    public event EventHandler<Guid>? PacketReceived;

    /// <summary>
    ///     Begins accepting incoming connections.
    /// </summary>
    public void Start()
    {
        Peer.Connected += Connected;
        Peer.Disconnected += Disconnected;
        Peer.PacketReceived += PacketReceived;

        _tcpListener.Start();
        _tcpListener.BeginAcceptTcpClient(ConnectCallback, null);
    }

    /// <summary>
    ///     Shuts down the server disconnecting all connected clients.
    /// </summary>
    public void Shutdown()
    {
        foreach (var peer in Clients.Values) peer.Disconnect();
    }

    /// <summary>
    ///     Sends a packet to a connected client.
    /// </summary>
    /// <param name="guid"> The client GUID. </param>
    /// <param name="packet"> The packet to be sent. </param>
    public async Task Send(Packet packet, Guid guid)
    {
        if (Clients.TryGetValue(guid, out var peer)) await peer.SendAsync(packet);
    }

    /// <summary>
    ///     Invoked when a connection is established.
    /// </summary>
    /// <param name="asyncResult"> The status of the operation. </param>
    private void ConnectCallback(IAsyncResult asyncResult)
    {
        var client = _tcpListener.EndAcceptTcpClient(asyncResult);
        var guid = Guid.NewGuid();

        Clients.Add(guid, new Peer(client, guid));
        Clients[guid].Connect();

        _tcpListener.BeginAcceptTcpClient(ConnectCallback, null);
    }
}