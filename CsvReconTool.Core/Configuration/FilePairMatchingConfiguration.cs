using System;
using System.Collections.Generic;
using System.Text;

namespace CsvReconTool.Core.Configuration
{
    public class FilePairMatchingConfiguration
    {
        public MatchingConfiguration DefaultMatchingRules { get; set; } = new();
        public Dictionary<string, MatchingConfiguration> FileSpecificRules { get; set; } = new();
        public MatchingConfiguration GetMatchingConfigForFile(string fileName)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            if (FileSpecificRules.TryGetValue(fileNameWithoutExtension, out var config))
            {
                return config;
            }

            return DefaultMatchingRules;
        }
    }
}
