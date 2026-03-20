# Streak: Keep That Habit Alive

> *Let your habits compound.*

Streak is a simple habit-tracking mobile app for Android. It helps users build and maintain daily habits by making it effortless to check in each day and watch their streaks grow over time.

## Design Philosophy

- **Speed over features.** The daily workflow (check in → exit) should take seconds, not minutes.
- **Minimal navigation.** The landing page is the primary surface; most tasks require zero page clicks.
- **Simplicity over completeness.** Binary habits (done / not done), a hard cap of 6 habits, and local-only data storage.

## Core Concepts

### Habit

A recurring daily activity the user wants to track.

| Property     | Required | Details                                                                                                  |
| ------------ | -------- | -------------------------------------------------------------------------------------------------------- |
| Name         | Yes      | Short descriptive label (e.g., "Meditate", "Read"). Max 30 characters.                                   |
| Emoji / Icon | No       | A single emoji to visually represent the habit (e.g., 🧘, 📖). A default icon is used if none is selected. |

- A user can have between 0 and **6** habits at any time.
- Habits are displayed in a user-defined order on the home page.

### Checkin

A daily record that marks a habit as **done** or **not done** for a given calendar day.

- Checkins are **binary**: done or not done. There is no partial completion or quantity tracking.
- Checkins apply to **today only**. Backdating (marking a past day) is not supported.
- A habit that has no checkin recorded for a day is treated as **not done**.
- There are two ways a checkin happens:
  1. **Voluntary**: the user taps the toggle on the home page as soon as they complete the activity.
  2. **Via reminder**: at a user-configured time each day, the app sends a notification prompting the user to check in on any habits that are still pending (not yet marked done).

### Streak

A streak is the count of **consecutive calendar days** on which a habit was checked in as done, ending with today (or the most recent completed day).

- A streak starts at **1** on the first day a habit is marked done.
- The streak increments by 1 for each subsequent consecutive day the habit is marked done.
- The streak **resets to 0** if the user does not check in for a day (i.e., the day passes without a "done" checkin).
- A newly created habit starts with a streak of **0**.

## Data Storage

- All data is stored **locally on the device**. There is no cloud sync, no user accounts, and no authentication.
- Data must persist across app restarts.

## Notifications and Reminders

- The user can configure a **daily reminder time** (e.g., 9:00 PM) in settings.
- The reminder fires **only if** there is at least one habit that has not been checked in as done for the day.
- If all habits are already checked in, no reminder is sent.
- The reminder should include a summary (e.g., "You have 2 habits pending today").
- The default reminder time is **9:00 PM** (local device time).
- Reminders can be disabled entirely by the user.

## Non-Functional Requirements

- The app should launch and be ready for interaction within **2 seconds**.
- Checkin toggling should feel **instant** (no loading spinners or delays).
- The app should work fully **offline** (local data only).
- Minimal battery and storage footprint.

## Page and Route Inventory

Each routed page has its own detailed spec:

| Page                         | Route                               | Spec                                                         | Purpose                                               |
| ---------------------------- | ----------------------------------- | ------------------------------------------------------------ | ----------------------------------------------------- |
| Home                         | `/`                                 | [home-page.md](./home-page.md)                               | Daily checkin surface (landing page)                  |
| Trends                       | `/trends/{habitId}`                 | [trends-page.md](./trends-page.md)                           | Habit streak heatmap and streak counter               |
| Manage Habits                | `/manage-habits`                    | [manage-habits-page.md](./manage-habits-page.md)             | Habit list, reorder, and maintenance actions          |
| Create Habit                 | `/manage-habits/new`                | [create-habit-page.md](./create-habit-page.md)               | Create a new habit from the global add action or manage habits flow |
| Edit Habit                   | `/manage-habits/{habitId}/edit`     | [edit-habit-page.md](./edit-habit-page.md)                   | Update an existing habit without changing its history |
| Delete Habit Confirmation    | `/manage-habits/{habitId}/delete`   | [delete-habit-page.md](./delete-habit-page.md)               | Confirm destructive habit deletion                    |
| Settings                     | `/settings`                         | [settings-page.md](./settings-page.md)                       | Configure daily reminder time                         |

## Information Architecture Notes

- The app remains **shallow by default**: Home is the landing page, with Trends, Manage Habits, and Settings as the primary secondary destinations.
- The global **+** app-bar action opens **Create Habit** directly.
- **Manage Habits** is a lower-frequency maintenance surface reached from the app bar's **More** menu.
- Habit creation, editing, and deletion are part of the **Manage Habits route hierarchy**, rather than separate top-level areas.
- The previous add, edit, and delete dialogs are now represented as **regular routed pages** so they can support direct navigation, browser history, and breadcrumbs without changing the overall structure of the app.

Common UI specifications (theme, typography, iconography, navigation):

- [ui.md](./ui.md)
