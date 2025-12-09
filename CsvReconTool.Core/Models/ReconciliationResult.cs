using System;
using System.Collections.Generic;
using System.Text;

namespace CsvReconTool.Core.Models
{
    public class ReconciliationResult
    {
        public string FileNameA { get; set; } = string.Empty;
        public string FileNameB { get; set; } = string.Empty;
        public int TotalInA { get; set; }
        public int TotalInB { get; set; }
        public int Matched { get; set; }
        public int OnlyInA { get; set; }
        public int OnlyInB { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public List<string> Warnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }


    public class GlobalReconciliationSummary
    {
        public int TotalFilePairs { get; set; }
        public int TotalRecordsInA { get; set; }
        public int TotalRecordsInB { get; set; }
        public int TotalMatched { get; set; }
        public int TotalOnlyInA { get; set; }
        public int TotalOnlyInB { get; set; }
        public TimeSpan TotalProcessingTime { get; set; }
        public List<ReconciliationResult> FileResults { get; set; } = new();
        public List<string> MissingFiles { get; set; } = new();
    }
}
