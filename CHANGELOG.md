# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.2] - 2025-07-08
### Added
- State management system with visual indicators
- `QuickAction.SetVisible()` and `QuickAction.GetVisible()` for dynamic action visibility
- `QuickAction.SetChecked()` and `QuickAction.GetChecked()` for toggle state display
- Visual checkmark indicators using colored left borders
- Context-aware action visibility (actions show/hide based on selection state)

### Changed
- Validation functions can now control action visibility and checked state
- Inspector actions (Lock/Debug Mode) now show current state with checkmarks
- Window actions (Maximize Game/Scene View) display maximize state
- Asset actions only appear when assets are selected
- GameObject actions only appear when GameObjects are selected
- Transform actions only appear when objects are selected
- Improved ActionButton implementation with custom UXML/USS structure

### Fixed
- Validation functions no longer cause window focus changes
- Eliminated duplicate code in Inspector and Window actions
- Performance improvements by using local state variables instead of reflection queries
- Removed side effects from validation functions

### Technical Improvements
- Refactored ActionRegistry functionality into QuickAction core class
- Optimized validation function performance
- Added comprehensive state management API
- Enhanced ActionButton with customizable checkmark display

## [1.0.1] - 2025-07-06
### Changed
- Change opacity to 0.9

## [1.0.0] - 2025-07-06
### Added
- Initial release of Quick Action System
