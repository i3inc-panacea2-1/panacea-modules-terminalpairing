namespace Panacea.Modules.TerminalPairing
{
    public class CommunicationMessage
    {
        public string Action { get; set; }
    }
    public class CommunicationMessage<T>
    {
        public string Action { get; set; }
        public T Object { get; set; }
    }
}