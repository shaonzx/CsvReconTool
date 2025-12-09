using CsvHelper.Configuration;
using CsvReconTool.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CsvReconTool.Core.Services
{
    public class CsvReaderService
    {
        internal readonly char _delimiter;
        private readonly bool _hasHeader;

        public CsvReaderService(char delimiter = ',', bool hasHeader = true)
        {
            _delimiter = delimiter;
            _hasHeader = hasHeader;
        }

        public async Task<List<CsvRecord>> ReadRecordsAsync(string filePath)
        {
            var records = new List<CsvRecord>();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = _delimiter.ToString(),
                HasHeaderRecord = _hasHeader,
                BadDataFound = null, // Ignore bad data
                MissingFieldFound = null // Ignore missing fields
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvHelper.CsvReader(reader, config);

            if (_hasHeader)
            {
                await csv.ReadAsync();
                csv.ReadHeader();
                var headers = csv.HeaderRecord;

                int lineNumber = 1;
                while (await csv.ReadAsync())
                {
                    lineNumber++;
                    var record = new CsvRecord { LineNumber = lineNumber };

                    foreach (var header in headers)
                    {
                        var value = csv.GetField(header) ?? string.Empty;
                        record.SetFieldValue(header, value);
                    }

                    records.Add(record);
                }
            }
            else
            {
                // Handle CSV without headers (use column indices)
                int lineNumber = 0;
                while (await csv.ReadAsync())
                {
                    lineNumber++;
                    var record = new CsvRecord { LineNumber = lineNumber };

                    for (int i = 0; i < csv.Parser.Count; i++)
                    {
                        var value = csv.GetField(i) ?? string.Empty;
                        record.SetFieldValue($"Column{i}", value);
                    }

                    records.Add(record);
                }
            }

            return records;
        }

        public async Task<List<string>> ReadHeadersAsync(string filePath)
        {
            if (!_hasHeader)
            {
                return new List<string>();
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = _delimiter.ToString(),
                HasHeaderRecord = true
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvHelper.CsvReader(reader, config);

            await csv.ReadAsync();
            csv.ReadHeader();

            return csv.HeaderRecord?.ToList() ?? new List<string>();
        }
    }
}
