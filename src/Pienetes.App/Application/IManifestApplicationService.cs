using System.Threading.Tasks;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Application
{
    public interface IManifestApplicationService
    {
        Task<QueuedManifestId> QueueManifest(string manifestContent, string contentType);
    }
}