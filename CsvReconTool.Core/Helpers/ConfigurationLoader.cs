using CsvReconTool.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CsvReconTool.Core.Helpers
{
    public static class ConfigurationLoader
    {
        public static MatchingConfiguration LoadMatchingConfiguration(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
            {
                throw new FileNotFoundException($"Configuration file not found: {jsonFilePath}");
            }

            var json = File.ReadAllText(jsonFilePath);
            var config = JsonSerializer.Deserialize<MatchingConfiguration>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (config == null)
            {
                throw new InvalidOperationException("Failed to deserialize matching configuration");
            }

            return config;
        }
        public static ReconciliationConfiguration LoadReconciliationConfiguration(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
            {
                throw new FileNotFoundException($"Configuration file not found: {jsonFilePath}");
            }

            var json = File.ReadAllText(jsonFilePath);
            var config = JsonSerializer.Deserialize<ReconciliationConfiguration>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (config == null)
            {
                throw new InvalidOperationException("Failed to deserialize reconciliation configuration");
            }

            return config;
        }
                
        public static void CreateSampleMatchingConfig(string outputPath, bool composite = false)
        {
            var config = composite
                ? new MatchingConfiguration
                {
                    MatchingFields = new List<string> { "FirstName", "LastName" },
                    CaseSensitive = false,
                    Trim = true
                }
                : new MatchingConfiguration
                {
                    MatchingFields = new List<string> { "InvoiceId" },
                    CaseSensitive = false,
                    Trim = true
                };

            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            File.WriteAllText(outputPath, json);
        }
        public static FilePairMatchingConfiguration LoadFilePairMatchingConfiguration(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
            {
                throw new FileNotFoundException($"Configuration file not found: {jsonFilePath}");
            }

            var json = File.ReadAllText(jsonFilePath);
            var config = JsonSerializer.Deserialize<FilePairMatchingConfiguration>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (config == null)
            {
                throw new InvalidOperationException("Failed to deserialize file-pair matching configuration");
            }

            return config;
        }
    }
}
