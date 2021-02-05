using System;
using Pienetes.App.Domain;
using Pienetes.App.Domain.Model;
using Xunit;

namespace Pienetes.Test.Domain
{
    public class TestServicePortMapping
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void returns_expected_host_port(int expected)
        {
            const int dummyPortValue = 1;
            var sut = new ServicePortMapping(expected, dummyPortValue);
            Assert.Equal(expected, sut.HostPort);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void returns_expected_container_port(int expected)
        {
            const int dummyPortValue = 1;
            var sut = new ServicePortMapping(dummyPortValue, expected);
            Assert.Equal(expected, sut.ContainerPort);
        }
        
        [Fact]
        public void returns_expected_when_comparing_two_equal_instances()
        {
            var left = new ServicePortMapping(1, 1);
            var right = new ServicePortMapping(1, 1);
            
            Assert.Equal(left, right);
        }

        [Fact]
        public void returns_expected_when_comparing_two_equal_instances_using_operator()
        {
            var left = new ServicePortMapping(1, 1);
            var right = new ServicePortMapping(1, 1);
            
            Assert.True(left == right);
        }

        [Fact]
        public void returns_expected_when_comparing_two_non_equal_instances()
        {
            var left = new ServicePortMapping(1, 1);
            var right = new ServicePortMapping(2, 2);
            
            Assert.NotEqual(left, right);
        }

        [Fact]
        public void returns_expected_when_comparing_two_non_equal_instances_using_operator()
        {
            var left = new ServicePortMapping(1, 1);
            var right = new ServicePortMapping(2, 2);
            
            Assert.True(left != right);
        }
        
        [Theory]
        [InlineData("1:2", 1, 2)]
        [InlineData(" 1:2", 1, 2)]
        [InlineData("1:2 ", 1, 2)]
        [InlineData(" 1:2 ", 1, 2)]
        [InlineData("         1:2         ", 1, 2)]
        public void returns_expected_when_parsing_simple_and_valid_text(string inputText, int host, int container)
        {
            var result = ServicePortMapping.Parse(inputText);
            
            Assert.Equal(
                expected: new ServicePortMapping(host, container),
                actual: result
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("      ")]
        [InlineData("-")]
        [InlineData("a:b")]
        [InlineData(":")]
        [InlineData("1:a")]
        [InlineData("a:1")]
        [InlineData("1")]
        public void throws_expected_exception_when_parsing_invalid_text(string invalidText)
        {
            Assert.Throws<ArgumentException>(() => ServicePortMapping.Parse(invalidText));
        }
    }
}