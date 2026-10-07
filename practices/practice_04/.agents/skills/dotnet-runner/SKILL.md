---
name: dotnet-runner
description: Minimal dotnet build/test/run executor for OpenCode. Provides tools to run dotnet build, test, and run on the current repo. Use when you need to compile or test .NET projects from the agent.
license: MIT
---

# Dotnet Runner

This skill provides three simple tools:

- dotnet.build: runs `dotnet build` in the workspace or specified path
- dotnet.test: runs `dotnet test` with optional project/solution path and flags
- dotnet.run: runs `dotnet run` for a specified project

The tools execute commands in the repository root by default. Provide `project` to target a specific .csproj/.sln.

## Tools

```tool
name: dotnet.build
description: Build a .NET project/solution. Defaults to repository root.
input:
  type: object
  properties:
    project:
      type: string
      description: Optional path to .sln or .csproj
    configuration:
      type: string
      description: Build configuration (Debug/Release)
  required: []
```

```tool
name: dotnet.test
description: Run .NET tests. Provide project/solution path if needed.
input:
  type: object
  properties:
    project:
      type: string
      description: Optional path to .sln or .csproj
    noBuild:
      type: boolean
      description: If true, adds --no-build
    filter:
      type: string
      description: Optional test filter expression
    additionalArgs:
      type: string
      description: Additional raw CLI args appended
  required: []
```

```tool
name: dotnet.run
description: Run a .NET project using dotnet run.
input:
  type: object
  properties:
    project:
      type: string
      description: Path to a .csproj to run
    additionalArgs:
      type: string
      description: Additional raw CLI args appended after --
  required: [project]
```

## Implementation Notes

- These tools shell out to dotnet CLI. They do not modify files.
- Return the command executed and exit code; include captured stdout/stderr.
- Keep commands minimal and repo-agnostic.
