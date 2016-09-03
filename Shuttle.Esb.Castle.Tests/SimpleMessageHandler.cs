namespace Shuttle.Esb.Castle.Tests
{
    public class FirstSimpleMessageHandler :
        IMessageHandler<SimpleCommand>,
        IMessageHandler<SimpleEvent>
    {
        public void ProcessMessage(IHandlerContext<SimpleCommand> context)
        {
        }

        public void ProcessMessage(IHandlerContext<SimpleEvent> context)
        {
        }
    }
}