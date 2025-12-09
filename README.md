# CSV Reconciliation Tool

A multithreaded .NET console application for reconciling CSV data between two folders. The tool compares files and produces detailed reconciliation reports showing matched and unmatched records.

## Features

- ✅ Multithreaded parallel processing for high performance
- ✅ File-specific matching configurations
- ✅ Single-field and composite-field matching
- ✅ Case-sensitive/insensitive comparison options
- ✅ Whitespace trimming and normalization
- ✅ Detailed reconciliation reports (CSV and JSON)
- ✅ Thread-safe logging with timestamps
- ✅ Graceful error handling for malformed data
- ✅ Two comparison modes: by filename or all-to-all

## Requirements

- .NET 10.0 SDK
- Visual Studio 2022 or later (recommended) or any .NET-compatible IDE

## Project Structure
```
CsvReconciliationTool/
├── CsvReconciliationTool.Core/          # Core library
│   ├── Configuration/                    # Configuration models
│   ├── Models/                          # Data models
│   ├── Services/                        # Business logic services
│   └── Helpers/                         # Utility helpers
├── CsvReconciliationTool/               # Console application
│   └── TestData/                        # Sample CSV files
│       ├── FolderA/                     # Source folder A
│       └── FolderB/                     # Source folder B
└── CsvReconciliationTool.Tests/         # Unit tests
```

## Build Instructions

### Using Visual Studio

1. Open `CsvReconciliationTool.sln` in Visual Studio
2. Right-click on the solution in Solution Explorer
3. Select **Build Solution** (or press `Ctrl+Shift+B`)

### Using Command Line
```bash
dotnet build CsvReconciliationTool.sln
```

## Configuration

### File-Pair Matching Configuration (Recommended)

Create a JSON file with file-specific matching rules:

**file-pair-matching.json**
```json
{
  "defaultMatchingRules": {
    "matchingFields": ["Id"],
    "caseSensitive": false,
    "trim": true
  },
  "fileSpecificRules": {
    "Orders": {
      "matchingFields": ["OrderId"],
      "caseSensitive": false,
      "trim": true
    },
    "Invoices": {
      "matchingFields": ["InvoiceId"],
      "caseSensitive": false,
      "trim": true
    },
    "Customers": {
      "matchingFields": ["FirstName", "LastName"],
      "caseSensitive": false,
      "trim": true
    }
  }
}
```

### Single Matching Configuration

Create a JSON file with a single matching rule for all files:

**matching-single-field.json**
```json
{
  "matchingFields": ["InvoiceId"],
  "caseSensitive": false,
  "trim": true
}
```

**matching-composite-field.json**
```json
{
  "matchingFields": ["FirstName", "LastName"],
  "caseSensitive": false,
  "trim": true
}
```

### Configuration Options

| Field | Description | Default |
|-------|-------------|---------|
| `matchingFields` | Array of field names to match on | Required |
| `caseSensitive` | Whether matching is case-sensitive | `false` |
| `trim` | Whether to trim whitespace from values | `true` |

## Usage

### Basic Usage
```bash
CsvReconciliationTool.exe -a <FolderA> -b <FolderB> -f <config.json>
```

### Command-Line Arguments

#### Required Arguments

| Argument | Short | Description |
|----------|-------|-------------|
| `--foldera` | `-a` | Path to folder A containing CSV files |
| `--folderb` | `-b` | Path to folder B containing CSV files |
| `--file-pair-config` | `-f` | Path to file-pair matching configuration (recommended) |
| `--matching-config` | `-m` | Path to single matching configuration (alternative to `-f`) |

#### Optional Arguments

| Argument | Short | Description | Default |
|----------|-------|-------------|---------|
| `--output` | `-o` | Output folder path | `Output` |
| `--parallelism` | `-p` | Degree of parallelism (threads) | CPU count |
| `--delimiter` | `-d` | CSV delimiter character | `,` |
| `--no-header` | | CSV files don't have header rows | Has headers |
| `--mode` | | Comparison mode: `byfilename` or `alltoall` | `byfilename` |

### Sample Commands

#### Using file-pair configuration (recommended + included with the launchsettings.json)
```bash
CsvReconciliationTool.exe -a ./TestData/FolderA -b ./TestData/FolderB -f file-pair-matching.json -o ./Results
```


## Output Structure
```
Output/
├── Orders_vs_Orders/
│   ├── matched.csv                    # Records found in both files
│   ├── only-in-folderA.csv           # Records only in folder A
│   ├── only-in-folderB.csv           # Records only in folder B
│   └── reconcile-summary.json        # Summary statistics
├── Invoices_vs_Invoices/
│   └── ...
├── global-summary.json               # Aggregated summary
└── reconciliation.log                # Detailed log file
```

### Output Files

#### matched.csv
Contains records that exist in both folders. Columns are prefixed with `A_` and `B_` to distinguish sources.
```csv
A_OrderId,A_CustomerName,A_Product,B_OrderId,B_CustomerName,B_Product
1001,John Smith,Laptop,1001,John Smith,Laptop
```

#### only-in-folderA.csv
Contains records that exist only in folder A.
```csv
OrderId,CustomerName,Product
1004,Alice Williams,Monitor
```

#### only-in-folderB.csv
Contains records that exist only in folder B.
```csv
OrderId,CustomerName,Product
1006,David Lee,Webcam
```

#### reconcile-summary.json
Per-file-pair summary statistics.
```json
{
  "fileNameA": "Orders.csv",
  "fileNameB": "Orders.csv",
  "totalInA": 5,
  "totalInB": 5,
  "matched": 3,
  "onlyInA": 2,
  "onlyInB": 2,
  "processingTime": "00:00:00.1234567",
  "warnings": [],
  "errors": []
}
```

#### global-summary.json
Aggregated summary across all file pairs.
```json
{
  "totalFilePairs": 5,
  "totalRecordsInA": 20,
  "totalRecordsInB": 20,
  "totalMatched": 10,
  "totalOnlyInA": 5,
  "totalOnlyInB": 5,
  "totalProcessingTime": "00:00:01.5000000",
  "fileResults": [...],
  "missingFiles": []
}
```

## Example Input Data

### Sample CSV Files

**FolderA/Orders.csv**
```csv
OrderId,CustomerName,Product,Quantity,Price
1001,John Smith,Laptop,2,1200.00
1002,Jane Doe,Mouse,5,25.50
1003,Bob Johnson,Keyboard,3,75.00
1004,Alice Williams,Monitor,1,350.00
1005,Charlie Brown,Headphones,2,89.99
```

**FolderB/Orders.csv**
```csv
OrderId,CustomerName,Product,Quantity,Price
1001,John Smith,Laptop,2,1200.00
1002,Jane Doe,Mouse,5,25.50
1003,Bob Johnson,Keyboard,3,75.00
1006,David Lee,Webcam,1,120.00
1007,Emma Davis,USB Cable,10,15.00
```

### Expected Output

When matching on `OrderId`:
- **Matched**: 3 records (OrderId: 1001, 1002, 1003)
- **Only in A**: 2 records (OrderId: 1004, 1005)
- **Only in B**: 2 records (OrderId: 1006, 1007)

## Running Unit Tests

### Using Visual Studio

1. Open **Test Explorer** (`Test → Test Explorer` or `Ctrl+E, T`)
2. Click **Run All Tests** button
3. View results in Test Explorer window

### Using Command Line
```bash
dotnet test CsvReconciliationTool.Tests/CsvReconciliationTool.Tests.csproj
```

### Test Coverage

The test suite includes:
- ✅ Configuration model tests
- ✅ Matching key generation tests (single/composite fields, case sensitivity, trimming)
- ✅ CSV reading and parsing tests
- ✅ Field validation tests
- ✅ Error handling tests

## Multithreading Behavior

The tool processes multiple file pairs concurrently using configurable parallelism:

- Default parallelism equals the number of CPU cores
- Each file pair is processed by a separate thread
- Thread-safe logging ensures correct output
- Check the log file to verify parallel execution (different thread IDs shown)

Example log output showing parallel processing:
```
[2024-12-09 10:30:15.123] [INFO] [Thread 4] Processing: Orders.csv vs Orders.csv
[2024-12-09 10:30:15.125] [INFO] [Thread 6] Processing: Invoices.csv vs Invoices.csv
[2024-12-09 10:30:15.127] [INFO] [Thread 8] Processing: Customers.csv vs Customers.csv
```

## Error Handling

The tool gracefully handles various error scenarios:

- **Malformed CSV rows**: Logged and skipped
- **Missing matching fields**: Logged as warnings, records skipped
- **Duplicate keys**: Logged as warnings, first occurrence used
- **Missing files**: Reported in global summary
- **Header mismatches**: Missing columns filled with empty values

All errors and warnings are logged to `reconciliation.log` and included in summary JSON files.

## Performance Considerations

- Uses streaming for large files to avoid memory issues
- Parallel processing scales with CPU cores
- Dictionary-based lookups for O(1) matching performance
- Adjust `--parallelism` parameter based on available resources

## Troubleshooting

### Issue: "Folder not found" error
**Solution**: Verify folder paths are correct and folders exist

### Issue: "Matching configuration is required" error
**Solution**: Provide either `-f` or `-m` argument with valid JSON file

### Issue: "Record missing fields" warnings
**Solution**: Ensure matching field names match CSV column headers exactly (case-sensitive)

### Issue: No matches found
**Solution**: 
- Verify matching configuration uses correct field names
- Check if `caseSensitive` setting is appropriate
- Verify data format consistency between files

### Issue: Tests failing
**Solution**: 
- Rebuild the solution (`dotnet build`)
- Ensure all NuGet packages are restored
- Check that .NET 10.0 SDK is installed

## License

This project is provided as-is for interview/assessment purposes.

## Author

Created as part of a technical assessment for Validata Group.