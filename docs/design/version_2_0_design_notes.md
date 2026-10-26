# DN-000: Title

Status: Draft | Proposed | Accepted | Rejected | Superseded by DN-xxx
Date:
Owner:

## Context
What problem are we solving? What is wrong today? Link to issues or PRs.

## Constraints
Target frameworks, C# language version, compatibility with satellite
packages (EF Core, ASP.NET Core, ObjectPool), serialization.

## Options considered
### Option A: ...
### Option B: ...
(For each: short description, small code sample of the public API.)

## Evaluation
| Criterion                          | A | B | C |
|------------------------------------|---|---|---|
| Exhaustive switch without `_ =>`   |   |   |   |
| Allocations (benchmark numbers)    |   |   |   |
| Source compatibility with 1.8      |   |   |   |
| Binary compatibility with 1.8      |   |   |   |
| System.Text.Json / EF Core / MVC   |   |   |   |
| Needs a newer TFM or LangVersion   |   |   |   |
| Analyzer impact                    |   |   |   |
| Migration cost for users           |   |   |   |

## Experiment results
What was built, how it was measured, links to the spike branch.

## Decision
What we chose and why.

## Consequences
What gets easier, what gets harder, what must be documented.

## Migration path
How 1.8 users get to the new design. Which obsoletes and code fixes ship.