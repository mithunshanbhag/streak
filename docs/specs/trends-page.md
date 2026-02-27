# Trends Page

> **Route**: `/trends/{habitId}`

The trends page shows a **detailed view of a single habit's history**, including a GitHub-style calendar heatmap and a prominent current streak counter.

## Navigation

- The user arrives here by **tapping the streak count / badge** on a habit card on the [Home page](./home-page.md).
- The page title in the app bar shows the habit's emoji and name (e.g., "🧘 Meditate").
- A **back arrow** in the app bar returns the user to the Home page.

## Layout

The page is a single-column, vertically scrollable layout with two main sections:

### 1. Current Streak Counter

Displayed prominently at the top of the page.

| Element       | Details                                                                                                                    |
| ------------- | -------------------------------------------------------------------------------------------------------------------------- |
| Streak number | Large bold text (32sp) showing the current streak count (e.g., "12").                                                      |
| Label         | Caption text below the number: *"day streak"* (or *"days streak"* if > 1). Shows *"No active streak"* when the count is 0. |
| Streak emoji  | 🔥 displayed next to the number when streak ≥ 7 days. 😎 displayed when streak is 1–6 days. No emoji when streak is 0.       |

### 2. Calendar Heatmap

A GitHub-contribution-style grid that visualizes daily checkin history.

| Property     | Details                                                                                                                                                              |
| ------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Time range   | Last **90 days** (approximately 13 weeks), scrollable or paginated to view older data.                                                                               |
| Grid layout  | 7 rows (one per weekday, Mon–Sun) × N columns (one per week). Most recent week on the right.                                                                         |
| Cell colors  | **Done**: filled with the secondary color (Teal). **Not done**: muted/empty cell. **Future dates**: not shown. **Today**: outlined or highlighted to distinguish it. |
| Day labels   | Abbreviated weekday labels on the left (M, W, F — skip alternates for space).                                                                                        |
| Month labels | Abbreviated month names along the top, aligned to the first week of each month.                                                                                      |

#### Heatmap Interactions

| Action            | Result                                                                                   |
| ----------------- | ---------------------------------------------------------------------------------------- |
| Tap a cell        | Shows a tooltip with the date and status (e.g., "Feb 15 — Done ✅" or "Feb 14 — Missed"). |
| Scroll/swipe left | Reveals older history beyond the 90-day window (if data exists).                         |

## Edge Cases

| Scenario                   | Behavior                                                                   |
| -------------------------- | -------------------------------------------------------------------------- |
| Habit created today        | Streak counter shows 0. Heatmap shows only today's cell (empty or filled). |
| Habit with no checkins     | Streak shows 0 / "No active streak". Heatmap shows all cells as muted.     |
| Very old habit (> 90 days) | Heatmap initially shows last 90 days; user can scroll to see older data.   |

## Visual Style

- The heatmap cells should be small squares (approx. 12–14dp) with 2dp gaps, similar to GitHub's contribution graph.
- Use rounded corners (2dp radius) on each cell.
- The overall heatmap section should have a subtle card or container background to visually separate it from the streak counter above.
