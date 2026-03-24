# Learnings

- Product specs require habits to appear in alphabetical order by name. Persisted manual ordering is not part of the accepted UX, so `DisplayOrder` and reorder flows should not be reintroduced unless the specs change.
- Checkin records are presence-only: a `Checkins` row should exist only for completed days, and same-day uncheck should delete the row rather than persist a false state.
- Habit deletion is confirmed through a `MudDialog` on the Habit Details page, while the page itself owns the `IHabitService.DeleteAsync(...)` call, redirects to home on success, and relies on SQLite cascade delete to remove related `Checkins`.
