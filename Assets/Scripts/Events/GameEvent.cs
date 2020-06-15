using System;

public delegate void Event( object sender, GameArgs e );

public class GameArgs : EventArgs {
    #region Properties        
    private object _data;
    #endregion

    #region Gets/Sets        
    public Object Data {
        get { return _data; }
        set { _data = value; }
    }

    public new static GameArgs Empty {
        get { return new GameArgs(); }
    }
    #endregion

    #region Constructor
    public GameArgs() {

    }

    public GameArgs( Object data ) {
        _data = data;
    }
    #endregion

    #region IEquatable Implementations
    public virtual bool Equals( GameArgs other ) {
        if( other == null ) return false;

        if( _data != null && _data != other.Data ) return false;

        return true;
    }

    public override bool Equals( Object obj ) {
        if( obj == null ) return false;

        GameArgs other = obj as GameArgs;
        if( other == null ) return false;
        else return Equals( other );
    }

    public static bool operator ==( GameArgs e1, GameArgs e2 ) {
        if( (object)e1 == null || ( (object)e2 ) == null )
            return Object.Equals( e1, e2 );

        return e1.Equals( e2 );
    }

    public static bool operator !=( GameArgs e1, GameArgs e2 ) {
        if( e1 == null || e2 == null )
            return !Object.Equals( e1, e2 );

        return !( e1.Equals( e2 ) );
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }
    #endregion
}