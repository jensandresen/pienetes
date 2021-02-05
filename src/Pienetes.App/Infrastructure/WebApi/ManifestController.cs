using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pienetes.App.Application;

namespace Pienetes.App.Infrastructure.WebApi
{
    [ApiController]
    // [Route("api/manifests")]
    public class ManifestController : ControllerBase
    {
        private readonly IManifestApplicationService _manifestApplicationService;

        public ManifestController(IManifestApplicationService manifestApplicationService)
        {
            _manifestApplicationService = manifestApplicationService;
        }

        [HttpPost("")]
        [Route("api/applymanifest")]
        [Consumes("text/yaml", "text/yml", "application/yaml", "application/yml")]
        public async Task<IActionResult> ApplyManifest()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var manifestContent = await reader.ReadToEndAsync();
                await _manifestApplicationService.QueueManifest(manifestContent, "text/yaml");

                return Accepted();
            }
        }
    }
}
