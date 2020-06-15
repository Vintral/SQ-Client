public static class PACKET_MAP {
    public const int PACKET_ID = 0;
    public const int COMMAND = 1;
    public const int CLIENT_ID_1 = 2;
    public const int CLIENT_ID_2 = 3;
}

public enum PACKET_TYPE {
    EMPTY,
    PING,
    PONG,
    ACK,
    INITIALIZE,
    INITIALIZED,
    REQUEST_ADDRESS,
    ADDRESS,
    STATE,
    JOIN,
    JOINED,
    ID,
    DISCONNECT,
    SELECT_SHIP,
    TURN,
    STOP_TURN,
    ACCELERATE,
    STOP_ACCELERATE,
    DECELERATE,
    STOP_DECELERATE,
    FIRE,
    USE,
    READY
};