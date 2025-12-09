using CsvReconTool.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CsvReconTool.Core.Services
{
    public class OutputWriterService
    {
        private readonly string _outputFolder;
        private readonly CsvWriterService _csvWriter;

        public OutputWriterService(string outputFolder, char delimiter = ',')
        {
            _outputFolder = outputFolder;
            _csvWriter = new CsvWriterService(delimiter);

            if (!Directory.Exists(_outputFolder))
            {
                Directory.CreateDirectory(_outputFolder);
            }
        }
        public async Task WriteSummaryAsync(string fileName, ReconciliationResult result)
        {
            var filePath = Path.Combine(_outputFolder, fileName);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(result, options);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task WriteGlobalSummaryAsync(GlobalReconciliationSummary summary)
        {
            var filePath = Path.Combine(_outputFolder, "global-summary.json");

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(summary, options);
            await File.WriteAllTextAsync(filePath, json);
        }

        public string CreateFilePairFolder(string fileNameA, string fileNameB)
        {
            var folderName = $"{Path.GetFileNameWithoutExtension(fileNameA)}_vs_{Path.GetFileNameWithoutExtension(fileNameB)}";
            var folderPath = Path.Combine(_outputFolder, folderName);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return folderPath;
        }

        public CsvWriterService GetCsvWriter() => _csvWriter;
    }
}
