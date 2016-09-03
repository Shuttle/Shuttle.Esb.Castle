namespace Shuttle.Esb.Castle.Tests.Duplicate
{
    public class DuplicateMessageHandler1 : IMessageHandler<DuplicateCommand>
    {
        public void ProcessMessage(IHandlerContext<DuplicateCommand> context)
        {
        }
    }
}