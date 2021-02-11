using System.Threading.Tasks;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Application
{
    public interface IContainerApplicationService
    {
        Task SpinUpNewContainer(ServiceId serviceId);
        Task UpdateExistingContainer(ServiceId serviceId);
    }
}