using System.Collections.Generic;
using System.Linq;
using Pienetes.App.Domain.Policies;
using Xunit;

namespace Pienetes.Test.Domain.Policies
{
    public class TestManifestEmbeddedDocument
    {
        [Fact]
        public void returns_expected_version_when_parsing()
        {
            var input = @"version: 1";
            var manifest = ManifestEmbeddedDocument.Parse(input);
            
            Assert.Equal(1, manifest.Version);
        }

        [Fact]
        public void throws_exception_when_parsing_and_version_is_not_defined()
        {
            var input = @"foo: bar";
            Assert.Throws<ManifestEmbeddedDocument.ManifestException>(() => ManifestEmbeddedDocument.Parse(input));
        }

        [Fact]
        public void returns_expected_service_when_none_is_defined()
        {
            var input = @"version: 1";
            var manifest = ManifestEmbeddedDocument.Parse(input);
            
            Assert.Null(manifest.GetServiceDefinition());
        }

        [Fact]
        public void returns_expected_service_name()
        {
            var input = @"
version: 1
service:
    name: foo
    image: dummy
";
            var manifest = ManifestEmbeddedDocument.Parse(input);
            var service = manifest.GetServiceDefinition();
            
            Assert.Equal(
                expected: "foo",
                actual: service.Name 
            );
        }

        [Fact]
        public void throws_expected_exception_when_service_is_missing_required_name()
        {
            var manifest = ManifestEmbeddedDocument.Parse(@"
version: 1
service:
    dummy: dummy
");
            
            Assert.Throws<ManifestEmbeddedDocument.ManifestException>(() => manifest.GetServiceDefinition());
        }

        [Fact]
        public void returns_expected_service_image()
        {
            var input = @"
version: 1
service:
    name: dummy
    image: foo
";
            var manifest = ManifestEmbeddedDocument.Parse(input);
            var service = manifest.GetServiceDefinition();
            
            Assert.Equal(
                expected: "foo",
                actual: service.Image 
            );
        }
        
        [Fact]
        public void throws_expected_exception_when_service_is_missing_required_image()
        {
            var manifest = ManifestEmbeddedDocument.Parse(@"
version: 1
service:
    name: dummy
");
            
            Assert.Throws<ManifestEmbeddedDocument.ManifestException>(() => manifest.GetServiceDefinition());
        }

        [Fact]
        public void returns_expected_service_ports_when_none_has_been_defined()
        {
            var input = @"
version: 1
service:
    name: dummy
    image: dummy
";
            var manifest = ManifestEmbeddedDocument.Parse(input);
            var service = manifest.GetServiceDefinition();
            
            Assert.Empty(service.Ports);
        }

        [Fact]
        public void returns_expected_service_ports_when_single_has_been_defined()
        {
            var input = @"
version: 1
service:
    name: dummy
    image: dummy
    ports:
        - ""1:2""
";
            var manifest = ManifestEmbeddedDocument.Parse(input);
            var service = manifest.GetServiceDefinition();

            Assert.Equal(
                expected: new[] {"1:2"},
                actual: service.Ports
            );
        }

        [Fact]
        public void returns_expected_service_ports_when_multiple_has_been_defined()
        {
            var input = @"
version: 1
service:
    name: dummy
    image: dummy
    ports:
        - ""1:2""
        - ""3:4""
";
            var manifest = ManifestEmbeddedDocument.Parse(input);
            var service = manifest.GetServiceDefinition();

            Assert.Equal(
                expected: new[] {"1:2", "3:4"},
                actual: service.Ports
            );
        }
        
        [Fact]
        public void returns_expected_service_secrets_when_none_has_been_defined()
        {
            var input = @"
version: 1
service:
    name: dummy
    image: dummy
";
            var manifest = ManifestEmbeddedDocument.Parse(input);
            var service = manifest.GetServiceDefinition();
            
            Assert.Empty(service.Secrets);
        }

        [Fact]
        public void returns_expected_service_secrets_when_single_has_been_defined()
        {
            var input = @"
version: 1
service:
    name: dummy
    image: dummy
    secrets:
        - ""foo""
";
            var manifest = ManifestEmbeddedDocument.Parse(input);
            var service = manifest.GetServiceDefinition();

            Assert.Equal(
                expected: new[] {"foo"},
                actual: service.Secrets
            );
        }
        
        [Fact]
        public void returns_expected_service_secrets_when_multiple_has_been_defined()
        {
            var input = @"
version: 1
service:
    name: dummy
    image: dummy
    secrets:
        - ""foo""
        - ""bar""
";
            var manifest = ManifestEmbeddedDocument.Parse(input);
            var service = manifest.GetServiceDefinition();

            Assert.Equal(
                expected: new[] {"foo", "bar"},
                actual: service.Secrets
            );
        }
        
        [Fact]
        public void returns_expected_service_environment_variables_when_none_has_been_defined()
        {
            var input = @"
version: 1
service:
    name: dummy
    image: dummy
";
            var manifest = ManifestEmbeddedDocument.Parse(input);
            var service = manifest.GetServiceDefinition();
            
            Assert.Empty(service.EnvironmentVariables);
        }

        [Fact]
        public void returns_expected_service_environment_variables_when_single_has_been_defined()
        {
            var input = @"
version: 1
service:
    name: dummy
    image: dummy
    environmentVariables:
        foo: bar
";
            var manifest = ManifestEmbeddedDocument.Parse(input);
            var service = manifest.GetServiceDefinition();

            Assert.Equal(
                expected: new[] { new KeyValuePair<string, string>("foo", "bar")},
                actual: service.EnvironmentVariables.AsEnumerable()
            );
        }
        
        [Fact]
        public void returns_expected_service_environment_variables_when_multiple_has_been_defined()
        {
            var input = @"
version: 1
service:
    name: dummy
    image: dummy
    environmentVariables:
        foo: bar
        baz: qux
";
            var manifest = ManifestEmbeddedDocument.Parse(input);
            var service = manifest.GetServiceDefinition();

            Assert.Equal(
                expected: new[]
                {
                    new KeyValuePair<string, string>("foo", "bar"),
                    new KeyValuePair<string, string>("baz", "qux"),
                },
                actual: service.EnvironmentVariables.AsEnumerable()
            );
        }
    }
}