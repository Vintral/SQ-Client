using System;
using UnityEngine;

public class Entity {
    protected bool _debug;
    protected float _x;
    protected float _y;
    protected float _angle;
    protected int _id;
    protected int _type;
    protected Byte _status;

    public float X {
        get { return _x; }
        set { _x = value; }
    }

    public float Y {
        get { return _y; }
        set { _y = value; }
    }

    public float Angle {
        get { return _angle; }
    }

    public int ID {
        get { return _id; }
    }

    public int Type {
        get { return _type; }
    }

    public Byte Status {
        get { return _status; }
    }

    public Entity( Byte[] data ) {
        _debug = false;
        debug( "Created" ); 

        _id = ( data[ 1 ] << 8 ) + data[ 0 ];
        _status = data[ 2 ];
        _type = ( data[ 4 ] << 8 ) + data[ 3 ];
        _x =  ( ( data[ 6 ] << 8 ) + data[ 5 ] ) / 100f;
        _y = ( ( data[ 8 ] << 8 ) + data[ 7 ] ) / 100f;
        _angle = ( ( data[ 10 ] << 8 ) + data[ 9 ] ) / 100f;

        //debug( "Angle: " + Angle, true );
    }

    public Entity( int id, int type, int x, int y, int angle ) {
        _debug = false;
        debug( "Created" );

        _id = id;
        _type = type;
        _x = x;
        _y = y;
        _angle = angle;
    }

    protected void debug( string msg, bool force = false, bool silence = false ) {
        if( silence ) return;
        if( _debug || force )
            Debug.Log( GetType().Name + ": " + msg );
    }
}