using UnityEngine;

public class StopDecelerateCommand : Command {
    protected byte _data;

    public StopDecelerateCommand() {
        _type = PACKET_TYPE.STOP_DECELERATE;
    }

    override public byte[] Pack() {
        byte[] packet = new byte[ 2 ];
        packet[ PACKET_MAP.PACKET_ID ] = 0;
        packet[ PACKET_MAP.COMMAND ] = (byte)_type;
        return packet;
    }
}