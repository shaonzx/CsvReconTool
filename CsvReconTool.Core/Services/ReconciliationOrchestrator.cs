using CsvReconTool.Core.Configuration;
using CsvReconTool.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CsvReconTool.Core.Services
{
    public class ReconciliationOrchestrator
    {
        private readonly ReconciliationConfiguration _config;
        private readonly ReconciliationLogger _logger;
        private readonly OutputWriterService _outputWriter;

        public ReconciliationOrchestrator(ReconciliationConfiguration config, ReconciliationLogger logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _outputWriter = new OutputWriterService(_config.OutputFolder, _config.CsvDelimiter);
        }

        public async Task<GlobalReconciliationSummary> RunReconciliationAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInfo("=== Starting CSV Reconciliation ===");
            _logger.LogInfo($"Folder A: {_config.FolderA}");
            _logger.LogInfo($"Folder B: {_config.FolderB}");
            _logger.LogInfo($"Output Folder: {_config.OutputFolder}");
            _logger.LogInfo($"Comparison Mode: {_config.ComparisonMode}");
            _logger.LogInfo($"Degree of Parallelism: {_config.DegreeOfParallelism}");

            // Validate folders exist
            ValidateFolders();

            // Get file pairs
            var filePairs = GetFilePairs();
            _logger.LogInfo($"Found {filePairs.Count} file pair(s) to reconcile");

            if (filePairs.Count == 0)
            {
                _logger.LogWarning("No file pairs found to reconcile");
                return new GlobalReconciliationSummary();
            }

            // Process file pairs in parallel
            var results = await ProcessFilePairsAsync(filePairs);

            stopwatch.Stop();

            // Create global summary
            var summary = CreateGlobalSummary(results, stopwatch.Elapsed);

            // Write global summary
            await _outputWriter.WriteGlobalSummaryAsync(summary);

            _logger.LogInfo("=== Reconciliation Complete ===");
            _logger.LogInfo($"Total Processing Time: {summary.TotalProcessingTime}");
            _logger.LogInfo($"Total File Pairs: {summary.TotalFilePairs}");
            _logger.LogInfo($"Total Matched: {summary.TotalMatched}");
            _logger.LogInfo($"Total Only in A: {summary.TotalOnlyInA}");
            _logger.LogInfo($"Total Only in B: {summary.TotalOnlyInB}");

            return summary;
        }


        private void ValidateFolders()
        {
            if (!Directory.Exists(_config.FolderA))
            {
                throw new DirectoryNotFoundException($"Folder A not found: {_config.FolderA}");
            }

            if (!Directory.Exists(_config.FolderB))
            {
                throw new DirectoryNotFoundException($"Folder B not found: {_config.FolderB}");
            }
        }


        private List<FilePair> GetFilePairs()
        {
            var filesA = Directory.GetFiles(_config.FolderA, "*.csv");
            var filesB = Directory.GetFiles(_config.FolderB, "*.csv");

            _logger.LogInfo($"Found {filesA.Length} CSV files in Folder A");
            _logger.LogInfo($"Found {filesB.Length} CSV files in Folder B");

            if (_config.ComparisonMode == ComparisonMode.ByFileName)
            {
                return GetFilePairsByFileName(filesA, filesB);
            }
            else
            {
                return GetAllToAllFilePairs(filesA, filesB);
            }
        }


        private List<FilePair> GetFilePairsByFileName(string[] filesA, string[] filesB)
        {
            var filePairs = new List<FilePair>();
            var filesBDict = filesB.ToDictionary(f => Path.GetFileName(f), f => f);

            foreach (var fileA in filesA)
            {
                var fileName = Path.GetFileName(fileA);

                if (filesBDict.TryGetValue(fileName, out var fileB))
                {
                    filePairs.Add(new FilePair
                    {
                        FilePathA = fileA,
                        FilePathB = fileB
                    });
                }
                else
                {
                    _logger.LogWarning($"No matching file in Folder B for: {fileName}");
                }
            }

            // Check for files in B that don't exist in A
            var filesADict = filesA.ToDictionary(f => Path.GetFileName(f), f => f);
            foreach (var fileB in filesB)
            {
                var fileName = Path.GetFileName(fileB);
                if (!filesADict.ContainsKey(fileName))
                {
                    _logger.LogWarning($"No matching file in Folder A for: {fileName}");
                }
            }

            return filePairs;
        }


        private List<FilePair> GetAllToAllFilePairs(string[] filesA, string[] filesB)
        {
            var filePairs = new List<FilePair>();

            foreach (var fileA in filesA)
            {
                foreach (var fileB in filesB)
                {
                    filePairs.Add(new FilePair
                    {
                        FilePathA = fileA,
                        FilePathB = fileB
                    });
                }
            }

            return filePairs;
        }

        private async Task<List<ReconciliationResult>> ProcessFilePairsAsync(List<FilePair> filePairs)
        {
            var results = new List<ReconciliationResult>();
            var resultsLock = new object();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = _config.DegreeOfParallelism
            };

            await Parallel.ForEachAsync(filePairs, parallelOptions, async (filePair, cancellationToken) =>
            {
                try
                {
                    _logger.LogInfo($"[Thread {Environment.CurrentManagedThreadId}] Processing: {filePair.FileNameA} vs {filePair.FileNameB}");

                    // Create output subfolder for this file pair
                    var pairOutputFolder = _outputWriter.CreateFilePairFolder(filePair.FileNameA, filePair.FileNameB);

                    // Get matching configuration for this file pair
                    var matchingConfig = _config.FilePairRules != null
                        ? _config.FilePairRules.GetMatchingConfigForFile(filePair.FileNameA)
                        : _config.MatchingRules;

                    // Create reconciliation engine for this file pair
                    var csvReader = new CsvReaderService(_config.CsvDelimiter, _config.HasHeaderRow);
                    var engine = new ReconciliationEngine(matchingConfig, csvReader, _logger);

                    // Reconcile the file pair
                    var result = await engine.ReconcileFilesAsync(
                        filePair.FilePathA,
                        filePair.FilePathB,
                        pairOutputFolder);

                    lock (resultsLock)
                    {
                        results.Add(result);
                    }

                    _logger.LogInfo($"[Thread {Environment.CurrentManagedThreadId}] Completed: {filePair.FileNameA} vs {filePair.FileNameB}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to process file pair: {filePair.FileNameA} vs {filePair.FileNameB}", ex);

                    var errorResult = new ReconciliationResult
                    {
                        FileNameA = filePair.FileNameA,
                        FileNameB = filePair.FileNameB
                    };
                    errorResult.Errors.Add($"Processing failed: {ex.Message}");

                    lock (resultsLock)
                    {
                        results.Add(errorResult);
                    }
                }
            });

            return results;
        }
        private GlobalReconciliationSummary CreateGlobalSummary(
            List<ReconciliationResult> results,
            TimeSpan totalTime)
        {
            var summary = new GlobalReconciliationSummary
            {
                TotalFilePairs = results.Count,
                TotalProcessingTime = totalTime,
                FileResults = results
            };

            foreach (var result in results)
            {
                summary.TotalRecordsInA += result.TotalInA;
                summary.TotalRecordsInB += result.TotalInB;
                summary.TotalMatched += result.Matched;
                summary.TotalOnlyInA += result.OnlyInA;
                summary.TotalOnlyInB += result.OnlyInB;
            }

            return summary;
        }
    

    }
}
