namespace CozyBus.Core
{
    public interface IEventBusSubscriptionsManager
    {
        void AddSubscription<T, TH>()
            where T : IBusMessage
            where TH : IBusMessageHandler<T>;

        void RemoveSubscription<T, TH>()
            where TH : IBusMessageHandler<T>
            where T : IBusMessage;
    }
}