# PacketEventArgs

Namespace: DataFlow

Contains event data for received packets.

```csharp
public class PacketEventArgs : System.EventArgs
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [EventArgs](https://docs.microsoft.com/en-us/dotnet/api/system.eventargs) → [PacketEventArgs](./dataflow.packeteventargs.md)

## Fields

### **Guid**

The sender GUID.

```csharp
public Guid Guid;
```

### **Packet**

The received packet.

```csharp
public Packet Packet;
```

### **Identifier**

The received packet identifier.

```csharp
public ushort Identifier;
```

## Constructors

### **PacketEventArgs(Guid, Packet, UInt16)**

Initializes the event data using the provided GUID, packet and identifier.

```csharp
public PacketEventArgs(Guid guid, Packet packet, ushort identifier)
```

#### Parameters

`guid` [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>
 The sender GUID.

`packet` [Packet](./dataflow.packet.md)<br>
 The received packet.

`identifier` [UInt16](https://docs.microsoft.com/en-us/dotnet/api/system.uint16)<br>
 The received packet identifier.
