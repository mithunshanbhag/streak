# Future Improvements

## Android Page Load and Startup Performance

This document captures follow-up work identified during an investigation into slow page loads and startup time, especially on Android devices.

The items below are prioritized by the strength of consensus across three independent investigations (Claude Opus 4.6, GPT-5.4, and Claude Sonnet 4.6), with the highest-consensus and highest-impact findings listed first.

## Highest Priority Findings

### 1. Move database bootstrap work off the UI thread

**Priority:** Critical  
**Consensus:** Strong (all three investigations)

The app currently performs SQLite database bootstrap work during window creation on the UI thread. On cold start, this can block first paint while file I/O, resource loading, connection open, and schema execution complete.

**Expected outcome**

- Faster perceived startup on Android
- Reduced splash-screen freeze on first launch
- Improved responsiveness before the first interactive frame

### 2. Remove the home page query fan-out

**Priority:** Critical  
**Consensus:** Strong (all three investigations)

The home page currently loads habits and then issues additional per-habit queries for streak state and today's check-in state. This creates an N+1 query pattern and scales poorly as habit count grows.

**Expected outcome**

- Faster home page loads
- Less SQLite overhead on Android hardware
- Lower battery and CPU cost during routine app use

### 3. Stop concurrent use of a shared `DbContext`

**Priority:** Critical  
**Consensus:** Strong (all three investigations)

The current dependency injection setup allows multiple concurrent operations to run against the same effective `DbContext` instance in the MAUI Blazor app. This is unsafe and can produce hangs, crashes, or intermittent failures under load.

**Expected outcome**

- Improved reliability during page loads
- Elimination of concurrency-related data access issues
- Safer foundation for future performance work

### 4. Optimize current streak calculation

**Priority:** Critical  
**Consensus:** Strong (all three investigations)

Current streak calculation loads full check-in history into memory for a habit and derives the streak in application code. This becomes increasingly expensive as history grows and is part of a hot path used repeatedly.

**Expected outcome**

- Faster home page rendering
- Faster habit details loading
- Better long-term scalability for users with large histories

## High Priority Findings

### 5. Avoid loading the same history twice on Habit Details

**Priority:** High  
**Consensus:** Medium-high

The Habit Details experience currently performs overlapping history work for streak and history visualization. The same underlying data should be loaded once and reused for derived calculations.

**Expected outcome**

- Lower page load time on Habit Details
- Less redundant database I/O
- Less duplicate CPU work

### 6. Push date filtering into SQL queries

**Priority:** High  
**Consensus:** Medium-high

Some history filtering currently happens after rows are materialized in memory rather than in the database query itself. This increases I/O, memory usage, and CPU work.

**Expected outcome**

- Smaller query result sets
- Lower memory pressure
- Faster history-related operations

### 7. Prevent unnecessary full reloads on Habit Details

**Priority:** High  
**Consensus:** Medium-high

The Habit Details page currently reloads its full data set during parameter updates even when the active habit has not changed.

**Expected outcome**

- Fewer redundant queries
- Faster navigation and re-render behavior
- Less unnecessary work on slower Android devices

### 8. Stop querying the database on each validation keystroke

**Priority:** High  
**Consensus:** Medium-high

The create/edit habit dialogs perform database-backed validation while the user types. Repeated queries during typing can introduce visible input lag on Android devices.

**Expected outcome**

- Smoother typing experience
- Reduced database activity during form entry
- Better perceived performance in dialogs

### 9. Remove startup dependency on externally hosted fonts

**Priority:** High  
**Consensus:** Medium-high

The app currently references Google Fonts from the network. This adds startup variability and can delay first paint when connectivity is slow or unavailable.

**Expected outcome**

- More predictable startup time
- Better offline behavior
- Lower dependence on network conditions for first render

## Medium Priority Findings

### 10. Use direct indexed lookups instead of full habit scans

**Priority:** Medium  
**Consensus:** Partial

Some habit lookup flows load all habits and then filter in memory rather than using targeted repository queries.

**Expected outcome**

- More efficient hot-path lookups
- Less unnecessary query work

### 11. Tune SQLite for mobile performance

**Priority:** Medium  
**Consensus:** Partial

The current SQLite configuration does not appear to take advantage of mobile-friendly performance settings such as WAL mode and less aggressive sync behavior.

**Expected outcome**

- Faster write operations
- Better responsiveness on Android flash storage

### 12. Reduce heatmap rendering cost on mobile

**Priority:** Medium  
**Consensus:** Partial

The heatmap view renders a large number of interactive UI elements, including tooltip-related components that are less valuable on touch devices.

**Expected outcome**

- Faster Habit Details rendering
- Smaller component tree on Android
- Reduced rendering overhead in the WebView

### 13. Avoid unnecessary layout re-renders on navigation

**Priority:** Medium  
**Consensus:** Partial

The main layout currently forces a render on navigation even when visible layout state may not have changed.

**Expected outcome**

- Fewer unnecessary renders
- Smoother navigation transitions

### 14. Reduce repeated style/string recomputation during rendering

**Priority:** Medium  
**Consensus:** Partial

Some UI components rebuild style strings and derived display values repeatedly during render cycles instead of reusing stable values.

**Expected outcome**

- Lower allocation pressure
- Slightly cheaper render passes

### 15. Revisit habit ID generation strategy

**Priority:** Medium  
**Consensus:** Partial

Habit creation currently derives the next identifier through application-side list inspection, which is inefficient and can create race-condition risk if concurrency expands later.

**Expected outcome**

- Simpler create flow
- Less unnecessary read work
- Safer long-term persistence behavior

## Suggested Delivery Order

1. Move DB bootstrap work off the UI thread
2. Fix `DbContext` lifetime/concurrency behavior
3. Collapse home page data loading into fewer queries
4. Optimize streak calculation
5. Eliminate duplicate Habit Details history loads
6. Push date filtering into SQL
7. Remove unnecessary reloads and validation queries
8. Remove network font dependency
9. Tackle lower-level rendering and SQLite tuning improvements

## Success Criteria for Future Work

Future implementation work should aim to satisfy the existing product expectations in `/home/runner/work/streak/streak/docs/specs/README.md`, especially:

- App launch should be ready for interaction within 2 seconds
- Check-in toggling should feel instant
- The app should work fully offline
- The app should maintain a minimal battery and storage footprint
