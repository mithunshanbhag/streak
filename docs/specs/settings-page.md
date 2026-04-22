# Settings Page

> **Route**: `/settings`

The settings page lets users configure **daily reminders** and access low-frequency **data management** actions such as daily automated backups, downloading, sharing, and restoring full data-backup archives, plus exporting diagnostic logs. Users access it from the **⚙** icon in the app bar.

## Navigation

- Accessible from the **⚙** icon in the Homepage app bar.
- A **back arrow** in the app bar returns the user to the [Homepage](./homepage.md).
- Secondary-screen chrome stays focused: show **Back** + `Settings` only.
- Daily automated backups, backup download, backup share, diagnostic export, and restore remain inside the page content rather than becoming dedicated app-bar icons.

## Layout

The page contains two vertically stacked sections presented as clean cards:

1. **Daily Reminder**
2. **Data**

- Prefer `MudCard` or `MudPaper` plus built-in spacing utilities.
- Prefer `MudSwitch`, `MudTimePicker`, `MudIconButton`, `MudTooltip`, and `MudText` rather than custom control treatments.
- No custom page-specific CSS should be required beyond ordinary spacing or width adjustments.

### Daily Reminder Section

| Element               | Type            | Details                                                                                                                                      |
| --------------------- | --------------- | -------------------------------------------------------------------------------------------------------------------------------------------- |
| Section header        | Text            | **"Daily Reminder"**                                                                                                                         |
| Enable/disable toggle | `MudSwitch`     | ON = reminders enabled, OFF = reminders disabled. Default: **OFF**. Enabling reminders requests Android notification permission when needed. |
| Reminder time picker  | `MudTimePicker` | Allows the user to select the time of day for the reminder. Visible only when the toggle is ON. Default: **9:00 PM** (local device time).    |
| Helper text           | Caption         | *"You'll be reminded only if there are habits you haven't checked in yet."*                                                                  |

### Data Section

| Element                       | Type                  | Details                                                                                                                                                                                                                                                      |
| ----------------------------- | --------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Section eyebrow               | Text                  | **"Data"**                                                                                                                                                                                                                                                   |
| Automated backups header      | Text                  | **"Daily automated backups"**                                                                                                                                                                                                                                |
| Automated backups info icon   | Glyph + tooltip       | Small info icon beside **Daily automated backups**. Hover/focus/press shows: *"Android only. Runs nightly at 11:30 PM when enabled."*                                                                                                                        |
| Automated backups description | Caption               | *"Create a nightly backup in local storage."*                                                                                                                                                                                                                |
| Enable/disable toggle         | `MudSwitch`           | ON = daily automated backups enabled, OFF = disabled. Default: **OFF**. The switch sits on its own trailing action row with no extra inline label or helper copy. The control is disabled on Windows because this feature is Android-only in this iteration. |
| Dividers                      | Visual                | Simple horizontal rules separate **Daily automated backups**, **Backup**, **Diagnostic logs**, and **Restore** so the four subsections read as one stacked group.                                                                                            |
| Backup header                 | Text                  | **"Backup"**                                                                                                                                                                                                                                                 |
| Backup info icon              | Glyph + tooltip       | Small info icon beside **Backup**. Hover/focus/press shows: *"Creates a '.zip' data archive with your local data and uploaded pictures. Android saves to 'Downloads/Streak'. Windows lets you choose where to save."*                                        |
| Backup description            | Caption               | *"Save or share a copy of your local data."*                                                                                                                                                                                                                 |
| Backup action cluster         | `MudIconButton` group | Two filled icon-only buttons shown side-by-side: download and share. Tooltips provide the visible labels **"Download data"** and **"Share data"**.                                                                                                           |
| Diagnostic logs header        | Text                  | **"Diagnostic logs"**                                                                                                                                                                                                                                        |
| Diagnostic logs info icon     | Glyph + tooltip       | Small info icon beside **Diagnostic logs**. Hover/focus/press shows: *"Exports recent app logs and basic support metadata. Does not include your full database."*                                                                                            |
| Diagnostic logs description   | Caption               | *"Export a support bundle of recent app logs."*                                                                                                                                                                                                              |
| Diagnostic export action      | `MudIconButton`       | Filled icon-only button with a download icon. Tooltip text is **"Export logs"**. Exports a `.zip` diagnostics bundle using the platform-specific save flow.                                                                                                  |
| Restore header                | Text                  | **"Restore"**                                                                                                                                                                                                                                                |
| Restore warning icon          | Glyph + tooltip       | Small warning icon beside **Restore**. Hover/focus/press shows: *"This will replace ALL existing data. This action cannot be undone."*                                                                                                                       |
| Restore description           | Caption               | *"Restore your data from a previous backup."*                                                                                                                                                                                                                |
| Restore action                | `MudIconButton`       | Filled icon-only button with an upload icon. Tooltip text is **"Upload data"**. Opens a file picker to select either a `.zip` data-backup archive or a legacy `.db` database backup.                                                                         |

- Daily automated backups, Backup, Diagnostic logs, and Restore should read as four vertically stacked subsections within the same card.
- The automated backups subsection should use the same quiet structure as Backup, Diagnostic logs, and Restore: heading, subtle tooltip icon, one short description line, then a single trailing control row.
- Backup, Diagnostic logs, and Restore should use the same subsection layout and spacing so they read as sibling manual actions within the same card.
- The backup action cluster should place **Share data** immediately next to **Download data** with the same size, shape, fill, and icon-button styling.
- The tooltip trigger icons should be visually subtle but clearly interactive, with the warning icon using a caution color treatment.
- Do not show the backup/help text, button labels, or restore warning as always-visible inline callouts inside the card body.
- **Download data**, **Share data**, **Export logs**, and **Upload data** should all use the same filled icon-button treatment so they read as one cohesive action family.

## Daily Automated Backup Behavior

- Tapping the **Daily automated backups** toggle to ON enables a once-per-day automated backup schedule.
- While enabled, the app creates an automated backup at **precisely 11:30 PM local device time** every day.
- Tapping the toggle back to OFF disables future automated backups.
- Automated backup scheduling is **fixed** in this iteration:
  - the user cannot change the storage location
  - the user cannot change the frequency
  - the user cannot change the backup time
- Automated backups must be written to a fixed **shared/common device location** outside uninstall-sensitive app storage so previously created backups remain available after the app is uninstalled.
- On Android, automated backups are saved into **Downloads/Streak/Backups/Automated**.
- Automated backups should run without prompting the user for a destination. No file picker, save dialog, or share sheet is part of the automated flow.
- Automated backups should create a standalone backup copy rather than writing directly against the live in-use database file.
- Automated backups should include the same local data as manual backups:
  - the SQLite database
  - uploaded picture-proof files referenced by check-ins
- Newly saved proof pictures should live outside uninstall-sensitive app storage:
  - on **Android**, under **Pictures/Streak/CheckinProofs**
  - on **Windows**, under **Pictures\Streak\CheckinProofs**
- Automated backups should use a timestamped filename pattern such as `streak-auto-data-backup-YYYYMMdd-HHmmss.zip`.
- On Android, a successful nightly automated backup should post a native completion notification when the app has notification permission.
- Tapping that Android completion notification should attempt to open the shared parent folder that contains the automated backups (`Downloads/Streak/Backups/Automated`). If the platform cannot deep-link to that exact folder, falling back to the broader Downloads surface is acceptable.
- If Android notification permission is denied, nightly backups should still run normally; only the completion notification is skipped.
- There is no user-facing control yet for backup history, retention, cleanup, schedule customization, or destination customization.

### Platform-specific Automated Backup UX

| Platform | Expected behavior                                                                                                                                                                                                                                                                                                          |
| -------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Windows  | Automated backups are unavailable. The toggle remains disabled, and the app does not schedule or run nightly automated backups.                                                                                                                                                                                            |
| Android  | The toggle is enabled. Turning it ON schedules the nightly 11:30 PM local alarm, each run saves a timestamped `.zip` data-backup archive into `Downloads/Streak/Backups/Automated` without prompting the user, and successful runs can post a completion notification that attempts to open the backup folder when tapped. |

## Export Behavior

- Tapping **Download data** creates a data-backup archive of the app's local data and then saves it using a platform-specific file flow.
- Export is a **manual** action; it does not run automatically.
- Manual export remains available even when automated backups are enabled.
- Export and Share should both create a standalone archive rather than handing out the live in-use database file directly.
- The export action does **not** modify habits, checkins, reminder settings, or automated backup settings.
- The exported backup should include:
  - the user's habit data plus saved reminder and backup preferences stored in the local database
  - uploaded picture-proof files referenced by the backup's check-ins and still available in current proof storage
- Export is considered a low-frequency maintenance / safety action, so it lives in **Settings** rather than in the Homepage app bar.
- The exported filename should use a timestamped pattern such as `streak-data-backup-YYYYMMdd-HHmmss.zip`.
- The platform-specific save note is exposed from the **Backup** info tooltip rather than as persistent inline helper text.
- After a successful manual export, the page should show an in-app success confirmation with an **Open folder** affordance so the user can jump straight to the saved backup location.
- If some check-ins still point to proof files that are no longer available in current app storage, export should still succeed and skip only those unavailable proof files.

### Platform-specific Export UX

| Platform | Expected behavior                                                                                                                                                                                                                                                           |
| -------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Windows  | Open a standard **Save As** file dialog prefilled with the timestamped backup filename. The user chooses where to save the `.zip` archive and confirms the dialog, then sees an in-app confirmation with an **Open folder** action that opens Explorer to the saved backup. |
| Android  | Save the timestamped backup archive directly into **Downloads/Streak/Backups/Manual**. No share sheet should be shown for the normal export flow. After save, show an in-app confirmation with an **Open folder** action for that folder.                                   |

- Do **not** use the operating system share sheet as the primary export UX on either platform.
- On Windows, cancelling the file-save dialog is treated as a user cancellation, not as an export error.
- On Android, a successful export should leave the file available in **Downloads/Streak/Backups/Manual** so the user can manage it with the system file manager or share it later if they choose.

## Share Behavior

- Tapping **Share data** creates a data-backup archive and opens a platform-native share flow so the user can hand that `.zip` file to another app or service manually.
- Share is a **manual** action; it does not run automatically or on a schedule.
- Share does **not** create cloud sync, account linkage, or periodic uploads. It is strictly a one-time user-initiated handoff.
- The shared backup should include the same database contents and uploaded picture-proof files as the normal export flow, including the same skip behavior for any unavailable proof files.
- The shared backup should use the same timestamped filename pattern as export, such as `streak-data-backup-YYYYMMdd-HHmmss.zip`.
- Share is considered a low-frequency maintenance / portability action, so it lives in **Settings** beside **Download data**.

### Platform-specific Share UX

| Platform | Expected behavior                                                                                                                                     |
| -------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| Windows  | Sharing is disabled. During local Windows development/debugging, use **Download data** instead to save the backup archive where you want it.          |
| Android  | Create the backup archive, then open the system share sheet with the generated `.zip` file so the user can send it to apps such as Drive or WhatsApp. |

- On Android, dismissing or cancelling the share UI is treated as a user cancellation, not as a share error.
- On Windows, the **Share data** control remains disabled.
- Share should remain additive to the normal export flow rather than replacing it.

## Diagnostic Export Behavior

- Tapping **Export logs** packages recent diagnostic logs into a standalone support artifact and then saves it using a platform-specific file flow.
- Diagnostic export is a **manual** action; it does not run automatically or on a schedule.
- The export should read from diagnostic logs stored in persistent app-private storage, not directly from temporary cache storage.
- The exported support bundle should use a timestamped filename pattern such as `streak-diagnostics-YYYYMMdd-HHmmss.zip`.
- The exported support bundle may include:
  - recent structured log files
  - a small manifest or metadata file with app version, platform, OS version, and export timestamp
- The exported support bundle must **not** silently include the live database or a data-backup archive. Data export/share remains a separate explicit action.
- Diagnostic export is considered a low-frequency support / troubleshooting action, so it lives in **Settings** beside the other maintenance actions.
- If the app has few or no recent logs available, export should still succeed by creating a valid diagnostics bundle with whatever diagnostic metadata is available.

### Platform-specific Diagnostic Export UX

| Platform | Expected behavior                                                                                                                                                    |
| -------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Windows  | Open a standard **Save As** file dialog prefilled with the timestamped diagnostics filename. The user chooses where to save the `.zip` file and confirms the dialog. |
| Android  | Save the timestamped diagnostics `.zip` file directly into **Downloads/Streak/Diagnostics**. No share sheet should be shown for the normal diagnostics export flow.  |

- Do **not** use the operating system share sheet as the primary diagnostics export UX on either platform.
- On Windows, cancelling the file-save dialog is treated as a user cancellation, not as a diagnostics export error.
- On Android, a successful diagnostics export should leave the `.zip` file available in **Downloads/Streak/Diagnostics** so the user can inspect it or share it later if they choose.

## Restore Behavior

- Tapping **Upload data** opens a platform-native file picker that accepts both `.zip` files and raw `.db` files.
- Once a valid backup file is selected, the user is shown a **confirmation dialog** warning that all existing data will be replaced.
- If the user confirms, the app restores from the selected file:
  - **`.zip` data-backup archive**:
    1. close the current database connection
    2. replace the live database file with the archived database
    3. restore uploaded picture-proof files from the archive back into the current shared proof-media location
    4. reopen the database connection
  - **`.db` database file**:
     1. close the current database connection
     2. replace only the live SQLite database file
     3. leave the current uploaded picture-proof files untouched
     4. clear proof metadata for any restored check-ins whose referenced proof files are unavailable in current app storage
     5. reopen the database connection
- Restore is a **manual** action; it does not run automatically.
- After a successful restore the app **navigates to the Homepage** so the user sees freshly loaded data.
- On failure the app rolls back to the previous database state and surfaces a clear error message; the user remains on Settings.
- Restore is considered a destructive, low-frequency action so it lives in **Settings** with a prominent warning.
- The destructive warning should be available from the **Restore** warning tooltip in the resting page layout, then repeated in the confirmation dialog before restore proceeds.

### Platform-specific Restore UX

| Platform | Expected behavior                                                                                                                                                   |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Windows  | Open a standard **Open File** dialog filtered to `.zip` and `.db` files. The user selects a Streak backup archive or legacy database backup and confirms.           |
| Android  | Open the system file picker for a Streak backup archive or legacy database backup (via `StorageAccessFramework` / intent). The user selects a `.zip` or `.db` file. |

- On both platforms, cancelling the file-picker dialog is treated as a user cancellation, not as a restore error.
- The app must validate that the selected file is a recognizable Streak data-backup archive or SQLite database before overwriting the live database.

## Reminder Behavior

- When enabled, the app schedules a **local notification** at the configured time each day.
- Fresh installs default reminders to **disabled** so Android notification permission is requested only after the user explicitly enables reminders.
- If reminders are already enabled from an existing database or restored backup, the app should request Android notification permission on app launch and when the user opens Settings.
- The notification fires **only if** at least one habit has not been checked in as done for that day.
- If all habits are already marked done before the reminder time, **no notification is sent**.
- The notification content should include:
  - Title: *"Streak Reminder"*
  - Body: *"You have {N} habit(s) pending today."* (where N is the count of unchecked habits).
- Tapping the notification opens the app to the [Homepage](./homepage.md).

## Defaults

| Setting                   | Default Value               |
| ------------------------- | --------------------------- |
| Reminder enabled          | OFF                         |
| Reminder time             | 9:00 PM (local device time) |
| Automated backups enabled | OFF                         |

## Persistence

- Settings are saved to **local SQLite** immediately when changed (no explicit "Save" button).
- Settings persist across app restarts.
- The automated backup preference is saved immediately when changed, and the fixed 11:30 PM schedule should be re-established from persisted settings after app restart / relaunch.
- Automated backups are stored outside the live app database location in a fixed shared/common directory that survives uninstall:
  - On **Android**, in **Downloads/Streak/Backups/Automated**.
  - On **Windows**, automated backups are not available.
- Export creates a backup on demand; it is not auto-saved in the background.
- Diagnostic export creates a diagnostics bundle on demand; it is not auto-saved or regenerated continuously in the background.
- Share creates a backup archive on demand and immediately hands it to the operating system's share flow; it is not auto-saved or repeated in the background.
- Restore replaces the live database on demand after user confirmation. When the selected file is a `.zip` archive, it also reloads picture-proof files from that archive. When the selected file is a `.db` backup, the current uploaded picture-proof files remain untouched, but any restored proof references without matching files are cleared during reconciliation.
- Exported backup archives are stored outside the live app database location:
  - On **Windows**, wherever the user selects in the file-save dialog.
  - On **Android**, in **Downloads/Streak/Backups/Manual**.
- Exported diagnostics bundles are stored outside the app's private log directory:
  - On **Windows**, wherever the user selects in the file-save dialog.
  - On **Android**, in **Downloads/Streak/Diagnostics**.

## Edge Cases

| Scenario                                       | Behavior                                                                                                                                                                                                              |
| ---------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| User has no habits                             | Reminder toggle is still available but no notification will fire (0 pending habits).                                                                                                                                  |
| User enables automated backups before 11:30 PM | The first automated backup should run at **11:30 PM local time today**.                                                                                                                                               |
| User enables automated backups after 11:30 PM  | The first automated backup should run at **11:30 PM local time tomorrow**.                                                                                                                                            |
| User disables automated backups                | Future scheduled automated backups stop; previously created backup files remain untouched.                                                                                                                            |
| User changes device timezone or clock          | The next automated backup follows the device's current local-time interpretation of **11:30 PM**.                                                                                                                     |
| App is uninstalled                             | Previously created automated backup files remain in `Downloads/Streak/Backups/Automated` because they are outside uninstall-sensitive app storage.                                                                    |
| User has no habits but exports                 | Export is still allowed so the user can back up reminder settings, automated backup settings, or an empty database state.                                                                                             |
| User has no habits but shares                  | Share is still allowed so the user can manually hand off reminder settings, automated backup settings, or an empty database state.                                                                                    |
| User disables reminders                        | No notifications are scheduled. The time picker is hidden.                                                                                                                                                            |
| User changes time                              | The next reminder is rescheduled to the new time. If the new time has already passed for today, the next reminder fires tomorrow.                                                                                     |
| App is force-closed                            | Reminders should still fire (use Android's alarm/notification scheduling APIs that persist beyond app lifecycle).                                                                                                     |
| User cancels Windows save dialog               | Keep the user on Settings and treat the action as cancelled rather than failed.                                                                                                                                       |
| Android export succeeds                        | The backup archive appears in **Downloads/Streak/Backups/Manual** with the generated timestamped filename.                                                                                                            |
| Export fails                                   | Keep the user on Settings and surface a clear error message rather than silently failing.                                                                                                                             |
| User exports diagnostics with few/no logs      | Export still succeeds, creating a valid diagnostics bundle with any available metadata and log content.                                                                                                               |
| User cancels diagnostics export save dialog    | Keep the user on Settings and treat the action as cancelled rather than failed.                                                                                                                                       |
| Android diagnostics export succeeds            | The diagnostics `.zip` appears in **Downloads/Streak/Diagnostics** with the generated timestamped filename.                                                                                                           |
| Diagnostics export fails                       | Keep the user on Settings and surface a clear error message rather than silently failing.                                                                                                                             |
| User cancels share sheet                       | Keep the user on Settings and treat the action as cancelled rather than failed.                                                                                                                                       |
| Share fails                                    | Keep the user on Settings and surface a clear error message rather than silently failing.                                                                                                                             |
| User cancels restore file picker               | Keep the user on Settings and treat the action as cancelled rather than failed.                                                                                                                                       |
| User selects invalid/corrupt backup file       | Keep the user on Settings and surface a clear error message; do not overwrite the live database.                                                                                                                      |
| Backup/share encounters missing proof files    | Keep the user on Settings, create the `.zip` archive anyway, and omit only the unavailable proof files from that archive.                                                                                             |
| User restores from a `.db` file                | Replace only the live database, keep existing uploaded picture-proof files untouched, clear any restored proof metadata whose files are unavailable, then navigate the user to the Homepage with freshly loaded data. |
| Restore succeeds                               | Replace the live database with the selected backup copy, restore any archived picture-proof files only for `.zip` restores, then navigate the user to the Homepage with freshly loaded data.                          |
| Restore fails mid-way                          | Roll back to the previous database state, keep the user on Settings, and surface a clear error message.                                                                                                               |
