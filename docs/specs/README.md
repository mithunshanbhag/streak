# Streak: Keep That Habit Alive

> *Let your habits compound.*

Streak is a simple habit-tracking mobile app for Android. It helps users build and maintain daily habits by making it effortless to check in each day and watch their streaks grow over time.

## Design Philosophy

- **Speed over features.** The daily workflow (check in → exit) should take seconds, not minutes.
- **Minimal navigation.** The landing page is the primary surface; most tasks require zero page clicks.
- **Simplicity over completeness.** Binary habits (done / not done), a hard cap of 10 habits, and local-only data storage.

## Core Concepts

### Habit

A recurring daily activity the user wants to track.

| Property     | Required | Details                                                                                                  |
| ------------ | -------- | -------------------------------------------------------------------------------------------------------- |
| Name         | Yes      | Short descriptive label (e.g., "Meditate", "Read"). Max 30 characters.                                   |
| Emoji / Icon | No       | A single emoji to visually represent the habit (e.g., 🧘, 📖). A default icon is used if none is selected. |

- A user can have between 0 and **10** habits at any time.
- On the [Homepage](./homepage.md), habits are listed in **alphabetical order by name**.
- Tapping a habit on the Homepage opens that habit's [Habit Details page](./habit-details-page.md).

### Checkin

A daily record that exists only when a habit is marked **done** for a given calendar day.

- Checkins are **binary**: a record exists when done, and no record exists when not done. There is no partial completion or quantity tracking.
- Checkins apply to **today only**. Backdating (marking a past day) is not supported.
- A habit that has no checkin recorded for a day is treated as **not done**.
- If the user unchecks a same-day habit, that day's checkin record is deleted.
- There are two ways a checkin happens:
  1. **Voluntary**: the user taps the toggle on the homepage as soon as they complete the activity.
  2. **Via reminder**: at a user-configured time each day, the app sends a notification prompting the user to check in on any habits that are still pending (not yet marked done).

### Streak

A streak is the count of **consecutive calendar days** on which a habit was checked in as done, ending with today (or the most recent completed day).

- A streak starts at **1** on the first day a habit is marked done.
- The streak increments by 1 for each subsequent consecutive day the habit is marked done.
- The streak **resets to 0** if the user does not check in for a day (i.e., the day passes without a "done" checkin).
- A newly created habit starts with a streak of **0**.

## Data Storage

- All data is stored **locally on the device**. There is no cloud sync, no user accounts, and no authentication.
- Data will be persisted across app restarts.

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

## Surface and Route Inventory

Each major surface has its own detailed spec:

| Surface         | Route / Trigger           | Spec                                             | Purpose                                                             |
| --------------- | ------------------------- | ------------------------------------------------ | ------------------------------------------------------------------- |
| Homepage        | `/`                       | [homepage.md](./homepage.md)                     | Landing page, daily checkin surface, and habit list                 |
| Habit Details   | `/habits/{habitId}`       | [habit-details-page.md](./habit-details-page.md) | Habit details, trends, edit dialog, and deletion                    |
| Quick Add Habit | `+ New Habit` on Homepage | [create-habit-page.md](./create-habit-page.md)   | Create a new habit in a compact dialog without leaving the homepage |
| Settings        | `/settings`               | [settings-page.md](./settings-page.md)           | Configure reminders, manage exports                                 |

## Information Architecture Notes

- The app remains **shallow by default**: **Homepage** is the landing page, with **Habit Details** and **Settings** as the primary secondary destinations.
- The Homepage includes a dedicated **`+ New Habit`** CTA below the habit list, which opens a compact **Quick Add Habit** dialog over **Homepage**.
- The **Homepage** doubles as the habit-list maintenance surface: habits are shown alphabetically and each habit opens its details on the Habit Details page.
- The Homepage app bar keeps **Settings** plus a right-most **GitHub** repo link instead of a global create icon.
- The **Habit Details** page contains the heatmap, edit dialog flow, and delete confirmation dialog for a single habit.
- **Settings** groups reminder preferences plus low-frequency data actions such as **Export Database**.
- **Homepage** opens directly into the habit list without instructional header copy, progress summary text, or a habit-count chip.
- There is no dedicated habit-list routed page separate from **Homepage**.
- There is no dedicated routed **Create Habit** page in the simplified direction.
- Separate **Edit Habit** and **Delete Habit** routed pages are no longer part of the app structure.

Common UI specifications (theme, typography, iconography, navigation):

- [ui.md](./ui.md)

## UX Exploration

- [ux-simplification-review.md](./ux-simplification-review.md) - accepted and rejected simplification ideas, plus links to the updated primary mockups.
