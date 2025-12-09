using CsvReconTool.Core.Configuration;
using CsvReconTool.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CsvReconTool.Core.Services
{
    public class ReconciliationEngine
    {
        private readonly MatchingConfiguration _matchingConfig;
        private readonly CsvReaderService _csvReader;
        private readonly ReconciliationLogger _logger;

        public ReconciliationEngine(
            MatchingConfiguration matchingConfig,
            CsvReaderService csvReader,
            ReconciliationLogger logger)
        {
            _matchingConfig = matchingConfig ?? throw new ArgumentNullException(nameof(matchingConfig));
            _csvReader = csvReader ?? throw new ArgumentNullException(nameof(csvReader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ReconciliationResult> ReconcileFilesAsync(
            string filePathA,
            string filePathB,
            string outputFolder)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new ReconciliationResult
            {
                FileNameA = Path.GetFileName(filePathA),
                FileNameB = Path.GetFileName(filePathB)
            };

            try
            {
                _logger.LogInfo($"Starting reconciliation: {result.FileNameA} vs {result.FileNameB}");

                // Read both files
                var recordsA = await _csvReader.ReadRecordsAsync(filePathA);
                var recordsB = await _csvReader.ReadRecordsAsync(filePathB);

                result.TotalInA = recordsA.Count;
                result.TotalInB = recordsB.Count;

                _logger.LogInfo($"Loaded {recordsA.Count} records from {result.FileNameA}");
                _logger.LogInfo($"Loaded {recordsB.Count} records from {result.FileNameB}");

                // Read headers
                var headersA = await _csvReader.ReadHeadersAsync(filePathA);
                var headersB = await _csvReader.ReadHeadersAsync(filePathB);

                // Generate matching keys and build dictionaries
                var keyGenerator = new MatchingKeyGenerator(_matchingConfig);

                var recordsAByKey = BuildRecordDictionary(recordsA, keyGenerator, result, "A");
                var recordsBByKey = BuildRecordDictionary(recordsB, keyGenerator, result, "B");

                // Find matches and unmatched records
                var matched = new List<(CsvRecord recordA, CsvRecord recordB)>();
                var onlyInA = new List<CsvRecord>();
                var onlyInB = new List<CsvRecord>(recordsBByKey.Values);

                foreach (var kvp in recordsAByKey)
                {
                    var key = kvp.Key;
                    var recordA = kvp.Value;

                    if (recordsBByKey.TryGetValue(key, out var recordB))
                    {
                        matched.Add((recordA, recordB));
                        onlyInB.Remove(recordB);
                    }
                    else
                    {
                        onlyInA.Add(recordA);
                    }
                }

                result.Matched = matched.Count;
                result.OnlyInA = onlyInA.Count;
                result.OnlyInB = onlyInB.Count;

                _logger.LogInfo($"Reconciliation complete: Matched={result.Matched}, OnlyInA={result.OnlyInA}, OnlyInB={result.OnlyInB}");

                // Write output files
                await WriteOutputFilesAsync(outputFolder, matched, onlyInA, onlyInB, headersA, headersB, result);

                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;
                result.Errors.Add($"Reconciliation failed: {ex.Message}");
                _logger.LogError($"Error reconciling {result.FileNameA} vs {result.FileNameB}", ex);
                throw;
            }
        }

        private Dictionary<string, CsvRecord> BuildRecordDictionary(
            List<CsvRecord> records,
            MatchingKeyGenerator keyGenerator,
            ReconciliationResult result,
            string source)
        {
            var dictionary = new Dictionary<string, CsvRecord>();

            foreach (var record in records)
            {
                try
                {
                    if (!keyGenerator.HasRequiredFields(record))
                    {
                        var missingFields = keyGenerator.GetMissingFields(record);
                        var warning = $"Record at line {record.LineNumber} in {source} missing fields: {string.Join(", ", missingFields)}";
                        result.Warnings.Add(warning);
                        _logger.LogWarning(warning);
                        continue;
                    }

                    var key = keyGenerator.GenerateKey(record);

                    if (dictionary.ContainsKey(key))
                    {
                        var warning = $"Duplicate key '{key}' found at line {record.LineNumber} in {source}";
                        result.Warnings.Add(warning);
                        _logger.LogWarning(warning);
                    }
                    else
                    {
                        dictionary[key] = record;
                    }
                }
                catch (Exception ex)
                {
                    var error = $"Error processing record at line {record.LineNumber} in {source}: {ex.Message}";
                    result.Errors.Add(error);
                    _logger.LogError(error);
                }
            }

            return dictionary;
        }

        private async Task WriteOutputFilesAsync(
            string outputFolder,
            List<(CsvRecord recordA, CsvRecord recordB)> matched,
            List<CsvRecord> onlyInA,
            List<CsvRecord> onlyInB,
            List<string> headersA,
            List<string> headersB,
            ReconciliationResult result)
        {
            var csvWriter = new CsvWriterService(_csvReader._delimiter);

            // Combine all unique headers
            var allHeaders = headersA.Union(headersB).ToList();

            // Write matched records
            var matchedPath = Path.Combine(outputFolder, "matched.csv");
            await csvWriter.WriteMatchedRecordsAsync(matchedPath, matched, headersA, headersB);
            _logger.LogInfo($"Written {matched.Count} matched records to {matchedPath}");

            // Write only-in-A records
            var onlyInAPath = Path.Combine(outputFolder, "only-in-folderA.csv");
            await csvWriter.WriteRecordsAsync(onlyInAPath, onlyInA, allHeaders);
            _logger.LogInfo($"Written {onlyInA.Count} records to {onlyInAPath}");

            // Write only-in-B records
            var onlyInBPath = Path.Combine(outputFolder, "only-in-folderB.csv");
            await csvWriter.WriteRecordsAsync(onlyInBPath, onlyInB, allHeaders);
            _logger.LogInfo($"Written {onlyInB.Count} records to {onlyInBPath}");

            // Write summary JSON
            var summaryPath = Path.Combine(outputFolder, "reconcile-summary.json");
            var outputWriter = new OutputWriterService(outputFolder);
            await outputWriter.WriteSummaryAsync("reconcile-summary.json", result);
            _logger.LogInfo($"Written summary to {summaryPath}");
        }
    }
}
