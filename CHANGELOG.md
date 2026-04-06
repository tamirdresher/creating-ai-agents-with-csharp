# Changelog

## [Unreleased] — MS Agent Framework GA + .NET 10 Update

### Changed
- **`global.json`**: Updated .NET SDK target from `9.0.0` → `10.0.0` (MS Agent Framework requires .NET 10)
- **`src/SKCodeAssistent/Directory.Packages.props`**: Package updates:
  - `Microsoft.Extensions.AI` `9.7.1` → `10.4.0` (GA with .NET 10, was preview)
  - `Microsoft.Extensions.AI.Abstractions` `9.7.1` → `10.4.0`
  - `Microsoft.Extensions.AI.OpenAI` `9.7.1-preview` → `10.4.0` (now stable)
  - `ModelContextProtocol` `0.5.0-preview.1` → `1.1.0` (stable release 🎉)
  - `A2A` / `A2A.AspNetCore` `0.3.3-preview` → `0.3.4-preview`
  - `Microsoft.AspNetCore.OpenApi` `9.0.0-preview.4.24267.6` → `10.0.0` (GA)
  - `Azure.AI.OpenAI` `2.5.0-beta.1` → `2.9.0-beta.1`
  - Added `Azure.AI.Projects` `2.0.0` (Azure Foundry GA)
  - Added `Azure.Identity` `1.20.0`
  - Added `Microsoft.Agents.AI` `1.0.0` (new MS Agent Framework GA)
  - Added `Microsoft.Agents.AI.OpenAI` `1.0.0`
  - Added `Microsoft.Agents.AI.AzureAI` `1.0.0`

### Added
- **`README.md`**: New "What's New — Microsoft Agent Framework GA (.NET 10)" section with:
  - Feature table, quick hello-agent example
  - Comparison of Semantic Kernel vs MS Agent Framework
  - Package versions table
  - Links to official MS docs and migration guide
- **`notebooks/README.md`**: Added "Notebook Status" section clarifying:
  - Polyglot Notebooks are still supported and recommended for exploration
  - MS Agent Framework official samples use console apps, not notebooks
  - Updated prerequisites to .NET 10 SDK
  - Added Polyglot Notebooks VS Code extension link

### Context
- [Microsoft Agent Framework GA announcement](https://github.com/microsoft/agent-framework)
- Addresses workshop repo issue: update for MS Agent Framework GA release with .NET 10
