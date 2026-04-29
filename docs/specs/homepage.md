# Homepage (Landing / Check-in)

> **Route**: `/`

The homepage is the **primary surface** of the Streak app. Users should be able to open the app, check in their habits for the day, and leave — all without navigating anywhere.

## Layout

The page displays a **vertical list of habit cards**, one per habit.

- Habits are sorted **alphabetically by name**.
- There is no separate manage-habits screen; the homepage is the main habit list surface.
- Prefer a `MudContainer` + `MudStack` composition with built-in spacing utilities rather than custom page layout CSS.

### Date Banner

- Display the current date prominently at the top of the content area, **above** the habit card list.
- Format: full weekday name, abbreviated month, and day number — for example `Friday, Mar 27`.
- This banner helps users confirm exactly which calendar day they are checking in for, and is especially useful when the app is opened around midnight.
- The banner always reflects the device's current **local** date, never UTC.
- The date is **read-only** and non-interactive; it must not open any dialog or navigate anywhere.
- See [Common UI Specifications — Date Banner](./ui.md#date-banner) for component and styling guidance.

### List Start

- Below the date banner, the page shows the habit list with no additional summary or status lines.
- Do **not** show a remaining-habits message such as `2 left today` or a total-habit-count chip on this screen.
- Do **not** show an A-Z sort chip or explanatory helper text on this screen.

### Habit Card

Each card shows:

| Element        | Position            | Details                                                                                                                             |
| -------------- | ------------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| Emoji / Icon   | Left                | The habit's emoji, or a default icon if none is set.                                                                                |
| Habit name     | Main text column    | The habit's name label. Tapping the card body opens the [Habit Details page](./habit-details-page.md) for that habit.               |
| Current streak | Secondary text line | A compact secondary line under the habit name showing the streak (for example, `😎 3 day streak`, `🔥 12 day streak`, or `0 streak`). |
| Checkin toggle | Right               | A `MudCheckBox` configured with custom icons. Done = green check circle icon. Not done = empty circle icon.                         |

- Cards use the full width of the screen with standard horizontal padding (16dp).
- Vertical spacing between cards: 12dp.
- Habit descriptions are intentionally **not** shown on homepage cards; this surface stays limited to emoji, name, streak, and toggle state.
- Tapping anywhere on a habit card **except** the checkin toggle navigates to that habit's [Habit Details page](./habit-details-page.md).
- Prefer `MudCard` or `MudPaper` plus `MudStack`, `MudText`, and utility classes before introducing any custom card-specific CSS.

### New Habit CTA

- When the user already has habits and is still below the 10-habit limit, show a full-width **`+ New Habit`** button below the list.
- Separate the list from this CTA with a subtle horizontal divider.
- Tapping **`+ New Habit`** opens the [Quick Add Habit dialog](./create-habit-page.md) over the homepage.
- Hide the CTA when the user is already at the maximum habit count.

## Post-startup Permission Recovery

- On Android, once startup initialization has completed and the homepage content is fully visible, the app should check for any missing runtime permissions tied to enabled features or homepage-adjacent flows.
- If one or more required permissions are missing, the app should present the corresponding Android system permission dialog(s) at that point.
- The homepage must render first; permission prompting is a **post-startup** action, not part of the splash / initial route gate.
- This check should cover:
  - notification permission when daily reminders, daily automated local backups, or daily automated OneDrive backups are already enabled from persisted state
  - camera permission for the optional picture-proof capture flow used by homepage check-ins

## Checkin Toggle Behavior

- **Default state**: Empty circle (not done) at the start of each new **local** calendar day.
- **Toggling to done**:
  - Opens the dedicated [Check-in Dialog](./checkin-dialogs.md#check-in-dialog) over the homepage before today's checkin is persisted.
  - The dialog can collect an optional short note plus one optional picture proof.
  - The homepage remains visible behind the dialog, dimmed.
  - The green check state and updated streak appear only after the user completes the dialog successfully.
- **Toggling to not done**:
  - Opens the dedicated [Undo Check-in Dialog](./checkin-dialogs.md#undo-check-in-dialog) before deleting today's checkin.
  - The dialog warns that removing the checkin also removes any associated optional note and picture-proof details from the app's check-in record.
  - **Remove check-in** deletes today's checkin record, changes the icon back to an empty circle, and updates the streak.
  - **Keep check-in** (or dismissing the dialog) closes the confirmation and leaves the current done state unchanged.
- Saved notes and saved picture proofs are captured and persisted with the checkin, but they are **not shown on the homepage card** in this scope.
- After each completed toggle action, the habit remains in the same alphabetical position; checking in does **not** change the list order.
- Checkins are persisted to local storage immediately after the user completes the dialog flow.
- If the device timezone changes because of travel or manual clock/timezone edits, the app's meaning of **today** changes with the device's current local timezone.

## Visual States

| State             | Toggle               | Streak Badge                                                                                              | Card Appearance                           |
| ----------------- | -------------------- | --------------------------------------------------------------------------------------------------------- | ----------------------------------------- |
| Not done (today)  | Empty circle / muted | Shows current streak (may be 0)                                                                           | Default surface color                     |
| Done (today)      | Green check circle   | Shows updated streak count with 👍 (1–2 days), 👏 (3–5 days), 😎 (6–9 days), 🔥 (10–29 days), or 🐐 (30+ days) | Subtle success tint or left border accent |
| Streak broken (0) | Empty circle / muted | Shows "0" in muted text (no emoji)                                                                        | Default surface color                     |

## Streak Counter Display

- The streak count is displayed in the secondary line below the habit name:
  - **🐐** (goat) when the streak is **30 or more** consecutive days.
  - **🔥** (fire) when the streak is **10–29** consecutive days.
  - **😎** (cool) when the streak is **6–9** consecutive days.
  - **👏** (clapping) when the streak is **3–5** consecutive days.
  - **👍** (thumbs up) when the streak is **1–2** consecutive days.
  - No emoji when the streak is **0** (just `0 streak`).
- The streak count is part of the tappable habit card and opens the [Habit Details page](./habit-details-page.md) for that specific habit.

## Empty State

When the user has no habits configured:

- Show a centered `+ New Habit` button that opens the [Quick Add Habit dialog](./create-habit-page.md).
- Prefer a centered `MudStack` with a `MudFab` or `MudButton`.

## Interaction Summary

| Action                                | Result                                                                         |
| ------------------------------------- | ------------------------------------------------------------------------------ |
| Toggle an unchecked habit to done     | Opens the check-in dialog; completing it persists today's checkin              |
| Cancel or dismiss the check-in dialog | Closes the dialog and leaves the habit unchecked                               |
| Toggle a checked habit to not done    | Opens a confirmation dialog before deleting today's checkin                    |
| Confirm **Remove check-in**           | Deletes today's checkin and any saved note/proof details; updates streak count |
| Tap a habit card (except toggle)      | Navigates to the Habit Details page for that habit                             |
| Tap **+ New Habit** on homepage       | Opens the Quick Add Habit dialog over the homepage                             |
| Tap **⚙** in app bar                  | Navigates to the Settings page                                                 |
| Tap GitHub icon in app bar            | Opens the public GitHub repository                                             |
| Tap **Streak** logo                   | No-op (already on the homepage)                                                |
