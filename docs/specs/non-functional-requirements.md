# Non-Functional Requirements

This document captures cross-cutting requirements that apply across the Streak app, independent of any single page or feature flow.

## Performance

- The app should launch and be ready for interaction within **2 seconds** on a typical supported device.
- Homepage rendering and day-state hydration should feel immediate after launch.
- Checkin toggling should feel **instant**, with no visible loading spinners for the normal success path.
- Common local-only operations such as creating a habit, editing a habit, or toggling a checkin should complete quickly enough to feel synchronous to the user.

## Offline and Availability

- The app should work fully **offline** for all core habit-tracking features.
- The app must not depend on user accounts, remote APIs, or cloud connectivity for core usage.
- Local data should persist across app restarts and normal device reboots.

## Storage and Footprint

- The app should maintain a minimal battery, CPU, and storage footprint appropriate for a lightweight local utility.
- Background activity should be limited to explicitly supported features such as reminders and Android automated backups.
- Diagnostic log retention should be bounded so logging does not grow storage usage without limit.

## Diagnostics and Supportability

- The app should expose enough structured diagnostic information to troubleshoot failures on real devices without requiring a debugger.
- Application logging should flow through the standard `.NET` `ILogger` abstraction.
- Production diagnostics should default to a **local structured file sink** rather than a mandatory cloud telemetry dependency.
- Persistent diagnostic files should live in app-private long-lived storage so they can be exported later if the user chooses.
- Exported diagnostics should be easy to retrieve through a user-facing save/share flow rather than requiring manual access to app-private storage; on Android, exported support bundles should live under `Downloads/Streak/Diagnostics`.
- Diagnostics exports should be scoped to operational troubleshooting data, not treated as a silent full-data backup mechanism.

## Privacy

- The baseline product should not upload telemetry, traces, or logs to any cloud service by default.
- Any future cloud telemetry capability must be opt-in and clearly communicated to the user.
- Diagnostic output should avoid unnecessary capture of user-authored content and should never intentionally dump raw database contents as part of routine logging.

## Platform Behavior

- The app is optimized first for **Android**, while supporting local desktop development and Windows usage where explicitly implemented.
- Device-local time remains the source of truth for day-boundary behavior, reminders, and streak calculations.
- Platform-specific capabilities may differ, but the core product behavior and local-first data model should remain consistent.
