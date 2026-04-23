# Future Improvement: Optional OneDrive Cloud Backups

## Summary

The accepted product behavior and UI direction for optional OneDrive backup now live in the main specs:

- `docs\specs\README.md` - product scope, storage model, and backup layout
- `docs\specs\settings-page.md` - Settings surface behavior and user flows
- `docs\specs\ui.md` - shared visual treatment for the cloud-backup UI

This document now focuses on the **implementation planning** that still sits behind those accepted specs, especially Azure Entra setup, MAUI authentication, Microsoft Graph integration, and follow-up delivery sequencing.

## Microsoft / OneDrive Assumptions

These assumptions should be treated as implementation requirements:

- Use the OneDrive **app folder** rather than broad user file access.
- Required delegated scopes:
  - `Files.ReadWrite.AppFolder`
  - `offline_access`
- The app should be registered as a **public client** in Azure Entra ID.
- Expected redirect URIs:
  - Android: `msal{clientId}://auth`
  - Windows: `http://localhost` if Windows support is included in v1
- The app folder is private to this app and counts against the user's OneDrive quota.

## Delivery Breakdown

### 1. Product / UX decisions

- Cloud backup ships on **Android only** in v1.
- Supported sign-in audience is **personal Microsoft accounts only** in v1.
- Keep **separate local + cloud actions** instead of introducing a destination picker first.
- Keep **cloud automated backup independent** from automated local backup.
- Keep OneDrive **restore** out of v1 scope.

### 2. Azure Entra / App Registration Prep

- Register a new Azure Entra app for Streak cloud backups.
- Configure it as a **public client**.
- Add delegated API permissions:
  - `Files.ReadWrite.AppFolder`
  - `offline_access`
- Configure redirect URIs for each supported platform.
- Capture:
  - application/client ID
  - supported account audience
  - Android redirect configuration
  - Windows redirect configuration if applicable
- Prepare any app-branding, publisher, and privacy-policy items needed for consent screens and release readiness.

### 3. MAUI authentication foundation

- Add MSAL.NET integration for the MAUI app.
- Create services for:
  - sign in
  - silent token acquisition
  - sign out / disconnect
  - reading current account state
- Store auth state securely using platform-safe storage.
- Add platform-specific callback/redirect handling:
  - Android manifest / callback activity wiring
  - Windows redirect handling if included in scope

### 4. OneDrive file service

- Create a OneDrive client abstraction over Microsoft Graph for:
  - ensuring the app folder path exists
  - listing backups
  - uploading backups
  - downloading backups
  - deleting backups later if retention is added
- Normalize folder and filename rules so manual/automated paths mirror the local layout.
- Handle auth failure, network failure, and quota-exceeded responses explicitly.

### 5. Backup pipeline refactor

- Refactor the current archive-generation flow so it can target multiple destinations without duplicating backup creation logic.
- Reuse the existing archive format and proof-file skip behavior.
- Ensure cloud upload never writes against the live database directly.

### 6. Manual cloud backup

- Add a manual **Back up to OneDrive** flow in Settings.
- Reuse the existing archive creation step.
- Upload the generated archive to:
  - `approot/Backups/Manual/`
- Show clear success, cancellation, and failure states.

### 7. Automated cloud backup

- Add a separate persisted setting for cloud automated backups unless product direction says otherwise.
- Reuse the existing Android nightly trigger and execution seam.
- Fan out from the scheduled run into:
  - local only
  - cloud only
  - both
- Decide and implement v1 failure behavior:
  - next scheduled run only, or
  - same-day retry/backoff

### 8. Cloud restore

- **Deferred to a later phase after backup-only v1.**
- When that later phase starts:
  - add a flow to select a OneDrive backup after sign-in
  - download the selected archive into the existing temp/export-working area
  - hand the downloaded file to `DatabaseImportService`
  - reuse existing validation, rollback, proof restore, and `.db` compatibility logic wherever possible

### 9. Settings UX changes

- Show OneDrive connection state.
- Add connect/disconnect actions.
- Add manual cloud backup action.
- Keep local restore visually separate; do not add a cloud restore entry point in v1.
- Show at least one useful status signal:
  - last successful cloud backup timestamp
  - latest failure state requiring re-auth or retry
- Keep unsupported-platform messaging explicit.

### 10. Security / privacy / resilience

- Keep the permission footprint minimal by limiting access to the app folder.
- Do not embed a client secret in the app.
- Clear local auth state on disconnect without deleting remote backup files automatically.
- Ensure cloud failures do not block local-only app usage.
- Ensure automated cloud failures do not corrupt or block existing local backup flows.

### 11. Testing

- Add unit tests for:
  - auth state transitions
  - remote path mapping
  - upload/download orchestration
  - combined automated-destination behavior
  - restore-from-cloud orchestration
- Extend Settings component tests for new OneDrive states and actions.
- Cover failure cases:
  - sign-in cancelled
  - silent token acquisition failure
  - network unavailable
  - quota exceeded
  - corrupt remote archive

### 12. Documentation and rollout

- Keep this future spec as the planning anchor until implementation starts.
- The accepted product behavior already lives in the main specs; keep those docs aligned if implementation details force any product or UX changes.
- Update the repository `README.md` only after the feature is real.

## Initial Non-Goals

- No automatic retention or cleanup inside the app for v1.
- No cloud sync of live app state; this is backup/restore only.
- No broad OneDrive folder picker in v1 if the app folder approach is accepted.
- No change to the existing local backup archive format.
- No mandatory cloud account to use the app.

## Deferred Follow-Up

- Add OneDrive restore in a later phase, reusing `DatabaseImportService` after downloading the selected cloud archive into the app's working storage.

## Useful Microsoft References

- OneDrive app folder / special folders:
  - https://learn.microsoft.com/en-us/graph/api/resources/specialfolder?view=graph-rest-1.0
  - https://learn.microsoft.com/en-us/onedrive/developer/rest-api/concepts/appfolder
- Microsoft Graph permissions reference:
  - https://learn.microsoft.com/en-us/graph/permissions-reference
- .NET MAUI authentication with MSAL.NET:
  - https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/authentication?view=net-maui-10.0
