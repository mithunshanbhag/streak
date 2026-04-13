# Settings Page

> **Route**: `/settings`

The settings page lets users configure **daily reminders** and access low-frequency **data management** actions such as exporting and importing the local database. Users access it from the **⚙** icon in the app bar.

## Navigation

- Accessible from the **⚙** icon in the Homepage app bar.
- A **back arrow** in the app bar returns the user to the [Homepage](./homepage.md).
- Secondary-screen chrome stays focused: show **Back** + `Settings` only.
- Export and Import remain inside the page content rather than becoming dedicated app-bar icons.

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

| Element             | Type                | Details                                                                                                                    |
| ------------------- | ------------------- | -------------------------------------------------------------------------------------------------------------------------- |
| Section eyebrow     | Text                | **"Data"**                                                                                                                 |
| Backup header       | Text                | **"Backup"**                                                                                                               |
| Backup info icon    | Glyph + tooltip     | Small info icon beside **Backup**. Hover/focus/press shows: *"Android saves to 'Downloads' folder. Windows lets you choose where to save."* |
| Section description | Caption             | *"Save a copy of your local data."*                                                                                        |
| Export action       | `MudButton`         | Filled action button labeled **"Download DB"** with a download icon. Starts the database export flow.                      |
| Divider             | Visual              | A horizontal rule separating the Backup and Restore sub-sections.                                                          |
| Restore header      | Text                | **"Restore"**                                                                                                              |
| Restore warning icon| Glyph + tooltip     | Small warning icon beside **Restore**. Hover/focus/press shows: *"This will replace ALL existing data. This action cannot be undone."* |
| Section description | Caption             | *"Restore your data from a previous backup."*                                                                              |
| Import action       | `MudButton`         | Filled action button labeled **"Upload DB"** with an upload icon. Opens a file picker to select a `.db` backup file.      |

- Backup and Restore should use the same subsection layout and spacing so they read as sibling actions within the same card.
- The tooltip trigger icons should be visually subtle but clearly interactive, with the warning icon using a caution color treatment.
- Do not show the backup/help text or restore warning as always-visible inline callouts inside the card body.
- The Backup and Restore action buttons should use the same font size, weight, foreground color, and filled background treatment so they feel like a matched pair.

## Export Behavior

- Tapping **Download DB** creates a backup of the app's local database and then saves it using a platform-specific file flow.
- Export is a **manual** action; it does not run automatically.
- The export action does **not** modify habits, checkins, or reminder settings.
- The exported backup should include the user's habit data plus reminder preferences stored in the local database.
- Export is considered a low-frequency maintenance / safety action, so it lives in **Settings** rather than in the Homepage app bar.
- The exported filename should use a timestamped pattern such as `streak-backup-YYYYMMdd-HHmmss.db`.
- The platform-specific save note is exposed from the **Backup** info tooltip rather than as persistent inline helper text.

### Platform-specific Export UX

| Platform | Expected behavior                                                                                                                                              |
| -------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Windows  | Open a standard **Save As** file dialog prefilled with the timestamped backup filename. The user chooses where to save the `.db` file and confirms the dialog. |
| Android  | Save the timestamped backup file directly into the device's **Downloads** folder. No share sheet should be shown for the normal export flow.                   |

- Do **not** use the operating system share sheet as the primary export UX on either platform.
- On Windows, cancelling the file-save dialog is treated as a user cancellation, not as an export error.
- On Android, a successful export should leave the file available in **Downloads** so the user can manage it with the system file manager or share it later if they choose.

## Import Behavior

- Tapping **Upload DB** opens a platform-native file picker scoped to `.db` files.
- Once a valid backup file is selected, the user is shown a **confirmation dialog** warning that all existing data will be replaced.
- If the user confirms, the app closes the current database connection, replaces the live database file with the selected backup, then reopens the connection.
- Import is a **manual** action; it does not run automatically.
- After a successful import the app **navigates to the Homepage** so the user sees freshly loaded data.
- On failure the app rolls back to the previous database state and surfaces a clear error message; the user remains on Settings.
- Import is considered a destructive, low-frequency action so it lives in **Settings** with a prominent warning.
- The destructive warning should be available from the **Restore** warning tooltip in the resting page layout, then repeated in the confirmation dialog before import proceeds.

### Platform-specific Import UX

| Platform | Expected behavior                                                                                                                     |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| Windows  | Open a standard **Open File** dialog filtered to `.db` files. The user selects a backup file and confirms the dialog.                 |
| Android  | Open the system file picker filtered to `.db` files (via `StorageAccessFramework` / intent). The user selects a backup file.          |

- On both platforms, cancelling the file-picker dialog is treated as a user cancellation, not as an import error.
- The app must validate that the selected file is a recognizable Streak backup before overwriting the live database.



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
- Import replaces the live database on demand; the previous data is permanently overwritten after user confirmation.
- Exported backup files are stored outside the live app database location:
  - On **Windows**, wherever the user selects in the file-save dialog.
  - On **Android**, in the device's **Downloads** folder.

## Edge Cases

| Scenario                         | Behavior                                                                                                                          |
| -------------------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| User has no habits               | Reminder toggle is still available but no notification will fire (0 pending habits).                                              |
| User has no habits but exports   | Export is still allowed so the user can back up reminder settings or an empty database state.                                     |
| User disables reminders          | No notifications are scheduled. The time picker is hidden.                                                                        |
| User changes time                | The next reminder is rescheduled to the new time. If the new time has already passed for today, the next reminder fires tomorrow. |
| App is force-closed              | Reminders should still fire (use Android's alarm/notification scheduling APIs that persist beyond app lifecycle).                 |
| User cancels Windows save dialog | Keep the user on Settings and treat the action as cancelled rather than failed.                                                   |
| Android export succeeds          | The backup file appears in **Downloads** with the generated timestamped filename.                                                 |
| Export fails                     | Keep the user on Settings and surface a clear error message rather than silently failing.                                         |
| User cancels import file picker  | Keep the user on Settings and treat the action as cancelled rather than failed.                                                   |
| User selects invalid/corrupt file | Keep the user on Settings and surface a clear error message; do not overwrite the live database.                                 |
| Import succeeds                  | Replace the live database with the backup, then navigate the user to the Homepage with freshly loaded data.                       |
| Import fails mid-way             | Roll back to the previous database state, keep the user on Settings, and surface a clear error message.                          |
