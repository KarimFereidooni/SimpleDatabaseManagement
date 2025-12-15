# Project Structure

## Solution Organization
Single-project solution with all code in the `DatabaseManagement` folder.

## Folder Structure

```
DatabaseManagement/
├── Forms/              # Windows Forms UI components
│   ├── MainForm.cs     # Primary application window
│   ├── LoginForm.cs    # Database connection dialog
│   ├── QueryForm.cs    # SQL query execution window
│   └── ...             # Other form dialogs
├── Services/           # Business logic and data access
│   ├── DataAccess.cs   # Singleton SQL connection wrapper
│   ├── INIFile.cs      # INI file configuration
│   └── PC.cs           # System utilities
├── Helpers/            # Utility classes
│   ├── SqlHelper.cs    # SQL query execution with GO statement support
│   └── VersionComparer.cs
├── Models/             # Data models
│   ├── Connection.cs   # Connection configuration model
│   └── DatabaseProperties.cs
├── Properties/         # Assembly metadata and settings
├── Resources/          # Embedded images and assets
└── Program.cs          # Application entry point
```

## Architecture Patterns

### Singleton Pattern
- `MainForm.Instance` - Single main window instance
- `DataAccess.Instance` - Single database connection instance
- Forms use singleton pattern to maintain state across dialog invocations

### Data Access Layer
- `DataAccess` class wraps `SqlConnection`, `SqlCommand`, and `SqlDataAdapter`
- Provides parameterized query methods: `Execute()`, `ExecuteScalar()`, `GetData()`
- Automatic connection state management (opens/closes as needed)
- Default command timeout: 5 minutes

### Form Patterns
- Each form has a `.Designer.cs` file for UI layout (auto-generated)
- `.resx` files contain embedded resources and localization
- Forms typically implement singleton pattern with `Instance` property

## Naming Conventions

### Files
- Forms: `{Purpose}Form.cs` (e.g., `LoginForm.cs`, `NewDatabaseForm.cs`)
- Services: Descriptive names (e.g., `DataAccess.cs`, `SqlHelper.cs`)

### Code
- Private fields: `_PascalCase` with underscore prefix
- Public properties: `PascalCase`
- Local variables: `camelCase`
- Methods: `PascalCase`
- Parameters: `camelCase` or `PascalCase`

### Namespace
Root namespace: `DatabaseManagement`
Sub-namespaces: `DatabaseManagement.Forms`, `DatabaseManagement.Services`, etc.

## Configuration
- Application settings stored via `Properties.Settings.Default`
- Connection strings managed through `Program.ConnectionString` static property
- INI file support through custom `INISettingsProvider`
