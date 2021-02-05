using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pienetes.App.Domain.Model;
using Pienetes.App.Infrastructure.Persistence;

namespace Pienetes.App.Infrastructure.WebApi
{
    [ApiController]
    [Route("api/servicedefinitions")]
    public class ServiceDefinitionController : ControllerBase
    {
        private readonly IServiceDefinitionRepository _repository;
        private readonly PienetesDbContext _dbContext;

        public ServiceDefinitionController(IServiceDefinitionRepository repository, PienetesDbContext dbContext)
        {
            _repository = repository;
            _dbContext = dbContext;
        }
        
        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var items = await _repository.GetAll();
            var dto = new 
            {
                Items = items.Select(x => new
                {
                    Id = x.Id.ToString(),
                    Image = x.Image.ToString(),
                    Checksum = x.Checksum,
                    Ports = x.Ports.Select(port => port.ToString()),
                    Secrets = x.Secrets.Select(secret => secret.ToString()),
                    EnvironmentVariables = x.EnvironmentVariables.Select(env => env.ToString())
                })
            };

            return Ok(dto);
        }

        [HttpPost("")]
        public async Task<IActionResult> Add([FromBody] InputModel input)
        {
            var serviceDefinition = ServiceDefinition.Create(input.Id, input.Image);
            serviceDefinition.AddSecret(new ServiceSecret("shh1", "duno1"));
            serviceDefinition.AddSecret(new ServiceSecret("shh2", "duno2"));
            
            serviceDefinition.AddEnvironmentVariable(new ServiceEnvironmentVariable("var1", "a"));
            serviceDefinition.AddEnvironmentVariable(new ServiceEnvironmentVariable("var2", "b"));
            
            serviceDefinition.AddPortMapping(new ServicePortMapping(1,2));
            serviceDefinition.AddPortMapping(new ServicePortMapping(3,4));
            
            await _dbContext.ServiceDefinitions.AddAsync(serviceDefinition);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        public class InputModel
        {
            [Required]
            public string Id { get; set; }
            
            [Required]
            public string Image { get; set; }
        }
    }
}