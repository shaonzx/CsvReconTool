using CsvReconTool.Core.Configuration;
using FluentAssertions;

namespace CsvReconciliationTool.Tests.ConfigurationTests
{
    public class MatchingConfigurationTests
    {
        [Fact]
        public void MatchingConfiguration_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var config = new MatchingConfiguration();

            // Assert
            config.MatchingFields.Should().NotBeNull();
            config.MatchingFields.Should().BeEmpty();
            config.CaseSensitive.Should().BeFalse();
            config.Trim.Should().BeTrue();
        }

        [Fact]
        public void MatchingConfiguration_CanSetProperties()
        {
            // Arrange
            var config = new MatchingConfiguration
            {
                MatchingFields = new List<string> { "Id", "Name" },
                CaseSensitive = true,
                Trim = false
            };

            // Assert
            config.MatchingFields.Should().HaveCount(2);
            config.MatchingFields.Should().Contain("Id");
            config.MatchingFields.Should().Contain("Name");
            config.CaseSensitive.Should().BeTrue();
            config.Trim.Should().BeFalse();
        }
    }
}