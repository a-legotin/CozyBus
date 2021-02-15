namespace CozyBus.Core
{
    public interface IMessageBus
    {
        void Publish(IBusMessage message);

        void Subscribe<T, TH>()
            where T : IBusMessage
            where TH : IBusMessageHandler<T>;
    }

    public interface IBusMessageHandler
    {
    }
}