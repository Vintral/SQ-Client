using UnityEngine;

public class SelectShipCommand : Command {
    protected int _data;

    public SelectShipCommand( int data ) {
        _type = PACKET_TYPE.SELECT_SHIP;
        _data = data;
    }

    override public byte[] Pack() {
        byte[] packet = new byte[ 4 ];
        packet[ PACKET_MAP.PACKET_ID ] = 0;
        packet[ PACKET_MAP.COMMAND ] = (byte)_type;
        packet[ 2 ] = (byte)( _data & 0xFF );
        packet[ 3 ] = (byte)( ( _data >> 8 ) & 0xFF );
        return packet;
    }
}