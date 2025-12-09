using CsvReconciliationTool.Tests.TestHelpers;
using CsvReconTool.Core.Services;
using FluentAssertions;

namespace CsvReconciliationTool.Tests.ServicesTests
{
    public class CsvReaderServiceTests
    {
        [Fact]
        public async Task ReadRecordsAsync_WithValidCsv_ShouldReturnRecords()
        {
            // Arrange
            var rows = new List<string[]>
            {
                new[] { "OrderId", "CustomerName", "Amount" },
                new[] { "1001", "John Doe", "100.50" },
                new[] { "1002", "Jane Smith", "200.75" }
            };
            var tempFile = await TestDataHelper.CreateTempCsvFile(rows);
            var reader = new CsvReaderService(',', true);

            try
            {
                // Act
                var records = await reader.ReadRecordsAsync(tempFile);

                // Assert
                records.Should().HaveCount(2);
                records[0].GetFieldValue("OrderId").Should().Be("1001");
                records[0].GetFieldValue("CustomerName").Should().Be("John Doe");
                records[1].GetFieldValue("OrderId").Should().Be("1002");
            }
            finally
            {
                TestDataHelper.DeleteTempFile(tempFile);
            }
        }

        [Fact]
        public async Task ReadHeadersAsync_WithValidCsv_ShouldReturnHeaders()
        {
            // Arrange
            var rows = new List<string[]>
            {
                new[] { "OrderId", "CustomerName", "Amount" },
                new[] { "1001", "John Doe", "100.50" }
            };
            var tempFile = await TestDataHelper.CreateTempCsvFile(rows);
            var reader = new CsvReaderService(',', true);

            try
            {
                // Act
                var headers = await reader.ReadHeadersAsync(tempFile);

                // Assert
                headers.Should().HaveCount(3);
                headers.Should().Contain("OrderId");
                headers.Should().Contain("CustomerName");
                headers.Should().Contain("Amount");
            }
            finally
            {
                TestDataHelper.DeleteTempFile(tempFile);
            }
        }

        [Fact]
        public async Task ReadRecordsAsync_EmptyFile_ShouldReturnEmptyList()
        {
            // Arrange
            var rows = new List<string[]>
            {
                new[] { "OrderId", "CustomerName", "Amount" }
            };
            var tempFile = await TestDataHelper.CreateTempCsvFile(rows);
            var reader = new CsvReaderService(',', true);

            try
            {
                // Act
                var records = await reader.ReadRecordsAsync(tempFile);

                // Assert
                records.Should().BeEmpty();
            }
            finally
            {
                TestDataHelper.DeleteTempFile(tempFile);
            }
        }
    }
}