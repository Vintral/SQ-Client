using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class Game : MonoBehaviour {
    private GameObject _background;
    private Ship _ship;
    private int _key;

    private Hashtable _prevState;
    private Hashtable _currentState;

    private UInt32 _prevTime;
    private UInt32 _curTime;    

    //private UdpConnection _connection;
    private bool _debug = true;

    protected float _diff;    

    private Ship _player;

    [SerializeField]
    public List<GameObject> Ships;

    [SerializeField]
    public List<GameObject> Missiles;

    [SerializeField]
    public Camera BackgroundCamera;

    Hashtable _ships = new Hashtable();
    private Connection _connection;    

    // Start is called before the first frame update
    void Start() {
        debug( "Start" );

        /* NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        int reliable = config.AddChannel( QosType.Reliable );
        int unreliable = config.AddChannel( QosType.Unreliable );

        HostTopology topology = new HostTopology( config, 10 );
        int host = NetworkTransport.AddHost( topology, 8888 );

        byte[] data = Encoding.ASCII.GetBytes( "Hello World" ); 
        byte error;
        int connectionID = NetworkTransport.Connect( host, "104.236.71.139", 8888, 0, out error );
        NetworkTransport.Send( host, connectionID, reliable, byte );//te, byte.length, out error );
        NetworkTransport.Disconnect( host, connectionID, out error);*/

        //QualitySettings.vSyncCount = 1;

        //Connection.Instance.
        Connection.Instance.KeyReceived += onKeyReceived;
        Connection.Instance.StateReceived += onStateReceived;
        Connection.Instance.SendCommand( new InitializeCommand() );

        _background = GameObject.FindWithTag( "Background" );

        _prevTime = 0;
        _curTime = 0;

        //  _player = spawnShip( new Entity( 0, 1, 0, 0, 0 ) );
    }

    private void onKeyReceived( object sender, GameArgs args ) {
        debug( "onKeyReceived: " + (int)args.Data );
        _key = (int)args.Data;
    }

    private void processStates() {
        //debug( "TIME DIFFERENCE: " + ( _curTime - _prevTime ) + "ms", true );

        /*Entity player = (Entity)_prevState[ Connection.Instance.Key ];
        float x = player != null ? player.X : 0;
        float y = player != null ? player.Y : 0;*/

        /*if( player != null && _player != null ) {
            _player.transform.rotation = Quaternion.Euler( 0, player.Angle, 0 );
        }*/

        Entity e;
        ICollection keys = _prevState.Keys;
        foreach( int key in keys ) {
            /*if( key == Connection.Instance.Key )
                continue;*/

            e = (Entity)_prevState[ key ];
            //e.X -= x;
            //e.Y -= y;            

            if( !_ships.ContainsKey( key ) ) {
                _ships[ key ] = spawnShip( e );
                if( key == Connection.Instance.Key )
                    _player = _ships[ key ] as Ship;
            }

            /*else {
                ( _ships[ key ] as GameObject ).transform.rotation = Quaternion.Euler( 0, e.Angle, 0 );
                ( _ships[ key ] as GameObject ).transform.position = new Vector3( e.X / 100f, 0, e.Y / 100f );
            }*/            

            ( _ships[ key ] as Ship ).setState( e.X, e.Y, e.Angle, e.Status );
        }
    }

    private void onStateReceived( object sender, GameArgs args ) {
        Hashtable data = (Hashtable)args.Data;
        //debug( "onStateReceived: " + data.Count, true );

        //updatePlayer( (Entity)data[ _key ] );

        _prevTime = _curTime;
        _curTime = (UInt32)( DateTime.UtcNow.Subtract( new DateTime( 1970, 1, 1 ) ) ).TotalMilliseconds;

        if( _currentState != null ) _prevState = _currentState;
        _currentState = data;
        if( _currentState != null && _prevState != null ) processStates();
        else {
            if( _currentState == null ) debug( "CURRENT STATE IS NULL", true );
            if( _prevState == null ) debug( "PREV STATE IS NULL", true );
        }
    }

    private Ship spawnShip( Entity data ) {
        debug( "spawnShip: " + data.Type );

        GameObject obj;
        if( data.Type <= 12 ) obj = (GameObject)Instantiate( Ships[ data.Type ], new Vector3( data.X, 0, data.Y ), Quaternion.Euler( 0, data.Angle, 0 ) );
        else obj = (GameObject)Instantiate( Missiles[ data.Type - 12 ], new Vector3( data.X, 0, data.Y ), Quaternion.Euler( 0, data.Angle, 0 ) );

        obj.SetActive( true );
        obj.transform.localScale = new Vector3( 0.1f, 0.1f, 0.1f );

        var ship = obj.AddComponent<Ship>();
        var particles = obj.AddComponent<ParticleSystem>();

        return ship;
    }

    private void updatePlayer( Entity data ) {
        if( data == null ) return;

        if( _ship == null ) {
            /*_ship = spawnShip( data );
            _ship.transform.localScale = new Vector3( 0.1f, 0.1f, 0.1f );
            _ship.transform.rotation = Quaternion.Euler( 0, data.Angle, 0 );
            //_ship.SetActive( true );*/
        }
    }

    // Update is called once per frame
    void Update() {
        debug( "Update", false, true );

        // Calculate how long it's been in MS
        _diff = Time.deltaTime * 1000;        

        Connection.Instance.Update();

        if( Input.GetKeyDown( "right" ) ) {
            debug( "START RIGHT TURN" );

            if( _player != null ) _player.TurningRight = true;
            Connection.Instance.SendCommand( new TurnCommand( 1 ) );
        }
        if( Input.GetKeyUp( "right" ) ) {
            debug( "STOP RIGHT TURN" );

            if( _player != null ) _player.TurningRight = false;
            Connection.Instance.SendCommand( new StopTurnCommand( 1 ) );
        }
        if( Input.GetKeyDown( "left" ) ) {
            debug( "START LEFT TURN" );

            if( _player != null ) _player.TurningLeft = true;
            Connection.Instance.SendCommand( new TurnCommand( 2 ) );
        }
        if( Input.GetKeyUp( "left" ) ) {
            debug( "STOP LEFT TURN" );

            if( _player != null ) _player.TurningLeft = false;
            Connection.Instance.SendCommand( new StopTurnCommand( 2 ) );
        }
        if( Input.GetKeyDown( "up" ) ) {
            debug( "START ACCELERATE" );

            if( _player != null ) _player.Accelerating = true;
            Connection.Instance.SendCommand( new AccelerateCommand( 1 ) );
        }
        if( Input.GetKeyUp( "up" ) ) {
            debug( "STOP ACCELERATE" );

            if( _player != null ) _player.Accelerating = false;
            Connection.Instance.SendCommand( new StopAccelerateCommand( 1 ) );
        }   
        if( Input.GetKeyDown( "space" ) ) {
            debug( "FIRE LASER" );
            Connection.Instance.SendCommand( new FireCommand( 1 ) );
        }
        if( Input.GetKeyDown( KeyCode.Return ) ) {
            debug( "FIRE MISSILE" );
            Connection.Instance.SendCommand( new FireCommand( 2 ) );
        }
        if( Input.GetKeyDown( KeyCode.Backspace ) ) {
            debug( "DECELERATE" );
            Connection.Instance.SendCommand( new DecelerateCommand() );
        }
        if( Input.GetKeyUp( KeyCode.Backspace ) ) {
            debug( "STOP DECELERATE" );
            Connection.Instance.SendCommand( new StopDecelerateCommand() );
        }

        if( Input.GetKey( "right" ) || Input.GetKey( "left" ) ) {
            var da = 0f;
            var ratio = _diff / 16.666667f;

            if( Input.GetKey( "right" ) ) {
                da += 2f * ratio;
            }
            if( Input.GetKey( "left" ) ) {
                da -= 2f * ratio;
            }
            if( da != 0 ) {
                /*var rotation = _player.transform.rotation.eulerAngles;
                rotation.y += da;
                _player.transform.rotation = Quaternion.Euler( rotation );*/
            }
        }        

        /*if( Input.GetKey( "right" ) )
            dx += 1;
        if( Input.GetKey( "left" ) )
            dx -= 1;
        if( Input.GetKey( "up" ) )
            dy += 1;
        if( Input.GetKey( "down" ) )
            dy -= 1;*/

        /*if( dx != 0 ) {
            Connection.Instance.SendCommand( new TurnCommand( dx > 0 ? (byte)1 : (byte)2 ) );
            if( dx > 0 ) Connection.Angle += 2;
            else Connection.Angle -= 2;              
        }

        if( dy != 0 ) { 
            Connection.Instance.SendCommand( new AccelerateCommand( dy > 0 ? (byte)1 : (byte)2 ) );
        }*/

        //_ship.transform.Rotate( 0, Connection.Angle, 0, Space.World );

        //if( _ship ) _ship.transform.rotation = Quaternion.Euler( 0, Connection.Angle, 0 );

        //debug( "ANGLE: " + Connection.Angle );
        return;
        /*/if( dx != 0 ) {
            debug( "Move: " + ( dx > 0 ? "Right" : "Left" ) );
            Connection.Instance.SendCommand( new MoveCommand( dx > 0 ? (byte)1 : (byte)2 ) );
            if( dx > 0 && _ship ) _ship.transform.Rotate( new Vector3( 0, 1, 0 ) );
            if( dx < 0 && _ship ) _ship.transform.Rotate( new Vector3( 0, -1, 0 ) );
            debug( "What" );
        }
        if( dy != 0 ) {
            debug( "Move: " + ( dy > 0 ? "Up" : "Down" ) );
            Connection.Instance.SendCommand( new MoveCommand( dy > 0 ? (byte)3 : (byte)4 ) );

            if( _background ) {
                var pos = _background.transform.position;
                if( dy > 0 && _background ) _background.transform.position = new Vector3( pos.x - .01f, pos.y, pos.z );
                if( dy < 0 && _background ) _background.transform.position = new Vector3( pos.x + .01f, pos.y, pos.z );
            }
        }*/
    }

    private void LateUpdate() {
        if( _player != null ) {
            var position = _player.transform.position;
            position.y = 15;
            Camera.main.transform.position = position;

            position.x = ( position.x - 100 ) / 5;
            position.z = ( position.z - 100 ) / 5;
            BackgroundCamera.transform.position = position;
        }
    }

    void OnApplicationQuit() {
        debug( "OnApplicationQuit" );        
        Connection.Instance.Dispose();
    }

    protected void debug( string msg, bool force = false, bool silence = false ) {
        if( silence ) return;
        if( _debug || force )
            print( "Game: " + msg );
    }
}