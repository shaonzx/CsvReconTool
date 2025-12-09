using CsvReconTool.Core.Configuration;
using CsvReconTool.Core.Helpers;
using CsvReconTool.Core.Services;

namespace CsvReconTool
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("=== CSV Reconciliation Tool ===");
            Console.WriteLine();

            try
            {
                // Parse command line arguments
                var config = ParseArguments(args);

                if (config == null)
                {
                    PrintUsage();
                    return 1;
                }

                // Create logger
                var logFilePath = Path.Combine(config.OutputFolder, "reconciliation.log");
                using var logger = new ReconciliationLogger(logFilePath);

                // Create and run orchestrator
                var orchestrator = new ReconciliationOrchestrator(config, logger);
                var summary = await orchestrator.RunReconciliationAsync();

                Console.WriteLine();
                Console.WriteLine("=== Summary ===");
                Console.WriteLine($"Total File Pairs Processed: {summary.TotalFilePairs}");
                Console.WriteLine($"Total Records in Folder A: {summary.TotalRecordsInA}");
                Console.WriteLine($"Total Records in Folder B: {summary.TotalRecordsInB}");
                Console.WriteLine($"Total Matched: {summary.TotalMatched}");
                Console.WriteLine($"Total Only in A: {summary.TotalOnlyInA}");
                Console.WriteLine($"Total Only in B: {summary.TotalOnlyInB}");
                Console.WriteLine($"Processing Time: {summary.TotalProcessingTime.TotalSeconds:F2} seconds");
                Console.WriteLine();
                Console.WriteLine($"Results written to: {config.OutputFolder}");
                Console.WriteLine($"Log file: {logFilePath}");

                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Stack trace:");
                Console.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        static ReconciliationConfiguration? ParseArguments(string[] args)
        {
            if (args.Length == 0)
            {
                return null;
            }

            // Check for sample config generation
            if (args.Contains("--create-sample-config"))
            {
                CreateSampleConfigs();
                Environment.Exit(0);
            }

            var config = new ReconciliationConfiguration();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--foldera":
                    case "-a":
                        if (i + 1 < args.Length)
                            config.FolderA = args[++i];
                        break;

                    case "--folderb":
                    case "-b":
                        if (i + 1 < args.Length)
                            config.FolderB = args[++i];
                        break;

                    case "--matching-config":
                    case "-m":
                        if (i + 1 < args.Length)
                        {
                            var matchingConfig = ConfigurationLoader.LoadMatchingConfiguration(args[++i]);
                            config.MatchingRules = matchingConfig;
                        }
                        break;

                    case "--file-pair-config":
                    case "-f":
                        if (i + 1 < args.Length)
                        {
                            var filePairConfig = ConfigurationLoader.LoadFilePairMatchingConfiguration(args[++i]);
                            config.FilePairRules = filePairConfig;
                        }
                        break;

                    case "--output":
                    case "-o":
                        if (i + 1 < args.Length)
                            config.OutputFolder = args[++i];
                        break;

                    case "--parallelism":
                    case "-p":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out int parallelism))
                            config.DegreeOfParallelism = parallelism;
                        break;

                    case "--delimiter":
                    case "-d":
                        if (i + 1 < args.Length && args[i + 1].Length > 0)
                            config.CsvDelimiter = args[++i][0];
                        break;

                    case "--no-header":
                        config.HasHeaderRow = false;
                        break;

                    case "--mode":
                        if (i + 1 < args.Length)
                        {
                            var mode = args[++i].ToLower();
                            config.ComparisonMode = mode == "alltoall"
                                ? ComparisonMode.AllToAll
                                : ComparisonMode.ByFileName;
                        }
                        break;
                }
            }

            // Validate required parameters
            if (string.IsNullOrEmpty(config.FolderA) || string.IsNullOrEmpty(config.FolderB))
            {
                Console.WriteLine("ERROR: Both FolderA and FolderB are required.");
                return null;
            }

            // Either matching-config or file-pair-config must be provided
            if (config.FilePairRules == null && config.MatchingRules.MatchingFields.Count == 0)
            {
                Console.WriteLine("ERROR: Either --matching-config or --file-pair-config is required.");
                return null;
            }

            return config;
        }


        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  CsvReconciliationTool --foldera <path> --folderb <path> --matching-config <json-file> [options]");
            Console.WriteLine();
            Console.WriteLine("Required Arguments:");
            Console.WriteLine("  --foldera, -a          Path to folder A containing CSV files");
            Console.WriteLine("  --folderb, -b          Path to folder B containing CSV files");
            Console.WriteLine("  --matching-config, -m  Path to JSON file with matching rules");
            Console.WriteLine();
            Console.WriteLine("Optional Arguments:");
            Console.WriteLine("  --file-pair-config, -f Path to JSON file with per-file matching rules (alternative to -m)");
            Console.WriteLine("  --output, -o           Output folder path (default: 'Output')");
            Console.WriteLine("  --parallelism, -p      Degree of parallelism (default: CPU count)");
            Console.WriteLine("  --delimiter, -d        CSV delimiter character (default: ',')");
            Console.WriteLine("  --no-header            CSV files don't have header rows");
            Console.WriteLine("  --mode                 Comparison mode: 'byfilename' or 'alltoall' (default: byfilename)");
            Console.WriteLine();
            Console.WriteLine("Other Commands:");
            Console.WriteLine("  --create-sample-config Create sample configuration files");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("  CsvReconciliationTool -a ./FolderA -b ./FolderB -m matching.json -o ./Results");
        }

        static void CreateSampleConfigs()
        {
            Console.WriteLine("Creating sample configuration files...");

            // Single field matching
            ConfigurationLoader.CreateSampleMatchingConfig("matching-single-field.json", false);
            Console.WriteLine("Created: matching-single-field.json");

            // Composite field matching
            ConfigurationLoader.CreateSampleMatchingConfig("matching-composite-field.json", true);
            Console.WriteLine("Created: matching-composite-field.json");

            Console.WriteLine();
            Console.WriteLine("Sample configurations created successfully!");
        }
    }
}