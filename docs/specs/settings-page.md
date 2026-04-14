# Settings Page

> **Route**: `/settings`

The Settings page currently focuses on low-frequency **data management** actions for the local database. Users access it from the **⚙** icon in the app bar.

Daily reminder controls are not part of the current Settings screen baseline. Keep reminder-related exploration in [`future.md`](./future.md) until that work is ready for its own accepted spec.

## Navigation

- Accessible from the **⚙** icon in the Homepage app bar.
- A **back arrow** in the app bar returns the user to the [Homepage](./homepage.md).
- Secondary-screen chrome stays focused: show **Back** + `Settings` only.
- Data actions remain inside the page content rather than becoming dedicated app-bar icons.

## Layout

The current page contains a single clean **Data** card.

- Prefer `MudPaper`, `MudStack`, `MudIconButton`, `MudTooltip`, and `MudText`.
- No custom page-specific CSS should be required beyond ordinary spacing and alignment.

### Data Section

| Element                | Type                    | Details                                                                                                                                   |
| ---------------------- | ----------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| Section eyebrow        | Text                    | **"Data"**                                                                                                                                 |
| Section header         | Text                    | **"Backup"**                                                                                                                               |
| Backup info icon       | Glyph + tooltip         | Small info icon beside **Backup**. Hover/focus/press shows: *"Android saves to 'Downloads' folder. Windows lets you choose where to save."* |
| Section description    | Caption                 | *"Download, share, or restore your local data."*                                                                                           |
| Data action group      | Inline icon-button row  | Three icon-only sibling actions shown in this order: **Download DB**, **Share DB**, **Upload DB**.                                      |
| Download action        | `MudIconButton`         | Download icon. Tooltip and accessible name: **"Download DB"**. Starts the manual backup-save flow.                                       |
| Share action           | `MudIconButton`         | Share icon. Tooltip and accessible name: **"Share DB"**. Starts the manual share flow for a generated backup copy.                       |
| Upload action          | `MudIconButton`         | Upload icon. Tooltip and accessible name: **"Upload DB"**. Opens a file picker to restore a `.db` backup file.                           |
| Restore warning affordance | Icon + caption / tooltip | Communicates that uploading a backup replaces **all existing data** and cannot be undone.                                              |

- The three data actions should read as a compact, visually matched sibling group.
- The resting layout should keep the buttons **icon-only**; text labels belong in tooltips and accessible names, not inline inside the buttons.
- **Download DB** and **Share DB** are non-destructive manual actions.
- **Upload DB** is destructive and should remain clearly signposted with warning affordance/copy.

## Download Behavior

- Tapping **Download DB** creates a backup of the app's local database and then saves it using a platform-specific file flow.
- Download is a **manual** action; it does not run automatically.
- The download action does **not** modify habits, checkins, or reminder settings stored in the live database.
- The exported backup should include the user's habit data plus other settings stored in the local database.
- The generated filename should use a timestamped pattern such as `streak-backup-YYYYMMdd-HHmmss.db`.
- The platform-specific save note is exposed from the **Backup** info tooltip rather than as persistent inline helper text.

### Platform-specific Download UX

| Platform | Expected behavior                                                                                                                                              |
| -------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Windows  | Open a standard **Save As** file dialog prefilled with the timestamped backup filename. The user chooses where to save the `.db` file and confirms the dialog. |
| Android  | Save the timestamped backup file directly into the device's **Downloads** folder.                                                                              |

- Do **not** use the operating system share sheet as the primary download UX.
- On Windows, cancelling the file-save dialog is treated as a user cancellation, not as a download error.
- On Android, a successful download should leave the file available in **Downloads** so the user can manage or share it later if they choose.

## Share Behavior

- Tapping **Share DB** creates a backup of the app's local database and opens a native share flow for that generated `.db` file.
- Share is a **manual** action; it does not run automatically.
- Sharing does **not** modify the live database.
- The shared file should use the same timestamped backup naming convention as **Download DB**.
- **Share DB** is additive: it complements **Download DB** rather than replacing it.
- If the share flow is cancelled or dismissed by the user, treat it as a user cancellation rather than an error.
- If the share flow fails, keep the user on Settings and surface a clear error message.

### Platform-specific Share UX

| Platform | Expected behavior                                                                                                             |
| -------- | ----------------------------------------------------------------------------------------------------------------------------- |
| Windows  | Open a native share flow for the generated `.db` backup so the user can hand it off to another compatible app or destination. |
| Android  | Open the system share sheet for the generated `.db` backup.                                                                   |

## Upload Behavior

- Tapping **Upload DB** opens a platform-native file picker scoped to `.db` files.
- Once a valid backup file is selected, the user is shown a **confirmation dialog** warning that all existing data will be replaced.
- If the user confirms, the app closes the current database connection, replaces the live database file with the selected backup, then reopens the connection.
- Upload is a **manual** action; it does not run automatically.
- After a successful upload the app **navigates to the Homepage** so the user sees freshly loaded data.
- On failure the app rolls back to the previous database state and surfaces a clear error message; the user remains on Settings.
- The destructive warning should be visible in the resting layout, then repeated in the confirmation dialog before restore proceeds.

### Platform-specific Upload UX

| Platform | Expected behavior                                                                                                                     |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| Windows  | Open a standard **Open File** dialog filtered to `.db` files. The user selects a backup file and confirms the dialog.                 |
| Android  | Open the system file picker filtered to `.db` files (via `StorageAccessFramework` / intent). The user selects a backup file.          |

- On both platforms, cancelling the file-picker dialog is treated as a user cancellation, not as an upload error.
- The app must validate that the selected file is a recognizable Streak backup before overwriting the live database.

## Persistence

- Download creates a backup on demand; it is not auto-saved in the background.
- Share creates a backup on demand solely for the share flow; it does not change the live database.
- Upload replaces the live database on demand; the previous data is permanently overwritten after user confirmation.
- Downloaded backup files are stored outside the live app database location:
  - On **Windows**, wherever the user selects in the file-save dialog.
  - On **Android**, in the device's **Downloads** folder.

## Edge Cases

| Scenario                        | Behavior                                                                                                                          |
| ------------------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| User has no habits but downloads | Download is still allowed so the user can back up an empty database state.                                                      |
| User has no habits but shares   | Share is still allowed so the user can send an empty database state to another app or device.                                   |
| User cancels Windows save dialog | Keep the user on Settings and treat the action as cancelled rather than failed.                                                 |
| Android download succeeds       | The backup file appears in **Downloads** with the generated timestamped filename.                                                |
| Download fails                  | Keep the user on Settings and surface a clear error message rather than silently failing.                                        |
| User cancels share flow         | Keep the user on Settings and treat the action as cancelled rather than failed.                                                  |
| Share fails                     | Keep the user on Settings and surface a clear error message rather than silently failing.                                        |
| User cancels upload file picker | Keep the user on Settings and treat the action as cancelled rather than failed.                                                  |
| User selects invalid/corrupt file | Keep the user on Settings and surface a clear error message; do not overwrite the live database.                               |
| Upload succeeds                 | Replace the live database with the backup, then navigate the user to the Homepage with freshly loaded data.                      |
| Upload fails mid-way            | Roll back to the previous database state, keep the user on Settings, and surface a clear error message.                         |
