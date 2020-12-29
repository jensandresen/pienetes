using System;
using Pienetes.App.Domain;
using Pienetes.Test.Builders;
using Xunit;

namespace Pienetes.Test.Domain
{
    public class TestServiceImage
    {
        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void returns_expected_name_when_initialized(string expected)
        {
            var sut = new ServiceImageBuilder()
                .WithName(expected)
                .Build();
            
            Assert.Equal(expected, sut.Name);
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void returns_expected_tag_when_initialized(string expected)
        {
            var sut = new ServiceImageBuilder()
                .WithTag(expected)
                .Build();
            
            Assert.Equal(expected, sut.Tag);
        }

        [Theory]
        [InlineData("foo:bar", "foo", "bar")]
        [InlineData("  foo:bar", "foo", "bar")]
        [InlineData("foo:bar  ", "foo", "bar")]
        [InlineData("  foo:bar  ", "foo", "bar")]
        [InlineData("            foo:bar            ", "foo", "bar")]
        public void returns_expected_when_parsing_simple_and_valid_text(string inputText, string expectedName, string expectedTag)
        {
            var result = ServiceImage.Parse(inputText);
            
            Assert.Equal(
                expected: new ServiceImage(expectedName, expectedTag),
                actual: result
            );
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void returns_expected_when_parsing_valid_text_without_tag(string expectedName)
        {
            var result = ServiceImage.Parse(expectedName);
            
            Assert.Equal(
                expected: new ServiceImage(expectedName),
                actual: result
            );
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void returns_expected_default_tag_when_parsing_valid_text_without_tag(string inputText)
        {
            var result = ServiceImage.Parse(inputText);
            
            Assert.Equal(
                expected: "latest",
                actual: result.Tag
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
            Assert.Throws<ArgumentException>(() => ServiceImage.Parse(invalidText));
        }
    }
}