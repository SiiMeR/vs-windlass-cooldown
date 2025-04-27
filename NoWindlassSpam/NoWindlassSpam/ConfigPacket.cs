using ProtoBuf;

namespace NoWindlassSpam;

[ProtoContract]
public class ConfigPacket
{
    [ProtoMember(1)] public required string[] ApplicableCrossbowIds;
}