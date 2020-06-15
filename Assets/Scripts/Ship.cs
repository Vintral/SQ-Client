using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Ship : MonoBehaviour {
    protected bool _debug;
    
    protected UInt32 _updated;

    protected bool _accelerating;
    protected bool _turningLeft;
    protected bool _turningRight;

    protected float _velocityAdjust;
    protected float _targetVX;
    protected float _targetVY;
    protected Vector2 _coords;
    protected float _vx;
    protected float _vy;
    protected float _va;

    protected Vector2 _oldPosition;
    protected Vector3 _position;
    protected Vector3 _testPosition;

    protected Quaternion _rotation;

    protected float _diff;

    protected float _da;
    protected float _angle;
    protected float _angleZ;

    protected ParticleSystem _particles;
    protected ParticleSystem.MainModule _particlesMain;
    protected ParticleSystem.ShapeModule _particlesShape;
    protected ParticleSystemRenderer _particlesRenderer;

    protected DateTime _originTime;

    public bool Accelerating {
        get { return _accelerating; }
        set {
            _accelerating = value;

            if( _particles != null ) {
                if( _accelerating ) _particles.Play();
                if( !_accelerating ) _particles.Stop();
            }
        }
    }

    public bool TurningLeft {
        get { return _turningLeft; }
        set { _turningLeft = value; }
    }

    public bool TurningRight {
        get { return _turningRight; }
        set { _turningRight = value; }
    }

    public void Start() {
        _debug = false;
        _updated = 0;
        _velocityAdjust = 0.001f;

        _position = new Vector3();
        _oldPosition = new Vector2();
        _testPosition = new Vector3();

        _originTime = new DateTime( 1970, 1, 1 );

        _angleZ = 0;
    }

    public void Awake() {
        _particles = gameObject.GetComponent<ParticleSystem>();
        if( _particles == null ) _particles = gameObject.AddComponent<ParticleSystem>();

        _particlesMain = _particles.main;
        _particlesMain.simulationSpace = ParticleSystemSimulationSpace.World;
        _particlesMain.startLifetime = 0.1f;

        _particlesShape = _particles.shape;
        _particlesShape.position = new Vector3( 0, 0, -5 );
        _particlesShape.rotation = new Vector3( 0, 180, 0 );

        //Material ma = Resources.Load<Material>( "Assets/Default-Particle" );
        _particlesRenderer = gameObject.GetComponent<ParticleSystemRenderer>();

        /*Material m = new Material( _particlesRenderer.sharedMaterial );
        string uPath = AssetDatabase.GenerateUniqueAssetPath( "Assets/Default-Particle(Clone).mat" );
        AssetDatabase.CreateAsset( m, uPath );
        AssetDatabase.SaveAssets();

        _particlesRenderer.material = m;*/

        _particles.Stop();        
    }

    public void OnBecameVisible() {
        debug( "OnBecameVisible" );
    }

    public void OnBecameInvisible() {
        debug( "OnBecameInvisible" );
    }

    public void Update() {
        debug( "Update", false, true );        

        try {
            if( _updated > 0 ) {
                // Calculate how long it's been in MS
                _diff = Time.deltaTime * 1000;                                

                // Store our position
                _oldPosition.x = _position.x;
                _oldPosition.y = _position.z;

                // Move our ship
                _position.x += _vx * _diff;
                _position.z += _vy * _diff;

                // Don't overshoot coords
                /*if( _oldPosition.x > _coords.x && _position.x < _coords.x ) _position.x = _coords.x;
                if( _oldPosition.x < _coords.x && _position.x > _coords.x ) _position.x = _coords.x;
                if( _oldPosition.y > _coords.y && _position.z < _coords.y ) _position.z = _coords.y;
                if( _oldPosition.y < _coords.y && _position.z > _coords.y ) _position.z = _coords.y;*/

                // Set the new position
                gameObject.transform.position = _position;                

                // Grab our angle and calculate the offset for time difference                
                _da = _va * _diff;

                float turnChange = 0.05f;
                float resetChange = turnChange * 3;
                float maxAngle = 60f;

                //if( _da < 0 ) _angleZ -= 1 * _diff;
                //if( _da > 0 ) _angleZ += 1 * _diff;
                if( _turningLeft != _turningRight ) {
                    if( _turningLeft ) {
                        _angleZ += turnChange * _diff;
                        if( _angleZ > maxAngle ) _angleZ = maxAngle;
                    }
                    if( _turningRight ) {
                        _angleZ -= turnChange * _diff;
                        if( _angleZ < -maxAngle ) _angleZ = -maxAngle;
                    }
                } else {
                    if( _angleZ != 0 ) {
                        if( _angleZ < 0 ) {
                            _angleZ += resetChange * _diff;
                            if( _angleZ > 0 ) _angleZ = 0;
                        }

                        if( _angleZ > 0 ) {
                            _angleZ -= resetChange * _diff;
                            if( _angleZ < 0 ) _angleZ = 0;
                        }

                        print( "ANGLE Z: " + _angleZ );
                    }
                }

                // Only rotate if we're not at our angle
                if( Math.Abs( _angle - _rotation.eulerAngles.y ) > .001f ) {
                    // If our delta is will jump beyond our angle, just go to the target angle
                    if( ( _angle < _rotation.y ) && ( _angle + _da > _rotation.y ) ||
                        ( _angle > _rotation.y ) && ( _angle - _da < _rotation.y ) ) {                            
                            _rotation = Quaternion.Euler( 0, _angle, _angleZ );
                    } else {                        
                        _rotation = Quaternion.Euler( 0, _rotation.eulerAngles.y + _da, _angleZ );
                    }

                    gameObject.transform.rotation = _rotation;
                }
            }
        } catch( Exception ex ) {
            debug( "ERROR: " + ex.Message );
        }
    }

    public void setState( float x, float y, float a, Byte status ) {
        //debug( "setState: (" + x + "," + y + ") @ " + a + " degrees" );
        _angle = a;

        /*if( ( status & 0b00000001 ) != 0 ) {
            debug( "TURN SHIP", true );
            if( _angleZ < 90 ) _angleZ += 1;
            debug( "Z Angle: " + _angleZ, true );
        }*/

        _coords.x = x;
        _coords.y = y;

        _testPosition.x = x;
        _testPosition.z = y;
        //gameObject.transform.position = _testPosition;

        if( _updated == 0 ) {
            _position.x = x;
            _position.z = y;

            _rotation = Quaternion.Euler( 0, a, 0 );
            gameObject.transform.rotation = _rotation;
            gameObject.transform.position = _position;
        } else {
            var diff = (UInt32)( DateTime.UtcNow.Subtract( _originTime ).TotalMilliseconds ) - _updated;
            if( diff == 0 ) return;

            //debug( "DIFF: " + diff, true );

            // Make sure our angle is between 1-360
            while( a > 360 ) a -= 360;

            // Grab our current angle and make sure it's 1-360
            var angle = gameObject.transform.rotation.eulerAngles.y;
            if( angle < 0 ) angle += 360;

            //debug( "CURRENT ANGLE: " + angle );

            // Calculate our rotation speed if need be
            if( Math.Abs( a - angle ) > .001f ) {                
                var da = getAngleDifference( a, angle );
                _va = da / diff;
            } else _va = 0;

            //debug( "ANGLE: " + angle + " --- " + a + " === " + _va, true );

            //debug( "VA: " + _va, true );

            var position = gameObject.transform.position;            
            if( position.x != x || position.z != y ) {
                //debug( "Move to: (" + x + "," + y + ")" );

                //debug( "X1: " + x + "   X2: " + position.x + "   DIFF: " + diff );
                _vx = getVelocity( x, position.x, diff );
                _vy = getVelocity( y, position.z, diff );

                //debug( "TargetVX: " + _targetVX + "   VX: " + _vx );
            } else { _vx = 0; _vy = 0; }
        }

        _updated = (UInt32)( DateTime.UtcNow.Subtract( _originTime ) ).TotalMilliseconds;
    }    

    protected float getVelocity( float to, float from, float time ) {
        //var v = (float)Math.Sqrt( ( to * to ) + ( from * from ) ) / time;
        //var v = ( to - from ) / time;
        //if( from > to ) v *= -1;
        //return v;
        return ( to - from ) / time;
    }

    protected float getAngleDifference( float to, float from ) {        
        var diff = 0f;
       
        if( from < to ) {
            if( from + 360 - to < to - from )
                diff = -1 * ( ( 360 + from ) - to );
            else diff = to - from;
        } else {            
            if( to + 360 - from < from - to )
                diff = ( ( to + 360 ) - from );
            else diff = to - from;
        }

        //debug( "Going from: " + from + " to " + to + " = " + diff );
        return diff;
    }

    protected void debug( string msg, bool force = false, bool silence = false ) {
        if( silence ) return;
        if( _debug || force )
            print( "Ship: " + msg );
    }
}
