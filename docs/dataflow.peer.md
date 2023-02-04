# Peer

Namespace: DataFlow

Provides base functionality for network communications.

```csharp
public class Peer
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Peer](./dataflow.peer.md)

## Constructors

### **Peer(TcpClient, Guid)**

Initializes a new peer using the provided TCP client.

```csharp
public Peer(TcpClient tcpClient, Guid guid)
```

#### Parameters

`tcpClient` TcpClient<br>
 The peer TCP client.

`guid` [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>
 The peer GUID.

## Methods

### **Connect()**

Connects the peer.

```csharp
public void Connect()
```

### **Disconnect()**

Disconnects the peer.

```csharp
public void Disconnect()
```

### **SendAsync(Packet)**

Sends a packet to the connected peer.

```csharp
public Task SendAsync(Packet packet)
```

#### Parameters

`packet` [Packet](./dataflow.packet.md)<br>
 The packet to be sent.

#### Returns

[Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task)<br>

## Events

### **Connected**

Invoked when the peer gets connected.

```csharp
public static event EventHandler<Guid> Connected;
```

### **Disconnected**

Invoked when the peer gets disconnected.

```csharp
public static event EventHandler<Guid> Disconnected;
```

### **PacketReceived**

Invoked when the peer receives a packet.

```csharp
public static event EventHandler<PacketEventArgs> PacketReceived;
```
