# Package Documentation

## Overview

This Unity package template provides a standardized foundation for creating custom Unity packages. This documentation will guide you through using and developing with this template.

## Getting Started

### Prerequisites
- Unity 2022.3.22f1 or later
- Git
- Basic knowledge of Unity package development

### Installation
Please refer to the [README.md](../README.md) for detailed installation instructions.

## Package Structure

```
com.template.package/
├── Runtime/           # Runtime scripts and assets
│   └── *.asmdef      # Runtime assembly definition
├── Editor/           # Editor-only scripts and assets
│   └── *.asmdef      # Editor assembly definition
├── Documentation~/   # Package documentation
│   └── index.md      # Main documentation
├── .github/          # GitHub configurations
│   └── workflows/    # GitHub Actions workflows
├── package.json      # Package manifest
├── README.md         # Quick start guide
├── CHANGELOG.md      # Version history
└── LICENSE          # MIT license
```

## Development Guide

### Runtime Development
- Place runtime scripts in `Runtime/`
- Follow Unity's coding conventions
- Keep dependencies minimal
- Document public APIs

### Editor Development
- Place editor scripts in `Editor/`
- Use `UnityEditor` namespace
- Create custom inspectors when needed
- Add editor utilities

### Documentation
- Document all public APIs
- Include code examples
- Keep documentation up-to-date
- Follow Unity's documentation style

## Best Practices

### Code Organization
- Use meaningful namespaces
- Follow C# coding conventions
- Keep classes focused and single-purpose
- Use interfaces for flexibility

### Asset Management
- Use appropriate asset formats
- Optimize asset sizes
- Follow Unity's asset naming conventions
- Document asset requirements

### Version Control
- Use meaningful commit messages
- Follow semantic versioning
- Update CHANGELOG.md for each release
- Tag releases properly

## API Reference

### Runtime APIs
Documentation for runtime classes and methods will be here.

### Editor APIs
Documentation for editor-specific functionality will be here.

## Version History
Please refer to [CHANGELOG.md](../CHANGELOG.md) for detailed version history. 