# Non-Functional Requirements

This document captures cross-cutting requirements that apply across the Streak app, independent of any single page or feature flow.

## Performance

- The app should launch and be ready for interaction within **2 seconds** on a typical supported device.
- Homepage rendering and day-state hydration should feel immediate after launch.
- Required Android runtime-permission recovery should happen **after** the homepage is first rendered, so permission prompting does not delay initial app visibility.
- Checkin toggling should feel **instant**, with no visible loading spinners for the normal success path.
- Common local-only operations such as creating a habit, editing a habit, or toggling a checkin should complete quickly enough to feel synchronous to the user.

## Offline and Availability

- The app should work fully **offline** for all core habit-tracking features.
- The app must not depend on user accounts, remote APIs, or cloud connectivity for core usage.
- Local data should persist across app restarts and normal device reboots.

## Storage and Footprint

- The app should maintain a minimal battery, CPU, and storage footprint appropriate for a lightweight local utility.
- Background activity should be limited to explicitly supported features such as reminders and Android automated backups.
- Telemetry volume and any retained local debug output should be bounded so logging does not grow storage or network usage without limit.

## Logging and Supportability

- The app should expose enough structured logging and telemetry to troubleshoot failures on real devices without requiring a debugger.
- Application logging should flow through the standard `.NET` `ILogger` abstraction.
- Production telemetry should default to **Azure Application Insights** as the baseline operational sink.
- Telemetry collection must stay operationally focused and must not become a silent full-data backup mechanism.
- Temporary telemetry delivery failures must not block core app workflows or compromise offline usage.

## Privacy

- The baseline product may upload operational telemetry, traces, and logs to Azure Application Insights by default.
- Telemetry should be clearly documented and limited to operational support needs.
- Logging and telemetry should avoid unnecessary capture of user-authored content and should never intentionally dump raw database contents as part of routine logging.

## Platform Behavior

- The app is optimized first for **Android**, while supporting local desktop development and Windows usage where explicitly implemented.
- Device-local time remains the source of truth for day-boundary behavior, reminders, and streak calculations.
- Platform-specific capabilities may differ, but the core product behavior and local-first data model should remain consistent.
