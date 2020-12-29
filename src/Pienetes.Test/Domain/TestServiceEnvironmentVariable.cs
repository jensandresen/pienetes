using System;
using Pienetes.App.Domain;
using Pienetes.Test.Builders;
using Xunit;

namespace Pienetes.Test.Domain
{
    public class TestServiceEnvironmentVariable
    {
        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void returns_expected_name(string expected)
        {
            var sut = new ServiceEnvironmentVariableBuilder()
                .WithName(expected)
                .Build();
            
            Assert.Equal(expected, sut.Name);
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void returns_expected_value(string expected)
        {
            var sut = new ServiceEnvironmentVariableBuilder()
                .WithValue(expected)
                .Build();
            
            Assert.Equal(expected, sut.Value);
        }
        
        [Theory]
        [InlineData("foo=bar", "foo", "bar")]
        [InlineData(" foo=bar", "foo", "bar")]
        [InlineData("foo=bar ", "foo", "bar")]
        [InlineData(" foo=bar ", "foo", "bar")]
        [InlineData("        foo=bar        ", "foo", "bar")]
        public void returns_expected_when_parsing_simple_and_valid_text(string inputText, string expectedName, string expectedValue)
        {
            var result = ServiceEnvironmentVariable.Parse(inputText);
            
            Assert.Equal(
                expected: new ServiceEnvironmentVariable(expectedName, expectedValue),
                actual: result
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("      ")]
        [InlineData("=")]
        public void throws_expected_exception_when_parsing_invalid_text(string invalidText)
        {
            Assert.Throws<ArgumentException>(() => ServiceEnvironmentVariable.Parse(invalidText));
        }
    }
}