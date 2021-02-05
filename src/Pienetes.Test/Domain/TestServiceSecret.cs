using System;
using Microsoft.IdentityModel.Tokens;
using Pienetes.App.Domain;
using Pienetes.App.Domain.Model;
using Pienetes.Test.Builders;
using Xunit;

namespace Pienetes.Test.Domain
{
    public class TestServiceSecret
    {
        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void returns_expected_name(string expected)
        {
            var sut = new ServiceSecretBuilder()
                .WithName(expected)
                .Build();
            
            Assert.Equal(expected, sut.Name);
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void returns_expected_mount_path(string expected)
        {
            var sut = new ServiceSecretBuilder()
                .WithMountPath(expected)
                .Build();
            
            Assert.Equal(expected, sut.MountPath);
        }
        
        [Theory]
        [InlineData("foo:bar", "foo", "bar")]
        [InlineData(" foo:bar", "foo", "bar")]
        [InlineData("foo:bar ", "foo", "bar")]
        [InlineData(" foo:bar ", "foo", "bar")]
        [InlineData("        foo:bar        ", "foo", "bar")]
        public void returns_expected_when_parsing_simple_and_valid_text(string inputText, string expectedName, string expectedMountPath)
        {
            var result = ServiceSecret.Parse(inputText);
            
            Assert.Equal(
                expected: new ServiceSecret(expectedName, expectedMountPath),
                actual: result
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("      ")]
        [InlineData(":")]
        public void throws_expected_exception_when_parsing_invalid_text(string invalidText)
        {
            Assert.Throws<ArgumentException>(() => ServiceSecret.Parse(invalidText));
        }
    }
}