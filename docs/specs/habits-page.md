# Habits Page (Landing / Checkin)

> **Routes**: `/` (default landing page), `/habits`

The habits page is the **primary surface** of the Streak app. Users should be able to open the app, check in their habits for the day, and leave — all without navigating anywhere.

## Layout

The page displays a **vertical list of habit cards**, one per habit.

- Habits are sorted **alphabetically by name**.
- There is no separate manage-habits screen; the Habits page is the main habit list surface.

### Habit Card

Each card shows:

| Element        | Position     | Details                                                                                                                                          |
| -------------- | ------------ | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| Emoji / Icon   | Left         | The habit's emoji, or a default icon if none is set.                                                                                                 |
| Habit name     | Center-left  | The habit's name label. Tapping the card body opens the [Habit Details page](./habit-details-page.md) for that habit.                                     |
| Current streak | Center-right | A badge or label showing the streak count (e.g., "😎 3" or "🔥 12"). It remains visible within the tappable card body and opens the same Habit Details page. |
| Checkin toggle | Right        | A `MudToggleIconButton`. Done = green check circle icon. Not done = empty circle icon.                                                               |

- Cards use the full width of the screen with standard horizontal padding (16dp).
- Vertical spacing between cards: 12dp.
- Tapping anywhere on a habit card **except** the checkin toggle navigates to that habit's [Habit Details page](./habit-details-page.md).

## Checkin Toggle Behavior

- **Default state**: Empty circle (not done) at the start of each new calendar day.
- **Toggling to done**: Marks the habit as done for today. The icon changes to a green check circle. The streak count increments immediately (visually).
- **Toggling to not done**: Un-marks the habit for today. The icon changes back to an empty circle. The streak count updates accordingly.
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

- The streak count is displayed as a number next to an emoji:
  - **🔥** (fire) when the streak is **7 or more** consecutive days.
  - **😎** (cool) when the streak is **1–6** consecutive days.
  - No emoji when the streak is **0** (just muted text).
- The streak count is part of the tappable habit card and opens the [Habit Details page](./habit-details-page.md) for that specific habit.

## Empty State

When the user has no habits configured:

- Display a friendly message: *"No habits yet. Tap **+** to add one."*
- Show a 🌱 emoji or illustration above the message.
- Optionally include a CTA button ("Add Habit") that navigates to the [Create Habit page](./create-habit-page.md).

## Interaction Summary

| Action                           | Result                                                             |
| -------------------------------- | ------------------------------------------------------------------ |
| Toggle a habit's check icon      | Immediately records checkin for today; updates streak count        |
| Tap a habit card (except toggle) | Navigates to the Habit Details page for that habit                 |
| Tap **+** in app bar             | Navigates to the Create Habit page                                 |
| Tap **⚙** in app bar             | Navigates to the Settings page                                     |
| Tap **Streak** logo              | No-op (already on the Habits page)                                 |
