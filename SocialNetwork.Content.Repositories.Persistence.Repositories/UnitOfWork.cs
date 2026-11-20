public class UnitOfWork : IUnitOfWork {
    private readonly Lazy<IInboxMessageRepository> _inboxMessages;
    public IInboxMessageRepository InboxMessages => _inboxMessages.Value;
    _inboxMessages = new Lazy<IInboxMessageRepository>(() => new InboxMessageRepository(_context));
}