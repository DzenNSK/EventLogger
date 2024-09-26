using System;

namespace EventLogger
{
    [Serializable]
    public class EventMessage
    {
        public GameEvent[] events;

        public EventMessage(GameEvent[] events)
        {
            this.events = events;
        }
    }
}