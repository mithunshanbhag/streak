# Streak: Keep That Habit Alive

> *Let your habits compound.*

Streak is a simple habit-tracking mobile app for Android. It helps users build and maintain daily habits by making it effortless to check in each day and watch their streaks grow over time.

## Design Philosophy

- **Speed over features.** The daily workflow (check in → exit) should take seconds, not minutes.
- **Minimal navigation.** The landing page is the primary surface; most tasks require zero page clicks.
- **Simplicity over completeness.** Binary habits (done / not done), a hard cap of 10 habits, and local-first data with optional backup safety nets.

## Core Concepts

### Habit

A recurring daily activity the user wants to track.

| Property     | Required | Details                                                                                                                                 |
| ------------ | -------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| Name         | Yes      | Short descriptive label (e.g., "Meditate", "Read"). Max 30 characters.                                                                  |
| Emoji / Icon | No       | A single emoji to visually represent the habit (e.g., 🧘, 📖). A default icon is used if none is selected.                                |
| Description  | No       | Plain-text supporting notes for the habit. Multiline input is allowed, line breaks are preserved, and the max length is 500 characters. |

- A user can have between 0 and **10** habits at any time.
- On the [Homepage](./homepage.md), habits are listed in **alphabetical order by name**.
- Habit descriptions are edited in the quick-add / edit flows and shown on [Habit Details](./habit-details-page.md), but they are **not displayed on the Homepage**.
- Tapping a habit on the Homepage opens that habit's [Habit Details page](./habit-details-page.md).

### Checkin

A daily record that exists only when a habit is marked **done** for a given calendar day.

- Checkins are **binary**: a record exists when done, and no record exists when not done. There is no partial completion or quantity tracking.
- Checkins apply to **today only**, where **today** means the device's current **local calendar day**. Backdating (marking a past day) is not supported.
- A habit that has no checkin recorded for a day is treated as **not done**.
- If the user unchecks a same-day habit, that day's checkin record is deleted.
- A same-day checkin may also include:
  - one optional short note
  - one optional picture proof linked from the checkin record
- There are two ways a checkin happens:
  1. **Voluntary**: the user taps the toggle on the homepage as soon as they complete the activity, then completes the [check-in dialog flow](./checkin-dialogs.md).
  2. **Via reminder**: at a user-configured time each day, the app sends a notification prompting the user to check in on any habits that are still pending (not yet marked done).

### Streak

A streak is the count of **consecutive calendar days** on which a habit was checked in as done, ending with today (or the most recent completed day).

- A streak starts at **1** on the first day a habit is marked done.
- The streak increments by 1 for each subsequent consecutive day the habit is marked done.
- The streak **resets to 0** if the user does not check in for a day (i.e., the day passes without a "done" checkin).
- A newly created habit starts with a streak of **0**.
- Streak calculations use the device's current **local calendar day**, not UTC.

## Data Storage

- Live habit data is stored **locally on the device**. Streak does **not** do live cloud sync of the in-use database.
- Cloud backup is **strictly optional**. The app must remain fully usable offline without any Microsoft account.
- On **Android** in this iteration, a user may optionally sign in with a **personal Microsoft account** and upload the same `.zip` backup archives into the app's private OneDrive app folder.
- Users may manually export or share full **data backup archives** (`.zip`) that include the local database plus any uploaded picture-proof files that are still available in the app's current proof storage.
- After a manual data-backup export succeeds, the app should show a lightweight in-app confirmation and let the user quickly open the parent folder that now contains the backup archive.
- On **Android**, manual exports, automated backups, and diagnostics exports are organized under `Downloads/Streak` so Streak artifacts stay easy to find without cluttering the top-level Downloads folder.
- On **Android**, users may also enable nightly automated local backups that save timestamped `.zip` data archives into shared device storage.
- On **Android**, users may also enable separate nightly **OneDrive** backups that upload the same archive format into OneDrive. Enabling cloud automated backups does **not** implicitly enable local automated backups, and vice versa.
- OneDrive cloud backup is **backup-only** in this iteration. Local restore remains available from local files, while cloud restore is deferred to a later phase.
- OneDrive cloud backups use the app-specific folder at `/me/drive/special/approot`, count against the user's OneDrive quota, and leave retention / cleanup management to the user for now.
- On Android, a successful nightly automated backup should also be able to post a local completion notification, subject to the platform's notification permission.
- Data will be persisted across app restarts.
- If a raw `.db` restore recreates check-ins whose proof files are no longer present in current app storage, the restore should keep the check-ins but clear those orphaned proof references so later backups remain healthy.

### Storage Layout

The app uses several storage categories:

| Category                 | Android                                                       | Windows                                                       | Purpose                                                                                                                                              |
| ------------------------ | ------------------------------------------------------------- | ------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| Live app data            | App-private `FileSystem.Current.AppDataDirectory`             | App-private `FileSystem.Current.AppDataDirectory`             | Live SQLite database and persistent diagnostics that users should not edit directly.                                                                  |
| Shared proof media       | `Pictures/Streak/CheckinProofs` through Android `MediaStore`  | `Pictures\Streak\CheckinProofs`                               | New user-uploaded check-in proof pictures live outside uninstall-sensitive app storage so they survive app upgrades and uninstalls.                 |
| Temporary working files  | App-private `FileSystem.Current.CacheDirectory/ExportWorking` | App-private `FileSystem.Current.CacheDirectory/ExportWorking` | Disposable backup, share, restore, and diagnostics-export staging files.                                                                             |
| User-visible exports     | `Downloads/Streak/...` through Android `MediaStore.Downloads` | User-selected save location through the Windows file picker   | Files the user explicitly exports, shares, or keeps as local backups, including data-backup archives and diagnostics.                                |
| Optional cloud backups   | OneDrive app folder (`/me/drive/special/approot`)             | Not supported in this iteration                               | User-enabled backup destination for the same `.zip` archives used by local backup. This is backup-only, not live sync.                              |
| Proof-file resilience    | Raw `.db` restore reconciles missing proof references         | Raw `.db` restore reconciles missing proof references         | Direct database restores keep check-ins but clear proof metadata whose referenced files are unavailable; `.zip` backups preserve and restore proofs. |

Android app-private storage:

```text
AppDataDirectory/
  streak.db
  streak.db-wal
  streak.db-shm
  Diagnostics/
    streak-diagnostics.log

CacheDirectory/
  ExportWorking/
    streak-data-backup-YYYYMMdd-HHmmss.zip
    streak-auto-data-backup-YYYYMMdd-HHmmss.zip
    RestoreExtracted/
      streak.db
      CheckinProofs/
    streak-diagnostics-YYYYMMdd-HHmmss.zip
```

Android shared proof-media storage:

```text
Pictures/
  Streak/
    CheckinProofs/
      Habit-7/
        2026/
          04/
            2026-04-21/
              habit-7-20260421-083012.jpg
```

Android user-visible storage:

```text
Downloads/
  Streak/
    Backups/
      Manual/
        streak-data-backup-YYYYMMdd-HHmmss.zip
      Automated/
        streak-auto-data-backup-YYYYMMdd-HHmmss.zip
    Diagnostics/
      streak-diagnostics-YYYYMMdd-HHmmss.zip
```

Android OneDrive app-folder storage:

```text
approot/
  Backups/
    Manual/
      streak-data-backup-YYYYMMdd-HHmmss.zip
    Automated/
      streak-auto-data-backup-YYYYMMdd-HHmmss.zip
```

Windows app-private storage:

```text
AppDataDirectory/
  streak.db
  streak.db-wal
  streak.db-shm
  Diagnostics/
    streak-diagnostics.log

CacheDirectory/
  ExportWorking/
    streak-data-backup-YYYYMMdd-HHmmss.zip
    RestoreExtracted/
      streak.db
      CheckinProofs/
    streak-diagnostics-YYYYMMdd-HHmmss.zip
```

Windows shared proof-media storage:

```text
Pictures\
  Streak\
    CheckinProofs\
      Habit-7\
        2026\
          04\
            2026-04-21\
              habit-7-20260421-083012.jpg
```

Windows user-visible storage:

```text
<user-selected-folder>/
  streak-data-backup-YYYYMMdd-HHmmss.zip
  streak-diagnostics-YYYYMMdd-HHmmss.zip
```

Windows does not currently support automated backups or OneDrive cloud backup. Android manual share uses a generated `streak-data-backup-YYYYMMdd-HHmmss.zip` archive and hands it to the native share sheet; it does not create a separate durable export unless the user chooses to save it through another app.

## Notifications and Reminders

- The user can configure a **daily reminder time** (e.g., 9:00 PM) in settings.
- The reminder fires **only if** there is at least one habit that has not been checked in as done for the day.
- If all habits are already checked in, no reminder is sent.
- The reminder should include a summary (e.g., "You have 2 habits pending today").
- Daily reminders are **disabled by default** on fresh installs. If the user enables them, the app requests the platform notification permission before reminder notifications can be shown.
- The default reminder time is **9:00 PM** (local device time).
- Reminders can be disabled entirely by the user.
- Backup-completion feedback is separate from reminder notifications:
  - manual **Download data** success uses an in-app confirmation with a quick folder-open action
  - Android nightly automated backups may post a native completion notification that attempts to open the backup folder when tapped

## Time and Timezone Behavior

- The product is intentionally **local-time-first**. Anywhere the UI or behavior refers to **today**, **day**, or **daily**, it uses the device's current **local date/time**, not UTC.
- This includes the Homepage date banner, check-in toggle state, streak calculations, history heatmap highlighting, and reminder scheduling.
- Some corner cases are accepted with this choice:
  - if the user travels to a different timezone, **today** follows the device's new local timezone
  - if the user manually changes the device clock or timezone, the app's notion of **today** changes with it
  - near timezone changes or travel, a check-in may appear under a different local day than the user expected before the device timezone changed
- These tradeoffs are acceptable for this app because it is a local-only habit tracker centered on the user's current day-to-day experience rather than globally synchronized UTC timelines.

## Diagnostics and Telemetry

- The app should use the standard `.NET` `ILogger` abstraction for application logging and diagnostics.
- Diagnostics are intentionally **local-first**: the app should not require Azure Application Insights or any other cloud telemetry service to function.
- The primary production telemetry sink should be **local structured log files** stored in the app's persistent private storage.
- Diagnostic log files must be stored in **persistent app data**, not in a cache-only location, so they survive routine app restarts and can be exported later.
- The app may also emit platform-native debug logs in development builds, but local file-based diagnostics are the primary end-user support mechanism.
- End users should not be expected to manually browse the app sandbox to retrieve logs. The product should provide an explicit **Export diagnostics** and/or **Share diagnostics** action.
- Exported diagnostics should package recent log files into a user-portable artifact, such as a `.zip`, created in temporary storage and then saved or shared through the native platform flow.
- Diagnostics exports may include lightweight environment metadata helpful for support, such as app version, platform, OS version, and timestamp, but must not include the full database unless the user explicitly chooses a separate backup/share action.
- Diagnostic logging should avoid collecting unnecessary personal content. In particular, logs should not intentionally dump raw database contents or excessively verbose user-authored habit notes/descriptions.
- Any future cloud telemetry integration must be strictly optional and explicitly user-enabled; the baseline product remains fully usable without network connectivity.

## Non-Functional Requirements

See [non-functional-requirements.md](./non-functional-requirements.md).

## Surface and Route Inventory

Each major surface has its own detailed spec:

| Surface          | Route / Trigger           | Spec                                             | Purpose                                                                                        |
| ---------------- | ------------------------- | ------------------------------------------------ | ---------------------------------------------------------------------------------------------- |
| Homepage         | `/`                       | [homepage.md](./homepage.md)                     | Landing page, daily checkin surface, and habit list                                            |
| Check-in Dialogs | Homepage habit toggle     | [checkin-dialogs.md](./checkin-dialogs.md)       | Collect optional note / picture proof before save and confirm removal before undoing a checkin |
| Habit Details    | `/habits/{habitId}`       | [habit-details-page.md](./habit-details-page.md) | Habit details, trends, edit dialog, and deletion                                               |
| Quick Add Habit  | `+ New Habit` on Homepage | [create-habit-page.md](./create-habit-page.md)   | Create a new habit in a compact dialog without leaving the homepage                            |
| Settings         | `/settings`               | [settings-page.md](./settings-page.md)           | Configure reminders and manage local backup, optional OneDrive backup, local restore, and diagnostics export/share |

## Information Architecture Notes

- The app remains **shallow by default**: **Homepage** is the landing page, with **Habit Details** and **Settings** as the primary secondary destinations.
- The Homepage includes a dedicated **`+ New Habit`** CTA below the habit list, which opens a compact **Quick Add Habit** dialog over **Homepage**.
- The **Homepage** doubles as the habit-list maintenance surface: habits are shown alphabetically and each habit opens its details on the Habit Details page.
- The Homepage app bar keeps **Settings** plus a right-most **GitHub** repo link instead of a global create icon.
- The **Habit Details** page contains the heatmap, edit dialog flow, and delete confirmation dialog for a single habit.
- **Settings** groups reminder preferences plus low-frequency data actions such as **Daily automated backups**, **OneDrive backup**, **Download data**, **Share data**, **Upload data**, and diagnostics export/share.
- **Homepage** opens directly into the habit list without instructional header copy, progress summary text, or a habit-count chip.
- There is no dedicated habit-list routed page separate from **Homepage**.
- There is no dedicated routed **Create Habit** page in the simplified direction.
- Separate **Edit Habit** and **Delete Habit** routed pages are no longer part of the app structure.

Common UI specifications (theme, typography, iconography, navigation):

- [ui.md](./ui.md)

## UX Exploration

- [ux-simplification-review.md](./ux-simplification-review.md) - accepted and rejected simplification ideas, plus links to the updated primary mockups.
