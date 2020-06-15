using UnityEngine;

public class DecelerateCommand : Command {
    protected byte _data;

    public DecelerateCommand() {
        _type = PACKET_TYPE.DECELERATE;
    }

    override public byte[] Pack() {
        byte[] packet = new byte[ 2 ];
        packet[ PACKET_MAP.PACKET_ID ] = 0;
        packet[ PACKET_MAP.COMMAND ] = (byte)_type;        
        return packet;
    }
}