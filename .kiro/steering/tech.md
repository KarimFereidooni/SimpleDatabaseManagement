# Technology Stack

## Framework & Platform
- .NET 8.0 Windows Forms application
- Target Framework: `net8.0-windows`
- Windows-only application (uses WinForms)

## Key Dependencies
- **Newtonsoft.Json** (13.0.3) - JSON serialization
- **System.Data.SqlClient** (4.8.6) - SQL Server connectivity (legacy)
- **Microsoft.Data.SqlClient** (5.2.2) - Modern SQL Server connectivity
- **System.Configuration.ConfigurationManager** (8.0.0) - Configuration management

## Project Configuration
- Nullable reference types: disabled
- Implicit usings: disabled (explicit using statements required)
- Assembly info generation: disabled (manual AssemblyInfo.cs)
- Binary formatter warnings: suppressed for resource files

## Build Commands

Build the solution:
```cmd
dotnet build "Simple Database Management.sln"
```

Build in Release mode:
```cmd
dotnet build "Simple Database Management.sln" -c Release
```

Run the application:
```cmd
dotnet run --project "DatabaseManagement\Simple Database Management.csproj"
```

Clean build artifacts:
```cmd
dotnet clean "Simple Database Management.sln"
```

## Development Environment
- Visual Studio 2022 (solution format version 17)
- Minimum Visual Studio version: 10.0.40219.1
