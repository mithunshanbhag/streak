# Learnings

- Product specs require habits to appear in alphabetical order by name. Persisted manual ordering is not part of the accepted UX, so `DisplayOrder` and reorder flows should not be reintroduced unless the specs change.
- Checkin records are presence-only: a `Checkins` row should exist only for completed days, and same-day uncheck should delete the row rather than persist a false state.
