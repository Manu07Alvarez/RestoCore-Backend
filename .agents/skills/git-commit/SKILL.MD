---
name: "git-commit"
description: "Create git commits following repository conventions"
compatibility: "Requires git repository"
metadata:
  author: "Manu07Alvarez"
  source: "custom"
---

## Goal

Create a git commit using repository commit conventions.

## Commit Types

Use exactly one prefix:

- FEATURE: New functionality
- BUG: Defect correction
- FIX: Refactor, optimization, cleanup, or non-defect improvements

## Execution Steps

1. Analyze staged changes.
2. Determine the change category.
3. Generate a commit message.
4. Display the proposed message.
5. Request confirmation unless explicitly instructed to commit immediately.
6. Execute git commit.

## Commit Rules

Do not use:

- update code
- changes
- fixes
- work
- WIP

Commit messages must be concise and descriptive.

## Examples

FEATURE: implement booking moderation workflow

BUG: prevent duplicate booking acceptance

FIX: simplify notification dispatch service