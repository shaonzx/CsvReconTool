using System;
using System.Collections.Generic;
using System.Text;

namespace CsvReconTool.Core.Models
{
    public class CsvRecord
    {
        public Dictionary<string, string> Fields { get; set; } = new();
        public int LineNumber { get; set; }

        public string GetFieldValue(string fieldName)
        {
            return Fields.TryGetValue(fieldName, out var value) ? value : string.Empty;
        }

        public void SetFieldValue(string fieldName, string value)
        {
            Fields[fieldName] = value;
        }
    }
}
