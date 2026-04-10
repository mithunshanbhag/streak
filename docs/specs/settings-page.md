# Settings Page

> **Route**: `/settings`

The settings page lets users configure **daily reminders** and access low-frequency **data management** actions such as exporting the local database. Users access it from the **⚙** icon in the app bar.

## Navigation

- Accessible from the **⚙** icon in the Homepage app bar.
- A **back arrow** in the app bar returns the user to the [Homepage](./homepage.md).
- Secondary-screen chrome stays focused: show **Back** + `Settings` only.
- Export remains inside the page content rather than becoming a dedicated app-bar icon.

## Layout

The page contains two vertically stacked sections presented as clean cards:

1. **Daily Reminder**
2. **Data**

- Prefer `MudCard` or `MudPaper` plus built-in spacing utilities.
- Prefer `MudSwitch`, `MudTimePicker`, `MudButton`, and `MudText` rather than custom control treatments.
- No custom page-specific CSS should be required beyond ordinary spacing or width adjustments.

### Daily Reminder Section

| Element               | Type            | Details                                                                                                                                   |
| --------------------- | --------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| Section header        | Text            | **"Daily Reminder"**                                                                                                                      |
| Enable/disable toggle | `MudSwitch`     | ON = reminders enabled, OFF = reminders disabled. Default: **ON**.                                                                        |
| Reminder time picker  | `MudTimePicker` | Allows the user to select the time of day for the reminder. Visible only when the toggle is ON. Default: **9:00 PM** (local device time). |
| Helper text           | Caption         | *"You'll be reminded only if there are habits you haven't checked in yet."*                                                               |

### Data Section

| Element         | Type        | Details                                                                                            |
| --------------- | ----------- | -------------------------------------------------------------------------------------------------- |
| Section header  | Text        | **"Data"**                                                                                         |
| Export action   | `MudButton` | Primary action labeled **"Export Database"**. Starts the database export flow.                     |
| Helper text     | Caption     | Explains that the export creates a backup of the user's local Streak data for saving or sharing.   |
| Optional status | Caption     | May show lightweight metadata such as the last export time when that becomes available in the app. |

## Export Behavior

- Tapping **Export Database** creates a backup of the app's local database and then hands it off to the platform's standard save / share flow.
- Export is a **manual** action; it does not run automatically.
- The export action does **not** modify habits, checkins, or reminder settings.
- The exported backup should include the user's habit data plus reminder preferences stored in the local database.
- Export is considered a low-frequency maintenance / safety action, so it lives in **Settings** rather than in the Homepage app bar.

## Reminder Behavior

- When enabled, the app schedules a **local notification** at the configured time each day.
- The notification fires **only if** at least one habit has not been checked in as done for that day.
- If all habits are already marked done before the reminder time, **no notification is sent**.
- The notification content should include:
  - Title: *"Streak Reminder"*
  - Body: *"You have {N} habit(s) pending today."* (where N is the count of unchecked habits).
- Tapping the notification opens the app to the [Homepage](./homepage.md).

## Defaults

| Setting          | Default Value               |
| ---------------- | --------------------------- |
| Reminder enabled | ON                          |
| Reminder time    | 9:00 PM (local device time) |

## Persistence

- Settings are saved to **local SQLite** immediately when changed (no explicit "Save" button).
- Settings persist across app restarts.
- Export creates a backup on demand; it is not auto-saved in the background.

## Edge Cases

| Scenario                       | Behavior                                                                                                                          |
| ------------------------------ | --------------------------------------------------------------------------------------------------------------------------------- |
| User has no habits             | Reminder toggle is still available but no notification will fire (0 pending habits).                                              |
| User has no habits but exports | Export is still allowed so the user can back up reminder settings or an empty database state.                                     |
| User disables reminders        | No notifications are scheduled. The time picker is hidden.                                                                        |
| User changes time              | The next reminder is rescheduled to the new time. If the new time has already passed for today, the next reminder fires tomorrow. |
| App is force-closed            | Reminders should still fire (use Android's alarm/notification scheduling APIs that persist beyond app lifecycle).                 |
| Export fails                   | Keep the user on Settings and surface a clear error message rather than silently failing.                                         |
