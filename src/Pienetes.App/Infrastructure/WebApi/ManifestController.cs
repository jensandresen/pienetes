using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Pienetes.App.Infrastructure.WebApi
{
    [ApiController]
    [Route("api/manifests")]
    public class ManifestController : ControllerBase
    {
        public ManifestController()
        {
            
        }

        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            return Ok("lala");
        }
        
        [HttpPost("")]
        [Route("api/applymanifest")]
        public async Task<IActionResult> ApplyManifest()
        {
            return Ok("lala");
        }
    }

    public class ManifestDTO
    {
        public string Version { get; set; }
        public class ServiceDefinition
        {
            public string Name { get; set; }
            public string Image { get; set; }
            public string[] Ports { get; set; }
            public string[] Secrets { get; set; }
            public Dictionary<string, string> EnvironmentVariables { get; set; }
        }
    }
}
