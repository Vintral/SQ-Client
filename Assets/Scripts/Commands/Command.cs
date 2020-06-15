using System;
using UnityEngine;

public class Command {
    protected PACKET_TYPE _type;
    protected bool _debug = true;

    public Command() {
        debug( "Created" );
    }

    virtual public byte[] Pack() {
        byte[] packet = new byte[ 2 ];
        packet[ PACKET_MAP.PACKET_ID ] = 0;
        packet[ PACKET_MAP.COMMAND ] = (byte)_type;
        return packet;
    }

    new public string ToString() {
        return GetType().Name;
    }

    protected void debug( string message, bool force = false, bool silence = false ) {
        if( silence ) return;
        if( _debug || force )
            Debug.Log( GetType().Name + ": " + message );
    }
}