---
name: speckit-git-commit
description: Commit changes on an isolated branch with standard naming conventions (FEATURE, BUG, FIX, TODO) and create a Pull Request upon completion
compatibility: Requires spec-kit project structure with .specify/ directory
metadata:
  author: github-spec-kit
  source: git:commands/speckit.git.commit.md
---

# Commit & Create Pull Request

Explicitly stage changes, ensure execution on an isolated branch matching the work type (`feature/`, `bug/`, `fix/`, `todo/`, `refactor/`, `docs/`), commit changes, push to origin, and create a Pull Request (PR).

## User Input

```text
$ARGUMENTS
```

Accept optional arguments:
- Branch type: `feature`, `bug`, `fix`, `todo`, `refactor`, `docs`
- Short title or message (e.g. `/speckit-git-commit fix/jwt-token-refresh "Fix expired JWT token handling"`)

## Branch Naming Conventions

When creating or checking out an isolated branch for the PR, enforce one of the following standard prefix conventions:

- `feature/<short-name>` or `FEATURE/<short-name>`: New feature implementations or enhancements.
- `bug/<short-name>` or `BUG/<short-name>`: Bug fixes and issue resolutions.
- `fix/<short-name>` or `FIX/<short-name>`: Quick hotfixes, patch fixes, or urgent corrections.
- `todo/<short-name>` or `TODO/<short-name>`: Technical debt, refactoring pending items, or TODO-TREE tag cleanups.
- `refactor/<short-name>`: Architectural cleanups and refactoring without changing functionality.
- `docs/<short-name>`: Documentation updates and specification docs.

If the current branch does not have one of these prefixes and is `main` or `master`, prompt to create and switch to an isolated branch with the correct prefix before pushing.

## TODO-TREE Integration Audit

Before committing and opening the PR, scan modified files for TODO-TREE tags:
- `TODO:` - Future tasks or missing functionality
- `FIXME:` - Known issues requiring resolution
- `BUG:` - Identified bug behaviors
- `HACK:` - Temporary workarounds
- `XXX:` - Critical code smells or warnings
- `REVIEW:` - Code sections needing peer review

Include a summary of detected TODO-TREE tags in the PR body description so unresolved marks remain tracked.

## Execution

To stage, commit, push, and open a Pull Request:

- **PowerShell**: `.specify/extensions/git/scripts/powershell/create-pr.ps1 -Type "<type>" -Title "<title>" -Description "<description>"`
- **Bash**: `.specify/extensions/git/scripts/bash/create-pr.sh --type "<type>" --title "<title>" --description "<description>"`

If the GitHub CLI (`gh`) is installed and authenticated:
- Automatically runs `gh pr create --base main --head <branch-name> --title "<title>" --body "<body>"`

If `gh` CLI is not installed or available:
- Pushes the branch to `origin` (`git push -u origin <branch-name>`)
- Displays the formatted PR Markdown template and a direct link to create the PR manually on GitHub.

## Configuration

In `.specify/extensions/git/git-config.yml`:

```yaml
pull_request:
  enabled: true
  base_branch: main
  auto_push: true
  require_isolated_branch: true
  conventions:
    - feature
    - bug
    - fix
    - todo
    - refactor
    - docs
```

## Graceful Degradation

- If Git is not available or the current directory is not a repository: skips with a warning.
- If no changes to commit and branch is already pushed: prompts user whether to create PR for existing commits.