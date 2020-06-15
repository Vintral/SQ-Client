public class DisconnectCommand : Command {
    public DisconnectCommand() {
        _type = PACKET_TYPE.DISCONNECT;
    }    
}