using System;
using System.Collections.Generic;
using System.Text;

namespace CsvReconTool.Core.Configuration
{
    public class MatchingConfiguration
    {
        public List<string> MatchingFields { get; set; } = new();
        public bool CaseSensitive { get; set; } = false;
        public bool Trim { get; set; } = true;
    }
}
