using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class Connection {
    private bool _debug = false;

    private UdpClient _udpClient;
    private readonly Queue<Byte[]> incomingQueue = new Queue<Byte[]>();
    private string _ip = "13.57.182.181";
    List<Byte[]> _pendingMessages = new List<Byte[]>();
    private int _sendPort = 8034;
    private int _receivePort = 8066;
    private bool _loggedIn = false;    
    private int stateCounter = 0;
    Byte[] _entityData;
    const int _entityDataSize = 11;
    private Hashtable entities = new Hashtable();
    private int _key;
    Thread receiveThread;
    private bool threadRunning = false;
    private static bool _constructing = false;

    private DateTime _originTime;

    private byte _packetID;
    private Dictionary<byte,byte[]> _reliablePackets;
    private Dictionary<byte, UInt32> _receivedPackets;
    private List<byte> _receivedPacketIDs;

    public int Key {
        get { return _key; }
    }

    private static Connection _instance;
    public static Connection Instance {
        get {
            if( _instance == null ) {
                _constructing = true;
                _instance = new Connection();
            }
            return _instance;
        }
    }

    #region Events
    public event Event KeyReceived;
    public event Event StateReceived;
    public event Event Error;
    #endregion

    public static int Angle;

    public Connection() {
        if( !_constructing )
            throw ( new System.Exception( "Should Not Be Instantiated" ) );
        _constructing = false;

        _reliablePackets = new Dictionary<byte,byte[]>();
        _receivedPackets = new Dictionary<byte, uint>();
        _receivedPacketIDs = new List<byte>();
        
        _entityData = new Byte[ _entityDataSize ];

        _originTime = new DateTime( 1970, 1, 1 );

        startConnection();
    }

    public static void Initialize() {
        
    }

    //==================================//
    //  Dispatcher                      //
    //==================================//
    private void dispatchError( string msg ) {
        debug( "dispatchError: " + msg );
        Error?.Invoke( this, new GameArgs( msg ) );        
    }

    //==================================//
    //  Connection Handling             //
    //==================================//
    private void startConnection() {
        debug( "startConnection" );

        try { 
            _udpClient = new UdpClient( _receivePort ); 
        } catch( Exception e ) {            
            dispatchError( "Failed to listen for UDP at port " + _receivePort + ": " + e.Message );
            return;
        }

        debug( "Connected" );
        
        startReceiveThread();
    }

    private void startReceiveThread() {
        debug( "startReceiveThread" );

        receiveThread = new Thread( () => ListenForMessages( _udpClient ) );
        receiveThread.IsBackground = true;
        threadRunning = true;
        receiveThread.Start();
    }

    private void checkReliableMessages() {
        debug( "checkReliableMessages", false, true );

        // Iterate over any pending packets
        foreach( var packet in _reliablePackets.Values )
            Send( packet );
    }

    private void checkReceivedPackets() {
        // Grab current ticks
        var current = (UInt32)( DateTime.UtcNow.Subtract( _originTime ) ).TotalSeconds;
        var span = 3;
        var diff = current - span;

        /*List<Byte> keys = new List<Byte>( _receivedPackets.Keys );
        keys.ForEach( key => {
            if( _receivedPackets[ key ] < current - span ) {
                _receivedPackets.Remove( key );
            }
        } );*/

        lock( _receivedPackets ) {
            _receivedPacketIDs.Clear();
            _receivedPacketIDs.AddRange( _receivedPackets.Keys );
            var length = _receivedPacketIDs.Count;
            for( int i = 0; i < length; i++ ) {
                if( _receivedPackets[ _receivedPacketIDs[ i ] ] < diff )
                    _receivedPackets.Remove( _receivedPacketIDs[ i ] );
            }            
        }        

        /*lock( _receivedPackets ) {
            foreach( var key in keys ) {
                // See if our record is older than the current time minus the span
                
            }
        }*/
    }

    private void ListenForMessages( UdpClient client ) {
        IPEndPoint remoteIpEndPoint = new IPEndPoint( IPAddress.Any, 0 );

        while( threadRunning ) {
            try {
                Byte[] receiveBytes = client.Receive( ref remoteIpEndPoint ); // Blocks until a message returns on this socket from a remote host.                

                lock( incomingQueue ) {
                    incomingQueue.Enqueue( receiveBytes );
                }
            } catch( SocketException e ) {                                
                dispatchError( "Socket Closed" );
            } catch( Exception e ) {                
                dispatchError( e.Message );
            }
            Thread.Sleep( 1 );
        }
    }

    public void Update() {
        debug( "Update", false, true );

        if( _instance == null ) return;        
        
        getMessages();
        PACKET_TYPE type;
        int length = _pendingMessages.Count;
        byte[] msg;
        for( int i = 0; i < length; i++ ) {
            msg = _pendingMessages[ i ];
            //debug( "MSG: " + msg );

            type = (PACKET_TYPE)msg[ PACKET_MAP.COMMAND ];
            if( type == PACKET_TYPE.ACK ) {
                //debug( "WE HAVE AN ACK: " + msg[ PACKET_MAP.PACKET_ID ] );
                _reliablePackets.Remove( msg[ 0 ] );
            } else {
                // If it has an ID it may be a dupe, make sure it's not
                // State packets use the id field for order purposes, so ignore those
                if( msg[ PACKET_MAP.PACKET_ID ] > 0 && type != PACKET_TYPE.STATE ) {                    
                    lock( _receivedPackets ) {
                        // See if we have a record of this packet, if so bail
                        if( _receivedPackets.ContainsKey( msg[ PACKET_MAP.PACKET_ID ] ) ) return;

                        // This is a new packet, store a record of it with current ticks                        
                        _receivedPackets[ msg[ PACKET_MAP.PACKET_ID ] ] = (UInt32)( DateTime.UtcNow.Subtract( _originTime ) ).TotalSeconds;
                    }

                    //Respond with an ACK
                    SendAckPacket( msg[ PACKET_MAP.PACKET_ID ] );                    
                }

                // Not a dupe packet so process it
                switch( type ) {
                    case PACKET_TYPE.ADDRESS:
                        debug( "WE HAVE AN ADDRESS" );
                        String address = String.Empty;
                        for( var n = 1; n < msg.Length; n++ ) {
                            address += (char)msg[ n ];
                        }
                        debug( "ADDRESS: " + address );

                        var m = address.LastIndexOf( ":" );
                        _sendPort = Convert.ToInt32( address.Substring( m + 1 ) );
                        _ip = address.Substring( 0, m );

                        debug( "IP: " + _ip + "   Port: " + _sendPort );

                        _ip = "13.57.182.181";

                        _loggedIn = true;

                        SendCommand( new JoinCommand() );
                        debug( "SENT JOIN" );

                        break;
                    case PACKET_TYPE.INITIALIZE:
                        //int key = ( msgs[ i ][ 1 ] << 8 ) + msgs[ i ][ 2 ];
                        //debug( "Key: " + key );
                        //_key = key;

                        //KeyReceived?.Invoke( this, new GameArgs( _key ) );
                        break;
                    case PACKET_TYPE.READY:
                        debug( "PROCESS READY" );
                        _key = msg[ PACKET_MAP.CLIENT_ID_1 ] + ( msg[ PACKET_MAP.CLIENT_ID_2 ] << 8 );
                        break;
                    case PACKET_TYPE.STATE:
                        debug( "PROCESS STATE" );
                        try {                            
                            int size = msg.Length;
                            size = ( size - 2 ) / _entityDataSize;

                            if( msg[ 0 ] > stateCounter || msg[ 0 ] < stateCounter - 200 ) {
                                /*String output = String.Empty;
                                for( int n = 0; n < msg.Length; n++ )
                                    output += msg[ n ] + " ";
                                //debug( "STATE: " + output, true );*/

                                stateCounter = msg[ 0 ];                                
                                Entity e;
                                entities.Clear();
                                for( int n = 0; n < size; n++ ) {                                    
                                    getData( msg, 2 + ( n * _entityDataSize ), _entityData );
                                    e = new Entity( _entityData );                                    
                                    entities.Add( e.ID, e );
                                }
                                
                                StateReceived?.Invoke( this, new GameArgs( entities ) );
                            } else debug( "WE HAVE AN OLD STATE: " + msg[ 0 ] + " :: " + stateCounter );
                        } catch( Exception ex ) {
                            debug( "ERROR: " + ex.Message );
                        }
                        break;
                    case PACKET_TYPE.JOINED:
                        debug( "PROCESS JOINED" );
                        SendCommand( new SelectShipCommand( 1 ) );
                        break;
                    default: debug( "Unrecognized Packet: " + type ); break;
                }
            }
        };

        checkReliableMessages();
        checkReceivedPackets();
    }

    public void getData( Byte[] source, int start, Byte[] output ) {
        if( source.Length < start + _entityDataSize ) return;
        
        for( var i = 0; i < _entityDataSize; i++ )
            output[ i ] = source[ start + i ];
    }   

    void getMessages() {
        _pendingMessages.Clear();
        lock( incomingQueue ) {
            while( incomingQueue.Count != 0 ) {
                _pendingMessages.Add( incomingQueue.Dequeue() );                
            }
        }        
    }

    public void Dispose() {
        debug( "Dispose" );

        if( _udpClient != null ) {
            SendCommand( new DisconnectCommand() );
            
            threadRunning = false;
            receiveThread.Abort();
            _udpClient.Close();
        }
    }

    public void SendReliable( byte[] payload ) {
        payload[ 0 ] = _packetID++;
        if( payload[ 0 ] == 0 ) payload[ 0 ] = _packetID++;

        _reliablePackets[ (byte)( _packetID - 1 ) ] = payload;

        Send( payload );
    }

    public void Send( byte[] payload ) {
        debug( "Send", false, true );

        IPEndPoint serverEndpoint = new IPEndPoint( IPAddress.Parse( _ip ), _sendPort );        
        _udpClient.Send( payload, payload.Length, serverEndpoint );
    }

    private void SendAckPacket( byte pid ) {
        debug( "SendAckPacket" );

        byte[] payload = new byte[ 2 ];
        payload[ 0 ] = pid;
        payload[ 1 ] = (byte)PACKET_TYPE.ACK;

        Send( payload );
    }

    public void SendCommand( Command command ) {
        debug( "SendCommand: " + command.ToString() );
        SendReliable( command.Pack() );
    }

    protected void debug( string msg, bool force = false, bool silence = false ) {
        if( silence ) return;
        if( _debug || force )
            Debug.Log( "Connection: " + msg );
    }
}