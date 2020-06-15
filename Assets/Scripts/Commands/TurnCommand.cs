using UnityEngine;

public class TurnCommand : Command {
    protected byte _data;

    public TurnCommand( byte data ) {
        _type = PACKET_TYPE.TURN;
        _data = data;
    }

    override public byte[] Pack() {
        byte[] packet = new byte[ 3 ];
        packet[ PACKET_MAP.PACKET_ID ] = 0;
        packet[ PACKET_MAP.COMMAND ] = (byte)_type;        
        packet[ 2 ] = _data;
        return packet;
    }
}