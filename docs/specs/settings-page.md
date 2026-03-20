# Settings Page

> **Route**: `/settings`

The settings page allows users to configure their **daily reminder** preferences and manage their full **habit list** from one place. Users access this page by tapping the **⚙** (gear) icon in the app bar.

## Navigation

- Accessible from the **⚙** icon in the app bar (available on all pages).
- A **back arrow** in the app bar returns the user to the [Home page](./home-page.md).

## Layout

The page contains two vertically stacked sections: one for reminders and one for habit management.

### Daily Reminder Section

| Element               | Type            | Details                                                                                                                                   |
| --------------------- | --------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| Section header        | Text            | **"Daily Reminder"**                                                                                                                      |
| Enable/disable toggle | `MudSwitch`     | ON = reminders enabled, OFF = reminders disabled. Default: **ON**.                                                                        |
| Reminder time picker  | `MudTimePicker` | Allows the user to select the time of day for the reminder. Visible only when the toggle is ON. Default: **9:00 PM** (local device time). |
| Helper text           | Caption         | *"You'll be reminded only if there are habits you haven't checked in yet."*                                                               |

### Habit Management Section

| Element                | Type                     | Details                                                                                                                                               |
| ---------------------- | ------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| Section header         | Text                     | **"Habits"**                                                                                                                                          |
| Section description    | Caption                  | Brief copy explaining that this is where the user reorders the home-page list and maintains existing habits.                                        |
| Habit list             | Reorderable list / card  | Shows each habit with drag handle, emoji/icon, name, edit action, and delete action.                                                                |
| Add habit button       | `MudButton`              | **"+ Add Habit"** button shown below the list. Disabled when the user already has 6 habits.                                                         |
| Empty state            | Inline empty state       | When there are no habits, show a 🌱 emoji, a short message, and the same **"+ Add Habit"** button.                                                  |

#### Habit List Behavior

- Users can **drag and drop** habits in the list to change their display order on the [Home page](./home-page.md).
- The new order is persisted immediately.
- Tapping the **edit** icon opens the [Edit Habit page](./edit-habit-page.md).
- Tapping the **delete** icon opens the [Delete Habit Confirmation page](./delete-habit-page.md).
- Tapping **"+ Add Habit"** opens the [Create Habit page](./create-habit-page.md).

## Child Pages

The habit CRUD flows remain dedicated routed pages, but they now live under the settings hierarchy:

| Page                                                        | Route                               | Purpose                                               |
| ----------------------------------------------------------- | ----------------------------------- | ----------------------------------------------------- |
| [Create Habit](./create-habit-page.md)                      | `/settings/habits/new`              | Create a new habit                                    |
| [Edit Habit](./edit-habit-page.md)                          | `/settings/habits/{habitId}/edit`   | Edit an existing habit without affecting its history |
| [Delete Habit Confirmation](./delete-habit-page.md)         | `/settings/habits/{habitId}/delete` | Confirm destructive deletion                          |

## Reminder Behavior

- When enabled, the app schedules a **local notification** at the configured time each day.
- The notification fires **only if** at least one habit has not been checked in as done for that day.
- If all habits are already marked done before the reminder time, **no notification is sent**.
- The notification content should include:
  - Title: *"Streak Reminder"*
  - Body: *"You have {N} habit(s) pending today."* (where N is the count of unchecked habits).
- Tapping the notification opens the app to the [Home page](./home-page.md).

## Defaults

| Setting          | Default Value               |
| ---------------- | --------------------------- |
| Reminder enabled | ON                          |
| Reminder time    | 9:00 PM (local device time) |

## Persistence

- Settings are saved to **local SQLite** immediately when changed (no explicit "Save" button).
- Settings persist across app restarts.
- Habit reorder changes are saved immediately.

## Edge Cases

| Scenario                | Behavior                                                                                                                          |
| ----------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| User has no habits      | Reminder toggle is still available but no notification will fire (0 pending habits).                                              |
| User disables reminders | No notifications are scheduled. The time picker is hidden.                                                                        |
| User changes time       | The next reminder is rescheduled to the new time. If the new time has already passed for today, the next reminder fires tomorrow. |
| App is force-closed     | Reminders should still fire (use Android's alarm/notification scheduling APIs that persist beyond app lifecycle).                 |
| User reaches 6 habits   | The **"+ Add Habit"** button is disabled and explains that the maximum has been reached.                                          |
