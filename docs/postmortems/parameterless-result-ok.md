# Postmortem: Parameterless `Result.Ok()`

## Summary

The parameterless `Result<Unit, E>.Ok()` factory did not reliably create the
successful `Unit` result callers expected. Because that API was not dependable,
versions that relied on it were left out. The factory now works and has
regression coverage.

## What broke

The no-argument `Ok()` path is intended for operations that succeed without
returning a meaningful value. It must produce the same success state as
`Result<Unit, E>.Ok(Unit.Value)`. That behavior was not working correctly, so
callers could not safely use the concise no-argument factory.

## Why tests missed it

The existing checks for the parameterless factory were narrow: they checked
success, equality with `Ok(Unit.Value)`, and the `Match` result. They did not
independently verify the success and failure accessors or explicitly assert
that the failure accessor rejects the returned result. As a result, coverage
did not fully describe the public contract of the no-argument path.

## Resolution and regression coverage

The parameterless factory now behaves as a successful `Unit` result. The
`Result_ParameterlessOk_CreatesSuccessWithUnit` test verifies both state
properties, equality with `Ok(Unit.Value)`, `TryGetValue`, `TryGetError`, and
the `Match` value. `Result_ParameterlessOk_ErrStillFails` verifies that an
error result remains distinct from the no-argument success.

The targeted tests pass. Versions that were left out because of this behavior
can now rely on the working factory.
