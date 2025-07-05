# Unity Package Template

A professional template for creating Unity packages with standardized structure and workflows.

## Features

- 📁 Standard Unity Package Layout
- 🔄 Automated GitHub Release Workflow
- 📦 Complete Package Configuration
- 🛠 Pre-configured Assembly Definitions
- 📝 Comprehensive Documentation Structure

## Directory Structure

```
com.template.package/
├── Runtime/             # Runtime code
├── Editor/              # Editor-specific code
├── Documentation~/      # Package documentation
├── .github/            # GitHub configurations
│   └── workflows/      # GitHub Actions
├── package.json        # Package manifest
├── README.md           # Package documentation
├── CHANGELOG.md        # Version history
└── LICENSE            # MIT license
```

## Quick Start

### Creating Your Package

1. Click "Use this template" to create a new repository
2. Clone your new repository
3. Open the project in Unity

### Package Configuration

1. Update `package.json`:
   ```json
   {
     "name": "com.company.package-name",    // Unique package identifier
     "displayName": "Package Display Name", // Name in Package Manager
     "version": "1.0.0",                   // Semantic versioning
     "unity": "2022.3",                    // Minimum Unity version
     "description": "Package description",  // Brief description
     "author": {
       "name": "Your Name",
       "email": "your.email@example.com"
     }
   }
   ```

2. Configure Assembly Definitions:
   - Runtime: `Runtime/com.company.package-name.Runtime.asmdef`
   - Editor: `Editor/com.company.package-name.Editor.asmdef`

### GitHub Setup

1. Configure repository settings:
   - Navigate to Settings > Secrets and variables > Actions
   - Add variable: `PACKAGE_NAME` = `com.company.package-name`

2. Configure PAT for workflow triggers:
   - Create a Personal Access Token (PAT):
     1. Go to GitHub Settings > Developer settings > Personal access tokens
     2. Select "Tokens (classic)"
     3. Click "Generate new token (classic)"
     4. Set description and expiration (90 days recommended)
     5. Select only the `workflow` permission
   
   - Add PAT secret:
     1. Go to repository Settings > Secrets and variables > Actions
     2. Add new secret named `PAT`
     3. Value format (JSON):
     ```json
     {
       "token": "your_pat_token",
       "targets": [
         {
           "repo": "target-repo-name",
           "workflow": "workflow-file.yml"
         }
       ]
     }
     ```

3. Available GitHub Actions:
   - **Release Workflow** (`release.yml`)
     - Automatically creates Unity package and GitHub release
     - Generates release from CHANGELOG.md
     - Creates both .unitypackage and .zip formats
     - Can trigger other repository workflows after release
     - Triggered manually from Actions tab

   - **Cleanup Workflow** (`cleanup.yml`)
     - Helps manage repository releases and tags
     - Can remove all releases and tags when needed
     - Useful for development and maintenance
     - Triggered manually from Actions tab

### Development Workflow

1. **Code Organization**
   - Runtime code → `Runtime/`
   - Editor tools → `Editor/`

2. **Documentation**
   - API documentation → `Documentation~/`
   - Update README.md
   - Maintain CHANGELOG.md

3. **Release Process**
   - Update version in package.json
   - Update CHANGELOG.md with new version
   - Run Release workflow from GitHub Actions
   - Automated package and release creation
   - Automatically triggers configured workflows in other repositories

## Installation Methods

### Via Unity Package Manager

1. Open Package Manager window
2. Click "+" button
3. Select "Add package from git URL"
4. Enter: `https://github.com/Yueby/Template-Package.git`

### Manual Installation

1. Clone repository
2. In Package Manager, click "Add package from disk"
3. Select the package folder

## Requirements

- Unity 2022.3.22f1 or higher
- Git

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details. 