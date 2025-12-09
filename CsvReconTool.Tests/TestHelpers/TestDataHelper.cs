using CsvReconTool.Core.Models;

namespace CsvReconciliationTool.Tests.TestHelpers
{
    public static class TestDataHelper
    {
        public static CsvRecord CreateRecord(Dictionary<string, string> fields, int lineNumber = 1)
        {
            var record = new CsvRecord { LineNumber = lineNumber };
            foreach (var kvp in fields)
            {
                record.SetFieldValue(kvp.Key, kvp.Value);
            }
            return record;
        }

        public static List<CsvRecord> CreateRecords(params Dictionary<string, string>[] recordData)
        {
            var records = new List<CsvRecord>();
            for (int i = 0; i < recordData.Length; i++)
            {
                records.Add(CreateRecord(recordData[i], i + 1));
            }
            return records;
        }

        public static async Task<string> CreateTempCsvFile(List<string[]> rows, bool hasHeader = true)
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.csv");

            using var writer = new StreamWriter(tempFile);
            foreach (var row in rows)
            {
                await writer.WriteLineAsync(string.Join(",", row));
            }

            return tempFile;
        }

        public static void DeleteTempFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}