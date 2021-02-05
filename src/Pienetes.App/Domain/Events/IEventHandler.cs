using System.Threading.Tasks;

namespace Pienetes.App.Domain.Events
{
    public interface IEventHandler<TEvent> where TEvent : IDomainEvent
    {
        Task Handle(TEvent e);
    }
}