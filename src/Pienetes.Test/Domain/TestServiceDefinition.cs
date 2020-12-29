using System.Linq;
using Pienetes.App.Domain;
using Pienetes.Test.Builders;
using Xunit;

namespace Pienetes.Test.Domain
{
    public class TestServiceDefinition
    {
        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void returns_expected_image(string expected)
        {
            var sut = new ServiceDefinitionBuilder()
                .WithImage(expected)
                .Build();
            
            Assert.Equal(new ServiceImage(expected), sut.Image);
        }

        #region ports
        
        [Fact]
        public void returns_expected_ports_when_initialized()
        {
            var sut = new ServiceDefinitionBuilder().Build();
            Assert.Empty(sut.Ports);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void returns_expected_ports_when_adding_single(int port)
        {
            var sut = new ServiceDefinitionBuilder().Build();
            sut.AddPortMapping(new ServicePortMapping(port, port));

            Assert.Equal(
                expected: new[]
                {
                    new ServicePortMapping(port, port)
                },
                actual: sut.Ports
            );
        }

        [Fact]
        public void returns_expected_ports_when_adding_multiple()
        {
            var sut = new ServiceDefinitionBuilder().Build();
            sut.AddPortMapping(new ServicePortMapping(1, 1));
            sut.AddPortMapping(new ServicePortMapping(2, 2));

            Assert.Equal(
                expected: new[]
                {
                    new ServicePortMapping(1, 1),
                    new ServicePortMapping(2, 2)
                },
                actual: sut.Ports
            );
        }

        [Fact]
        public void throws_expected_exception_if_adding_port_mapping_that_already_exists()
        {
            var sut = new ServiceDefinitionBuilder().Build();
            sut.AddPortMapping(new ServicePortMapping(1, 1));
            
            Assert.Throws<PortMappingException>(() => sut.AddPortMapping(new ServicePortMapping(1, 1)));
        }

        [Fact]
        public void throws_expected_exception_if_adding_port_mapping_with_host_port_than_is_already_mapped()
        {
            var sut = new ServiceDefinitionBuilder().Build();
            sut.AddPortMapping(new ServicePortMapping(1, 1));
            
            Assert.Throws<PortMappingException>(() => sut.AddPortMapping(new ServicePortMapping(1, 2)));
        }

        #endregion
        
        #region secrets
        
        [Fact]
        public void returns_expected_secrets_when_initialized()
        {
            var sut = new ServiceDefinitionBuilder().Build();
            Assert.Empty(sut.Secrets);
        }

        [Theory]
        [InlineData("foo,bar")]
        [InlineData("baz,qux")]
        public void returns_expected_secrets_when_adding_single(string secret)
        {
            var secretArgs = secret.Split(",");
            var secretName = secretArgs[0];
            var secretMountPath = secretArgs[1];
            
            var sut = new ServiceDefinitionBuilder().Build();
            sut.AddSecret(new ServiceSecret(secretName, secretMountPath));

            Assert.Equal(
                expected: new[]
                {
                    new ServiceSecret(secretName, secretMountPath)
                },
                actual: sut.Secrets
            );
        }

        [Fact]
        public void returns_expected_secrets_when_adding_multiple()
        {
            var sut = new ServiceDefinitionBuilder().Build();
            sut.AddSecret(new ServiceSecret("foo", "bar"));
            sut.AddSecret(new ServiceSecret("baz", "qux"));
        
            Assert.Equal(
                expected: new[]
                {
                    new ServiceSecret("foo", "bar"),
                    new ServiceSecret("baz", "qux")
                },
                actual: sut.Secrets
            );
        }
        
        [Fact]
        public void throws_expected_exception_if_adding_secret_that_already_exists()
        {
            var secretStub = new ServiceSecret("foo", "bar");
            
            var sut = new ServiceDefinitionBuilder().Build();
            sut.AddSecret(secretStub);
            
            Assert.Throws<SecretException>(() => sut.AddSecret(secretStub));
        }
        
        [Fact]
        public void throws_expected_exception_if_adding_a_secret_with_same_name_as_one_that_already_exists()
        {
            var sut = new ServiceDefinitionBuilder().Build();
            sut.AddSecret(new ServiceSecret("foo", "bar"));
            
            Assert.Throws<SecretException>(() => sut.AddSecret(new ServiceSecret("foo", "a different bar")));
        }
        
        #endregion

        #region environment variables
        
        [Fact]
        public void returns_expected_environment_variables_when_initialized()
        {
            var sut = new ServiceDefinitionBuilder().Build();
            Assert.Empty(sut.EnvironmentVariables);
        }

        [Theory]
        [InlineData("foo", "bar")]
        [InlineData("baz", "qux")]
        public void returns_expected_environment_variables_when_adding_single(string name, string value)
        {
            var sut = new ServiceDefinitionBuilder().Build();
            sut.AddEnvironmentVariable(new ServiceEnvironmentVariable(name, value));

            Assert.Equal(
                expected: new[]
                {
                    new ServiceEnvironmentVariable(name, value)
                },
                actual: sut.EnvironmentVariables
            );
        }

        [Fact]
        public void returns_expected_environment_variables_when_adding_multiple()
        {
            var sut = new ServiceDefinitionBuilder().Build();
            sut.AddEnvironmentVariable(new ServiceEnvironmentVariable("foo", "bar"));
            sut.AddEnvironmentVariable(new ServiceEnvironmentVariable("baz", "qux"));
        
            Assert.Equal(
                expected: new[]
                {
                    new ServiceEnvironmentVariable("foo", "bar"),
                    new ServiceEnvironmentVariable("baz", "qux"),
                },
                actual: sut.EnvironmentVariables
            );
        }
        
        [Fact]
        public void throws_expected_exception_if_adding_environment_variable_that_already_exists()
        {
            var variableStub = new ServiceEnvironmentVariable("foo", "bar");
            
            var sut = new ServiceDefinitionBuilder().Build();
            sut.AddEnvironmentVariable(variableStub);
            
            Assert.Throws<EnvironmentVariableException>(() => sut.AddEnvironmentVariable(variableStub));
        }
        
        [Fact]
        public void throws_expected_exception_if_adding_an_environment_variable_with_same_name_as_one_that_already_exists()
        {
            var sut = new ServiceDefinitionBuilder().Build();
            sut.AddEnvironmentVariable(new ServiceEnvironmentVariable("foo", "bar"));
            
            Assert.Throws<EnvironmentVariableException>(() => sut.AddEnvironmentVariable(new ServiceEnvironmentVariable("foo", "a different bar")));
        }
        
        #endregion

        #region checksum

        [Fact]
        public void returns_expected_checksum_when_having_same_id_and_image()
        {
            var sut1 = new ServiceDefinitionBuilder()
                .WithId("foo-id")
                .WithImage("foo-image")
                .Build();
            
            var sut2 = new ServiceDefinitionBuilder()
                .WithId("foo-id")
                .WithImage("foo-image")
                .Build();
            
            Assert.Equal(sut1.Checksum, sut2.Checksum);
        }

        [Fact]
        public void returns_expected_checksum_when_having_same_id_but_different_image()
        {
            var sut1 = new ServiceDefinitionBuilder()
                .WithId("foo-id")
                .WithImage("foo-image")
                .Build();
            
            var sut2 = new ServiceDefinitionBuilder()
                .WithId("foo-id")
                .WithImage("bar")
                .Build();
            
            Assert.NotEqual(sut1.Checksum, sut2.Checksum);
        }

        [Fact]
        public void returns_expected_checksum_when_having_different_id_but_same_image()
        {
            var sut1 = new ServiceDefinitionBuilder()
                .WithId("foo-id")
                .WithImage("foo-image")
                .Build();
            
            var sut2 = new ServiceDefinitionBuilder()
                .WithId("bar")
                .WithImage("foo-image")
                .Build();
            
            Assert.NotEqual(sut1.Checksum, sut2.Checksum);
        }

        [Fact]
        public void returns_expected_checksum_when_having_same_ports()
        {
            var sut1 = new ServiceDefinitionBuilder()
                .WithId("foo-id")
                .WithImage("foo-image")
                .WithPorts(new ServicePortMapping(1, 2))
                .Build();
            
            var sut2 = new ServiceDefinitionBuilder()
                .WithId("foo-id")
                .WithImage("foo-image")
                .WithPorts(new ServicePortMapping(1, 2))
                .Build();
            
            Assert.Equal(sut1.Checksum, sut2.Checksum);
        }

        [Fact]
        public void returns_expected_checksum_when_having_different_ports()
        {
            var sut1 = new ServiceDefinitionBuilder()
                .WithId("foo-id")
                .WithImage("foo-image")
                .WithPorts(new ServicePortMapping(1, 1))
                .Build();
            
            var sut2 = new ServiceDefinitionBuilder()
                .WithId("foo-id")
                .WithImage("foo-image")
                .WithPorts(new ServicePortMapping(2, 2))
                .Build();
            
            Assert.NotEqual(sut1.Checksum, sut2.Checksum);
        }

        [Fact]
        public void returns_expected_checksum_when_having_same_secrets()
        {
            var sut1 = new ServiceDefinitionBuilder()
                .WithSecrets(new ServiceSecret("foo", "bar"))
                .Build();
            
            var sut2 = new ServiceDefinitionBuilder()
                .WithSecrets(new ServiceSecret("foo", "bar"))
                .Build();
            
            Assert.Equal(sut1.Checksum, sut2.Checksum);
        }

        [Fact]
        public void returns_expected_checksum_when_having_different_secrets()
        {
            var sut1 = new ServiceDefinitionBuilder()
                .WithSecrets(new ServiceSecret("foo", "bar"))
                .Build();
            
            var sut2 = new ServiceDefinitionBuilder()
                .WithSecrets(new ServiceSecret("baz", "qux"))
                .Build();
            
            Assert.NotEqual(sut1.Checksum, sut2.Checksum);
        }

        [Fact]
        public void returns_expected_checksum_when_having_same_environment_variables()
        {
            var sut1 = new ServiceDefinitionBuilder()
                .WithEnvironmentVariables(new ServiceEnvironmentVariable("foo", "bar"))
                .Build();
            
            var sut2 = new ServiceDefinitionBuilder()
                .WithEnvironmentVariables(new ServiceEnvironmentVariable("foo", "bar"))
                .Build();
            
            Assert.Equal(sut1.Checksum, sut2.Checksum);
        }

        [Fact]
        public void returns_expected_checksum_when_having_different_environment_variables()
        {
            var sut1 = new ServiceDefinitionBuilder()
                .WithEnvironmentVariables(new ServiceEnvironmentVariable("foo", "bar"))
                .Build();
            
            var sut2 = new ServiceDefinitionBuilder()
                .WithEnvironmentVariables(new ServiceEnvironmentVariable("baz", "qux"))
                .Build();
            
            Assert.NotEqual(sut1.Checksum, sut2.Checksum);
        }

        #endregion
    }
}