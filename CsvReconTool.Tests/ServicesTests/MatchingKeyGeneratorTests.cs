using CsvReconciliationTool.Tests.TestHelpers;
using CsvReconTool.Core.Configuration;
using CsvReconTool.Core.Services;
using FluentAssertions;

namespace CsvReconciliationTool.Tests.ServicesTests
{
    public class MatchingKeyGeneratorTests
    {
        [Fact]
        public void Constructor_WithNullConfig_ShouldThrowException()
        {
            // Act
            Action act = () => new MatchingKeyGenerator(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_WithEmptyMatchingFields_ShouldThrowException()
        {
            // Arrange
            var config = new MatchingConfiguration { MatchingFields = new List<string>() };

            // Act
            Action act = () => new MatchingKeyGenerator(config);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GenerateKey_SingleField_ShouldReturnCorrectKey()
        {
            // Arrange
            var config = new MatchingConfiguration
            {
                MatchingFields = new List<string> { "OrderId" },
                CaseSensitive = false,
                Trim = true
            };
            var generator = new MatchingKeyGenerator(config);
            var record = TestDataHelper.CreateRecord(new Dictionary<string, string>
            {
                { "OrderId", "  12345  " },
                { "CustomerName", "John Doe" }
            });

            // Act
            var key = generator.GenerateKey(record);

            // Assert
            key.Should().Be("12345");
        }

        [Fact]
        public void GenerateKey_CompositeFields_ShouldReturnCombinedKey()
        {
            // Arrange
            var config = new MatchingConfiguration
            {
                MatchingFields = new List<string> { "FirstName", "LastName" },
                CaseSensitive = false,
                Trim = true
            };
            var generator = new MatchingKeyGenerator(config);
            var record = TestDataHelper.CreateRecord(new Dictionary<string, string>
            {
                { "FirstName", "John" },
                { "LastName", "Doe" }
            });

            // Act
            var key = generator.GenerateKey(record);

            // Assert
            key.Should().Be("john||doe");
        }

        [Fact]
        public void GenerateKey_CaseSensitiveTrue_ShouldPreserveCase()
        {
            // Arrange
            var config = new MatchingConfiguration
            {
                MatchingFields = new List<string> { "Name" },
                CaseSensitive = true,
                Trim = true
            };
            var generator = new MatchingKeyGenerator(config);
            var record = TestDataHelper.CreateRecord(new Dictionary<string, string>
            {
                { "Name", "John" }
            });

            // Act
            var key = generator.GenerateKey(record);

            // Assert
            key.Should().Be("John");
        }

        [Fact]
        public void GenerateKey_TrimFalse_ShouldKeepWhitespace()
        {
            // Arrange
            var config = new MatchingConfiguration
            {
                MatchingFields = new List<string> { "Name" },
                CaseSensitive = false,
                Trim = false
            };
            var generator = new MatchingKeyGenerator(config);
            var record = TestDataHelper.CreateRecord(new Dictionary<string, string>
            {
                { "Name", "  John  " }
            });

            // Act
            var key = generator.GenerateKey(record);

            // Assert
            key.Should().Be("  john  ");
        }

        [Fact]
        public void HasRequiredFields_AllFieldsPresent_ShouldReturnTrue()
        {
            // Arrange
            var config = new MatchingConfiguration
            {
                MatchingFields = new List<string> { "FirstName", "LastName" }
            };
            var generator = new MatchingKeyGenerator(config);
            var record = TestDataHelper.CreateRecord(new Dictionary<string, string>
            {
                { "FirstName", "John" },
                { "LastName", "Doe" },
                { "Email", "john@example.com" }
            });

            // Act
            var result = generator.HasRequiredFields(record);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void HasRequiredFields_MissingField_ShouldReturnFalse()
        {
            // Arrange
            var config = new MatchingConfiguration
            {
                MatchingFields = new List<string> { "FirstName", "LastName" }
            };
            var generator = new MatchingKeyGenerator(config);
            var record = TestDataHelper.CreateRecord(new Dictionary<string, string>
            {
                { "FirstName", "John" },
                { "Email", "john@example.com" }
            });

            // Act
            var result = generator.HasRequiredFields(record);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetMissingFields_ShouldReturnMissingFieldsList()
        {
            // Arrange
            var config = new MatchingConfiguration
            {
                MatchingFields = new List<string> { "FirstName", "LastName", "Email" }
            };
            var generator = new MatchingKeyGenerator(config);
            var record = TestDataHelper.CreateRecord(new Dictionary<string, string>
            {
                { "FirstName", "John" }
            });

            // Act
            var missingFields = generator.GetMissingFields(record);

            // Assert
            missingFields.Should().HaveCount(2);
            missingFields.Should().Contain("LastName");
            missingFields.Should().Contain("Email");
        }
    }
}