# Parameter Manager

A Revit Add-In for managing shared and project type parameters efficiently.

## Features

- Browse and select element categories
- Organize family types hierarchically
- View and edit common parameters across multiple types
- Support for String, Integer, and Double parameter types
- MVVM architecture with WPF UI
- Clean blue color scheme

## Project Structure

```
ParameterManager/
├── Common/                    # Shared base classes
├── Models/                    # Data models
├── Contracts/                 # Service interfaces
├── Services/                  # Business logic
├── ViewModels/                # MVVM ViewModels
├── Views/                     # WPF UI
├── Resources/                 # Styles and resources
└── ParameterManager.csproj    # Project file
```

## Installation

1. Build the project to generate `ParameterManager.dll`
2. Copy the compiled files to: `C:\ProgramData\Autodesk\Revit\Addins\2025\ParameterManager\`
3. Copy `ParameterManager.addin` to: `C:\ProgramData\Autodesk\Revit\Addins\2025\`
4. Restart Revit
5. Look for **Parameter Manager** button in the **Add-Ins** ribbon panel

## Usage

1. Click the **Parameter Manager** button in Revit's Add-Ins panel
2. Select a category from the dropdown
3. Check the family types you want to modify in the left tree
4. Edit parameter values in the right panel
5. Click **Assign Parameter** to apply changes

## Architecture

The project follows MVVM with these design patterns:
- **Repository Pattern**: `RevitFamilyTypeRepository`
- **Service Layer**: `RevitParameterService`
- **Strategy Pattern**: Parameter value setters (`StringParameterValueSetter`, etc.)
- **Factory Pattern**: `ParameterValueSetterFactory`

## Requirements

- Revit 2025
- .NET 8.0 Windows
- WPF
