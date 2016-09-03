namespace Shuttle.Esb.Castle.Tests.Duplicate
{
    public class DuplicateMessageHandler2 : IMessageHandler<DuplicateCommand>
    {
        public void ProcessMessage(IHandlerContext<DuplicateCommand> context)
        {
        }
    }
}