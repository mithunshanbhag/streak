# Habit Details Page

> **Route**: `/habits/{habitId}`

The habit details page shows a **detailed view of a single habit**, including its editable habit details, a GitHub-style calendar heatmap, and a prominent current streak counter.

## Navigation

- The user arrives here by **tapping a habit card** on the [Habits page](./habits-page.md), including its visible streak badge.
- The page title in the app bar shows the habit's current emoji and name (e.g., "🧘 Meditate").
- A **back arrow** in the app bar returns the user to the previous in-app page. If there is no in-app history, it falls back to the [Habits page](./habits-page.md).

## Layout

The page is a single-column, vertically scrollable layout with three main sections:

### 1. Habit Details Header

Displayed at the top of the page.

#### View Mode

| Element        | Details                                                                                  |
| -------------- | ---------------------------------------------------------------------------------------- |
| Emoji / icon   | The habit's emoji or default icon                                                        |
| Habit name     | Primary label for the habit                                                              |
| Edit action    | Pencil icon. Switches the header into inline edit mode                                   |
| Delete action  | Bin icon. Opens a destructive confirmation dialog                                        |

#### Inline Edit Mode

When the user taps the edit icon, the header switches into an inline editing state.

| Field | Type                       | Required | Validation                                                                                                  |
| ----- | -------------------------- | -------- | ----------------------------------------------------------------------------------------------------------- |
| Name  | Text input                 | Yes      | 1–30 characters. Must be unique among the user's habits (case-insensitive), excluding the current habit.   |
| Emoji | Emoji picker or text input | No       | Single emoji character. If left empty, a default icon is used.                                              |

- **Save** applies the updated name and/or emoji without leaving the page.
- **Cancel** discards any unsaved edits and returns to view mode.
- Editing a habit's name or emoji does **not** affect its checkin history or streak.

#### Delete Confirmation Dialog

When the user taps the delete icon:

- Show a confirmation dialog over the current page.
- Message: *"Delete '{habit name}'? All checkin history for this habit will be permanently lost."*
- **Delete** permanently deletes the habit and all associated checkin history, then returns the user to the previous in-app page (or [Habits](./habits-page.md) if there is no in-app history).
- **Cancel** closes the dialog and keeps the user on the page.

### 2. Current Streak Counter

Displayed prominently at the top of the page.

| Element       | Details                                                                                                                      |
| ------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| Streak number | Large bold text (32sp) showing the current streak count (e.g., "12").                                                        |
| Label         | Caption text below the number: *"day streak"* (or *"days streak"* if > 1). Shows *"No active streak"* when the count is 0. |
| Streak emoji  | 🔥 displayed next to the number when streak ≥ 7 days. 😎 displayed when streak is 1–6 days. No emoji when streak is 0.         |

### 3. Calendar Heatmap

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

| Scenario                   | Behavior                                                                                   |
| -------------------------- | ------------------------------------------------------------------------------------------ |
| Habit created today        | Streak counter shows 0. Heatmap shows only today's cell (empty or filled).                 |
| Habit with no checkins     | Streak shows 0 / "No active streak". Heatmap shows all cells as muted.                     |
| Very old habit (> 90 days) | Heatmap initially shows last 90 days; user can scroll to see older data.                   |
| User renames a habit       | The app-bar title, habit header, and any alphabetical lists update immediately after save. |
| User clears the emoji      | The default icon is shown after save.                                                       |

## Visual Style

- The heatmap cells should be small squares (approx. 12–14dp) with 2dp gaps, similar to GitHub's contribution graph.
- Use rounded corners (2dp radius) on each cell.
- The overall heatmap section should have a subtle card or container background to visually separate it from the streak counter above.
