# Settings Page

> **Route**: `/settings`

The settings page allows users to configure their **daily reminder** preferences. Users access this page by tapping the **⚙** (gear) icon in the app bar.

## Navigation

- Accessible from the **⚙** icon in the app bar (available on all pages).
- A **back arrow** in the app bar returns the user to the [Home page](./home-page.md).

## Layout

The page contains a single settings section for reminders.

### Daily Reminder Section

| Element               | Type            | Details                                                                                                                                   |
| --------------------- | --------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| Section header        | Text            | **"Daily Reminder"**                                                                                                                      |
| Enable/disable toggle | `MudSwitch`     | ON = reminders enabled, OFF = reminders disabled. Default: **ON**.                                                                        |
| Reminder time picker  | `MudTimePicker` | Allows the user to select the time of day for the reminder. Visible only when the toggle is ON. Default: **9:00 PM** (local device time). |
| Helper text           | Caption         | *"You'll be reminded only if there are habits you haven't checked in yet."*                                                               |

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

## Edge Cases

| Scenario                | Behavior                                                                                                                          |
| ----------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| User has no habits      | Reminder toggle is still available but no notification will fire (0 pending habits).                                              |
| User disables reminders | No notifications are scheduled. The time picker is hidden.                                                                        |
| User changes time       | The next reminder is rescheduled to the new time. If the new time has already passed for today, the next reminder fires tomorrow. |
| App is force-closed     | Reminders should still fire (use Android's alarm/notification scheduling APIs that persist beyond app lifecycle).                 |
