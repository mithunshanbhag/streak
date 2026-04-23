# Future Improvements

## Android Page Load and Startup Performance

This document tracks the performance follow-up items that are still open after the latest round of fixes.

## Highest Priority Findings

### 1. Stop concurrent use of a shared `DbContext`

**Priority:** Critical

The MAUI Blazor host still registers `StreakDbContext` through `AddDbContext(...)` and resolves repositories/services from the root container. That leaves the app with a long-lived effective context during overlapping UI work, which is still a reliability and performance risk. The existing detach safeguards help with tracking conflicts, but they do not fix the underlying lifetime model.

**Expected outcome**

- Improved reliability during page loads and dialog workflows
- Elimination of context-concurrency failures under overlapping operations
- Safer foundation for further performance work

### 2. Optimize current streak calculation

**Priority:** Critical

`CheckinService.GetCurrentStreakAsync(...)` still loads the habit's full check-in history and calculates the streak in application code. Homepage loading already batches history more efficiently, but Habit Details and same-day refresh paths still pay for full-history reads.

**Expected outcome**

- Faster Habit Details loading
- Less memory churn as history grows
- Better long-term scalability for large local histories

### 3. Avoid loading the same history twice on Habit Details

**Priority:** High

Habit Details still calls `GetCurrentStreakAsync(...)` and `GetHistoryAsync(...)` separately for the same habit. That duplicates database work and repeats history processing during one page load.

**Expected outcome**

- Lower page load time on Habit Details
- Less redundant database I/O
- Less duplicate CPU work

### 4. Prevent unnecessary full reloads on Habit Details

**Priority:** High

`HabitDetails.razor` still reloads its full data set inside `OnParametersSetAsync()` even when the active `HabitId` has not changed. This can trigger redundant work during re-renders and in-page updates.

**Expected outcome**

- Fewer redundant queries
- Faster navigation and re-render behavior
- Less unnecessary work on slower Android devices

### 5. Stop querying the database on each validation keystroke

**Priority:** High

The new/edit habit dialogs still run async uniqueness validation against `HabitService.GetAllAsync()` while the user types, with a short debounce. That continues to introduce avoidable database traffic during form entry.

**Expected outcome**

- Smoother typing experience
- Reduced database activity during dialog input
- Better perceived responsiveness on Android

## Medium Priority Findings

### 6. Use direct repository lookups instead of full habit scans

**Priority:** Medium

Some service-layer flows still load all habits and filter in memory even though targeted repository APIs already exist. This is most visible in habit-name lookups and uniqueness checks.

**Expected outcome**

- More efficient hot-path lookups
- Less unnecessary query work

### 7. Tune SQLite for mobile performance

**Priority:** Medium

The current SQLite startup/connection path still does not apply mobile-focused settings such as WAL mode or tuned sync behavior.

**Expected outcome**

- Faster writes
- Better responsiveness on Android flash storage

### 8. Reduce heatmap rendering cost on mobile

**Priority:** Medium

The heatmap still renders a large grid of interactive elements, including tooltip wrappers for every day cell. On touch devices, that is a relatively expensive component tree for limited interaction value.

**Expected outcome**

- Faster Habit Details rendering
- Smaller component tree on Android
- Reduced WebView rendering overhead

### 9. Avoid unnecessary layout re-renders on navigation

**Priority:** Medium

`MainLayout.razor` still forces `StateHasChanged()` in `HandleLocationChanged(...)` after updating route state. That may still trigger unnecessary layout work during navigation.

**Expected outcome**

- Fewer redundant renders
- Smoother navigation transitions

### 10. Reduce repeated style/string recomputation during rendering

**Priority:** Medium

Some UI components still rebuild style strings and derived display values during render cycles, especially in the heatmap path.

**Expected outcome**

- Lower allocation pressure
- Slightly cheaper render passes

### 11. Revisit habit ID generation strategy

**Priority:** Medium

Habit creation still derives the next identifier by loading all habits and computing `Max(Id) + 1` in application code. That is inefficient and becomes riskier if concurrency expands later.

**Expected outcome**

- Simpler create flow
- Less unnecessary read work
- Safer long-term persistence behavior

## Suggested Delivery Order

1. Fix `DbContext` lifetime/concurrency behavior
2. Optimize streak calculation
3. Eliminate duplicate Habit Details history loads
4. Remove unnecessary Habit Details reloads and validation queries
5. Replace full habit scans with direct lookups
6. Tackle SQLite tuning and rendering refinements

## Success Criteria for Future Work

Future implementation work should continue to satisfy the existing product expectations in `docs\specs\README.md`, especially:

- App launch should be ready for interaction within 2 seconds
- Check-in toggling should feel instant
- The app should work fully offline
- The app should maintain a minimal battery and storage footprint
