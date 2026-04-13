# Habit Details Page

> **Route**: `/habits/{habitId}`

The habit details page shows a **detailed view of a single habit**, including editable habit details, an optional saved description, a compact streak summary, and a GitHub-style history heatmap that is treated as secondary information.

## Navigation

- The user arrives here by **tapping a habit card** on the [Homepage](./homepage.md), including its visible streak badge.
- The page title in the app bar shows the habit's current emoji and name (e.g., "🧘 Meditate").
- A **back arrow** in the app bar always returns the user to the [Homepage](./homepage.md).
- Secondary-screen chrome stays focused: use **Back** + title only. Do not repeat the root-screen `+` or `Settings` actions here.

## Layout

The page is a single-column, vertically scrollable layout with two main sections:

### 1. Habit Summary Card

Displayed at the top of the page. This section combines identity, current streak, and edit access into one compact surface instead of stacking multiple top-level cards.

#### View Mode

| Element        | Details                                                                  |
| -------------- | ------------------------------------------------------------------------ |
| Emoji / icon   | The habit's emoji or default icon                                        |
| Habit name     | Primary label for the habit                                              |
| Description    | Optional plain-text body copy shown only when a saved description exists |
| Current streak | Large streak number plus supporting label in the same card               |
| Edit action    | Pencil icon. Opens the edit dialog for the current habit                 |
| More actions   | Overflow menu (`MoreVert`). Contains the delete action                   |

- Prefer `MudCard` or `MudPaper`, `MudText`, `MudIconButton`, and `MudMenu`.
- Use built-in spacing utilities rather than custom section wrappers.
- If the habit has no description, omit the description block entirely rather than showing an empty placeholder.

#### Edit Habit Dialog

When the user taps the edit icon, show a standard dialog over the current page with the habit's current values prefilled.

| Field       | Type                       | Required | Validation                                                                                                               |
| ----------- | -------------------------- | -------- | ------------------------------------------------------------------------------------------------------------------------ |
| Name        | Text input                 | Yes      | 1–30 characters. Must be unique among the user's habits (case-insensitive), excluding the current habit.                 |
| Emoji       | Emoji picker or text input | No       | Single emoji character. If left empty, a default icon is used.                                                           |
| Description | Multiline text input       | No       | Plain text only. Up to 500 characters. Line breaks are preserved. Clearing the value removes the description after save. |

- **Save** applies the updated name, emoji, and/or description, closes the dialog, and keeps the user on the Habit Details page.
- **Cancel** closes the dialog and leaves the current habit unchanged.
- Editing a habit's name, emoji, or description does **not** affect its checkin history or streak.
- After save, the app-bar title, summary card, description block, and any dependent habit-name displays should refresh immediately.
- Prefer a standard `MudDialog`, `MudForm`, `MudTextField`, and standard `MudButton` actions before creating any custom form styling.

#### Delete Confirmation Dialog

When the user chooses **Delete** from the overflow menu:

- Show a confirmation dialog over the current page.
- Message: *"Delete '{habit name}'? All checkin history for this habit will be permanently lost."*
- **Delete** permanently deletes the habit and all associated checkin history, then returns the user to the previous in-app page (or [Homepage](./homepage.md) if there is no in-app history).
- **Cancel** closes the dialog and keeps the user on the page.
- Prefer a standard `MudDialog` rather than a custom destructive modal treatment.

#### Current Streak Display

Displayed prominently inside the summary card.

| Element       | Details                                                                                                                                                                                                             |
| ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Streak number | Large bold text (32sp) showing the current streak count (e.g., "12").                                                                                                                                               |
| Label         | Caption text below the number: *"day streak"* (or *"days streak"* if > 1). Shows *"No active streak"* when the count is 0.                                                                                          |
| Streak emoji  | 🐐 displayed when streak ≥ 30 days. 🔥 displayed when streak is 10–29 days. 😎 displayed when streak is 6–9 days. 👏 displayed when streak is 3–5 days. 👍 displayed when streak is 1–2 days. No emoji when streak is 0. |

### 2. History Panel

The history surface is secondary to the daily flow and should be **collapsed by default** behind a disclosure such as **"Show history"**.

- Prefer `MudExpansionPanels` / `MudExpansionPanel` for the disclosure pattern.
- When expanded, show a GitHub-contribution-style grid that visualizes daily checkin history.

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

- Prefer `MudTooltip` for per-cell feedback when feasible.

## Edge Cases

| Scenario                    | Behavior                                                                                   |
| --------------------------- | ------------------------------------------------------------------------------------------ |
| Habit created today         | Streak counter shows 0. Heatmap shows only today's cell (empty or filled).                 |
| Habit with no checkins      | Streak shows 0 / "No active streak". Heatmap shows all cells as muted.                     |
| Very old habit (> 90 days)  | Heatmap initially shows last 90 days; user can scroll to see older data.                   |
| Habit with no description   | Hide the description block and keep the summary layout compact.                            |
| User renames a habit        | The app-bar title, habit header, and any alphabetical lists update immediately after save. |
| User clears the description | The saved description disappears from the summary card after save.                         |
| User clears the emoji       | The default icon is shown after save.                                                      |

## Visual Style

- The heatmap cells should be small squares (approx. 12–14dp) with 2dp gaps, similar to GitHub's contribution graph.
- Use rounded corners (2dp radius) on each cell.
- The expanded history area should sit inside a subtle card or expansion-panel surface.
- Minimal inline sizing/style is acceptable for the heatmap cell grid if MudBlazor's standard primitives do not express the layout cleanly.
