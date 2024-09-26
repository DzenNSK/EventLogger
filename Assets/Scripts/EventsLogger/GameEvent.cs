namespace EventLogger
{
    public class GameEvent
    {
        public string type { get; private set; }
        public string data { get; private set; }

        public GameEvent(string type, string data)
        {
            this.type = type;
            this.data = data;
        }
    }
}