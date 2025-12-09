using System;
using System.Collections.Generic;
using System.Text;

namespace CsvReconTool.Core.Models
{
    public class FilePair
    {
        public string FilePathA { get; set; } = string.Empty;
        public string FilePathB { get; set; } = string.Empty;
        public string FileNameA => Path.GetFileName(FilePathA);
        public string FileNameB => Path.GetFileName(FilePathB);
    }
}
