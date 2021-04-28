using Microsoft.Extensions.Logging;

namespace SecretariaEletronica.Events
{
    public class EventIdent
    {
        public static readonly EventId BotEventId = new EventId(42, "BotMessage");
        public static readonly string DatabaseCollection = "SecretariaEletronica";
    }
}