using System.Linq;
using Microsoft.EntityFrameworkCore;
using Pienetes.App.Domain;
using Pienetes.App.Domain.Model;
using Pienetes.App.Infrastructure.Persistence;
using Xunit;

namespace Pienetes.Test.Domain
{
    public class DatabaseTest
    {
        [Fact]
        public void duno()
        {
            var options = new DbContextOptionsBuilder<PienetesDbContext>()
                .UseInMemoryDatabase(databaseName: "Test")
                .Options;

            using (var context = new PienetesDbContext(options))
            {
                var serviceDefinition = ServiceDefinition.Create(ServiceId.Create("foo"), new ServiceImage("bar"));
                serviceDefinition.AddSecret(new ServiceSecret("foo", "bar"));
                    
                context.ServiceDefinitions.Add(serviceDefinition);
                context.SaveChanges();

                var found = context.ServiceDefinitions.FirstOrDefault(x => x.Id.ToString() == "foo");
                Assert.NotNull(found);
                
                Assert.Equal(serviceDefinition, found);
                Assert.Equal(serviceDefinition.Checksum, found.Checksum);
            }
        }
    }
}