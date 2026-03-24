# Homepage (Landing / Check-in)

> **Route**: `/`

The homepage is the **primary surface** of the Streak app. Users should be able to open the app, check in their habits for the day, and leave — all without navigating anywhere.

## Layout

The page displays a **vertical list of habit cards**, one per habit.

- Habits are sorted **alphabetically by name**.
- There is no separate manage-habits screen; the homepage is the main habit list surface.
- Prefer a `MudContainer` + `MudStack` composition with built-in spacing utilities rather than custom page layout CSS.

### List Start

- The page should open directly into the habit list with no separate progress summary section above it.
- Do **not** show a `Today` status line, a remaining-habits message such as `2 left today`, or a total-habit-count chip on this screen.
- Do **not** show an A-Z sort chip or explanatory helper text on this screen.

### Habit Card

Each card shows:

| Element        | Position            | Details                                                                                                                                             |
| -------------- | ------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| Emoji / Icon   | Left                | The habit's emoji, or a default icon if none is set.                                                                                               |
| Habit name     | Main text column    | The habit's name label. Tapping the card body opens the [Habit Details page](./habit-details-page.md) for that habit.                             |
| Current streak | Secondary text line | A compact secondary line under the habit name showing the streak (for example, `😎 3 day streak`, `🔥 12 day streak`, or `0 streak`).             |
| Checkin toggle | Right               | A `MudCheckBox` configured with custom icons. Done = green check circle icon. Not done = empty circle icon.                                       |

- Cards use the full width of the screen with standard horizontal padding (16dp).
- Vertical spacing between cards: 12dp.
- Tapping anywhere on a habit card **except** the checkin toggle navigates to that habit's [Habit Details page](./habit-details-page.md).
- Prefer `MudCard` or `MudPaper` plus `MudStack`, `MudText`, and utility classes before introducing any custom card-specific CSS.

### New Habit CTA

- When the user already has habits and is still below the 6-habit limit, show a full-width **`+ New Habit`** button below the list.
- Separate the list from this CTA with a subtle horizontal divider.
- Tapping **`+ New Habit`** opens the [Quick Add Habit dialog](./create-habit-page.md) over the homepage.
- Hide the CTA when the user is already at the maximum habit count.

## Checkin Toggle Behavior

- **Default state**: Empty circle (not done) at the start of each new calendar day.
- **Toggling to done**: Creates today's checkin record. The icon changes to a green check circle. The streak count increments immediately (visually).
- **Toggling to not done**: Deletes today's checkin record. The icon changes back to an empty circle. The streak count updates accordingly.
- After each toggle, the habit remains in the same alphabetical position; checking in does **not** change the list order.
- Toggling is **instant** — no confirmation dialog, no loading state.
- The checkin is persisted to local storage immediately on toggle.

## Visual States

| State             | Toggle               | Streak Badge                                                | Card Appearance                           |
| ----------------- | -------------------- | ----------------------------------------------------------- | ----------------------------------------- |
| Not done (today)  | Empty circle / muted | Shows current streak (may be 0)                             | Default surface color                     |
| Done (today)      | Green check circle   | Shows updated streak count with 😎 (1–6 days) or 🔥 (7+ days) | Subtle success tint or left border accent |
| Streak broken (0) | Empty circle / muted | Shows "0" in muted text (no emoji)                          | Default surface color                     |

## Streak Counter Display

- The streak count is displayed in the secondary line below the habit name:
  - **🔥** (fire) when the streak is **7 or more** consecutive days.
  - **😎** (cool) when the streak is **1–6** consecutive days.
  - No emoji when the streak is **0** (just muted text such as `0 streak`).
- The streak count is part of the tappable habit card and opens the [Habit Details page](./habit-details-page.md) for that specific habit.

## Empty State

When the user has no habits configured:

- Display a friendly message: *"No habits yet. Tap **New Habit** to add one."*
- Show a 🌱 emoji or illustration above the message.
- Include a CTA button (`+ New Habit`) that opens the [Quick Add Habit dialog](./create-habit-page.md).
- Prefer a centered `MudStack` with `MudText` and `MudButton`.

## Interaction Summary

| Action                           | Result                                                             |
| -------------------------------- | ------------------------------------------------------------------ |
| Toggle a habit's check icon      | Immediately records checkin for today; updates streak count        |
| Tap a habit card (except toggle) | Navigates to the Habit Details page for that habit                 |
| Tap **+ New Habit** on homepage  | Opens the Quick Add Habit dialog over the homepage                 |
| Tap **⚙** in app bar             | Navigates to the Settings page                                     |
| Tap GitHub icon in app bar       | Opens the public GitHub repository                                 |
| Tap **Streak** logo              | No-op (already on the homepage)                                    |
