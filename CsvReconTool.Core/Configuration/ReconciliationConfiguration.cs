using System;
using System.Collections.Generic;
using System.Text;

namespace CsvReconTool.Core.Configuration
{
    public class ReconciliationConfiguration
    {
        public string FolderA { get; set; } = string.Empty;
        public string FolderB { get; set; } = string.Empty;
        public MatchingConfiguration MatchingRules { get; set; } = new();
        public FilePairMatchingConfiguration? FilePairRules { get; set; }
        public string OutputFolder { get; set; } = "Output";
        public int DegreeOfParallelism { get; set; } = Environment.ProcessorCount;
        public char CsvDelimiter { get; set; } = ',';
        public bool HasHeaderRow { get; set; } = true;
        public ComparisonMode ComparisonMode { get; set; } = ComparisonMode.ByFileName;
    }

    public enum ComparisonMode
    {
        ByFileName,
        AllToAll
    }
}
