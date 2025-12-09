using CsvHelper;
using CsvHelper.Configuration;
using CsvReconTool.Core.Models;
using System.Globalization;

namespace CsvReconTool.Core.Services
{
    public class CsvWriterService
    {
        private readonly char _delimiter;

        public CsvWriterService(char delimiter = ',')
        {
            _delimiter = delimiter;
        }

        public async Task WriteRecordsAsync(string filePath, List<CsvRecord> records, List<string> headers)
        {
            if (records.Count == 0)
            {
                await File.WriteAllTextAsync(filePath, string.Empty);
                return;
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = _delimiter.ToString()
            };

            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, config);

            // Write headers
            foreach (var header in headers)
            {
                csv.WriteField(header);
            }
            await csv.NextRecordAsync();

            // Write records
            foreach (var record in records)
            {
                foreach (var header in headers)
                {
                    var value = record.GetFieldValue(header);
                    csv.WriteField(value);
                }
                await csv.NextRecordAsync();
            }
        }

        public async Task WriteMatchedRecordsAsync(string filePath,
            List<(CsvRecord recordA, CsvRecord recordB)> matchedPairs,
            List<string> headersA,
            List<string> headersB)
        {
            if (matchedPairs.Count == 0)
            {
                await File.WriteAllTextAsync(filePath, string.Empty);
                return;
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = _delimiter.ToString()
            };

            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, config);

            // Combine headers with prefix to avoid conflicts
            var allHeaders = headersA.Select(h => $"A_{h}")
                .Concat(headersB.Select(h => $"B_{h}"))
                .ToList();

            // Write headers
            foreach (var header in allHeaders)
            {
                csv.WriteField(header);
            }
            await csv.NextRecordAsync();

            // Write matched records
            foreach (var (recordA, recordB) in matchedPairs)
            {
                // Write fields from A
                foreach (var header in headersA)
                {
                    csv.WriteField(recordA.GetFieldValue(header));
                }

                // Write fields from B
                foreach (var header in headersB)
                {
                    csv.WriteField(recordB.GetFieldValue(header));
                }

                await csv.NextRecordAsync();
            }
        }
    }
}