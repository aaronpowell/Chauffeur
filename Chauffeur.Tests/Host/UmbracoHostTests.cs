using Chauffeur.Host;
using Xunit;

namespace Chauffeur.Tests.Host
{
    public class UmbracoHostTests
    {
        [Theory]
        [InlineData("install", new[] { "install" })]
        [InlineData("install y", new[] { "install", "y" })]
        [InlineData("install \"y\"", new[] { "install", "y" })]
        [InlineData("user create-user \"Aaron Powell\" password email", new[] { "user", "create-user", "Aaron Powell", "password", "email" })]
        public void CanParseCommands(string input, string[] expected)
        {
            var actual = UmbracoHost.ParseCommandline(input);

            Assert.Equal(expected, actual);
        }
    }
}
