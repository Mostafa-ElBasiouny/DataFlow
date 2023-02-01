// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace DataFlow;

/// <summary>
///     Provides functionality for managing a client.
/// </summary>
public class Client
{
    /// <summary>
    ///     The client connected peer.
    /// </summary>
    private readonly Peer? _peer;

    /// <summary>
    ///     Initializes a new client using the provided IP address and port.
    /// </summary>
    /// <param name="ipAddress"> The server IP address. </param>
    /// <param name="port"> The port number. </param>
    public Client(IPAddress ipAddress, ushort port)
    {
        try
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect(ipAddress, port);

            _peer = new Peer(tcpClient);
        }
        catch (Exception exception)
        {
            Trace.TraceError($"{exception.Message}.");
        }
    }

    /// <summary>
    ///     Invoked when the client gets connected.
    /// </summary>
    public event EventHandler<Guid>? Connected;

    /// <summary>
    ///     Invoked when the client gets disconnected.
    /// </summary>
    public event EventHandler<Guid>? Disconnected;

    /// <summary>
    ///     Invoked when a packet is received.
    /// </summary>
    public event EventHandler<Guid>? PacketReceived;

    /// <summary>
    ///     Connects the client.
    /// </summary>
    public void Connect()
    {
        Peer.Connected += Connected;
        Peer.Disconnected += Disconnected;
        Peer.PacketReceived += PacketReceived;

        _peer!.Connect();
    }

    /// <summary>
    ///     Disconnects the client.
    /// </summary>
    public void Disconnect()
    {
        _peer!.Disconnect();
    }

    /// <summary>
    ///     Sends a packet to the server.
    /// </summary>
    /// <param name="packet"> The packet to be sent. </param>
    public async Task Send(Packet packet)
    {
        await _peer!.SendAsync(packet);
    }
}