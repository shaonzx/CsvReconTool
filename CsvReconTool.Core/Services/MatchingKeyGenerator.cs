using CsvReconTool.Core.Configuration;
using CsvReconTool.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CsvReconTool.Core.Services
{
    public class MatchingKeyGenerator
    {
        private readonly MatchingConfiguration _config;

        public MatchingKeyGenerator(MatchingConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            if (_config.MatchingFields == null || _config.MatchingFields.Count == 0)
            {
                throw new ArgumentException("MatchingFields cannot be empty", nameof(config));
            }
        }

        public string GenerateKey(CsvRecord record)
        {
            var keyParts = new List<string>();

            foreach (var field in _config.MatchingFields)
            {
                var value = record.GetFieldValue(field);

                if (_config.Trim)
                {
                    value = value.Trim();
                }

                if (!_config.CaseSensitive)
                {
                    value = value.ToLowerInvariant();
                }

                keyParts.Add(value);
            }

            return string.Join("||", keyParts);
        }


        public bool HasRequiredFields(CsvRecord record)
        {
            return _config.MatchingFields.All(field => record.Fields.ContainsKey(field));
        }


        public List<string> GetMissingFields(CsvRecord record)
        {
            return _config.MatchingFields.Where(field => !record.Fields.ContainsKey(field)).ToList();
        }
    }
}
