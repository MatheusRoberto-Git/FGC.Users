namespace FGC.Users.Domain.Common.Events
{
    public interface IDomainEvent
    {
        Guid Id { get; }
        DateTime OccurredAt { get; }
    }
}
