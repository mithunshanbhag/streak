# Future best-practice violations review

Reviewed scope: `src\`

Reviewers:
- GPT-5.4
- Claude Sonnet 4.6
- GPT-5.3-Codex

The findings below were manually verified after the multi-model review. They are ordered by reviewer consensus first, then by severity and practical impact.

## Prioritized findings

| Priority | Consensus | Severity | Finding                                                                                |
| -------- | --------- | -------- | -------------------------------------------------------------------------------------- |
| 1        | 2/3       | Resolved | Check-in date filters now run in the database query instead of loading full history    |
| 2        | 1/3       | High     | Windows proof-path handling can escape the configured proof root                       |
| 3        | 1/3       | High     | ZIP restore extraction is vulnerable to path traversal                                 |
| 4        | 1/3       | Medium   | Service contracts leak persistence models into the Razor UI layer                      |
| 5        | 1/3       | Medium   | UI date handling bypasses `TimeProvider` in a couple of user-facing places             |
| 6        | 1/3       | Medium   | Android proof saves finalize `MediaStore` records before the output stream is disposed |
| 7        | 1/3       | Low      | Registered FluentValidation validators and DTOs are effectively dead code today        |

## 1. Resolved: check-in date filters now run in the database query

**Consensus:** 2/3 reviewers  
**Status:** Resolved on 2026-04-23

**Why it matters**

The homepage and reminder-related reads ask for bounded date windows, but `CheckinRepository` fetches the full matching history first and trims it afterward in memory. That adds unnecessary I/O and allocations on core app paths, and the cost grows with every extra day of history.

**Evidence**

- `src\Streak.Ui\Repositories\Implementations\CheckinRepository.cs:30-35`
- `src\Streak.Ui\Repositories\Implementations\CheckinRepository.cs:55-60`
- `src\Streak.Ui\Repositories\Implementations\CheckinRepository.cs:102-121`
- `src\Streak.Ui\Services\Implementations\CheckinService.cs:56-70`
- `src\Streak.Ui\Services\Implementations\CheckinService.cs:73-91`

`GetByHabitNamesAsync(...)` and `GetByHabitIdsAsync(...)` call `ToListAsync(...)` before `ApplyDateRange(...)`. `GetHomePageHabitCheckinsAsync(...)` and `GetPendingHabitCountForTodayAsync(...)` both rely on that path.

**Resolution**

`CheckinRepository.GetByHabitNamesAsync(...)` and `GetByHabitIdsAsync(...)` now apply the optional `fromDate` / `toDate` bounds to the `IQueryable` before materializing results. The repository tests also cover inclusive ranges plus lower-bound-only and upper-bound-only query paths.

## 2. Windows proof-path handling can escape the configured proof root

**Consensus:** 1/3 reviewers  
**Severity:** High

**Why it matters**

`CheckinProofPathUtility` normalizes separators and blocks `.` / `..`, but it still allows drive-qualified segments such as `C:`. On Windows, that is enough for `Path.Combine(...)` to escape the intended proof root. A crafted `ProofImageUri` restored from a raw database backup can therefore make proof existence, open, or delete operations target files outside `CheckinProofs`.

**Evidence**

- `src\Streak.Ui\Misc\Utilities\CheckinProofPathUtility.cs:5-22`
- `src\Streak.Ui\Misc\Utilities\CheckinProofPathUtility.cs:49-55`
- `src\Streak.Ui\Services\Implementations\FileSystemCheckinProofFileStore.cs:28-33`
- `src\Streak.Ui\Services\Implementations\FileSystemCheckinProofFileStore.cs:36-46`
- `src\Streak.Ui\Services\Implementations\FileSystemCheckinProofFileStore.cs:49-59`
- `src\Streak.Ui\Misc\Utilities\DataBackupArchiveUtility.cs:168-199`

`DataBackupArchiveUtility` reads `ProofImageUri` values from the database and passes them to `ExistsAsync(...)`, which in turn relies on `CheckinProofPathUtility.GetAbsolutePath(...)`.

**Recommendation**

After combining the root and relative path, call `Path.GetFullPath(...)` and reject anything that does not stay under the configured proof root. Also reject rooted or drive-qualified segments up front (`Path.IsPathRooted(...)`, colon checks on Windows) instead of only filtering `.` and `..`.

## 3. ZIP restore extraction is vulnerable to path traversal

**Consensus:** 1/3 reviewers  
**Severity:** High

**Why it matters**

The restore flow accepts ZIP archives and extracts allowed entries into a working directory, but the extraction code only blocks `.` and `..` segments before combining the destination path. On Windows, a drive-qualified segment such as `C:` still escapes the extraction root.

**Evidence**

- `src\Streak.Ui\Services\Implementations\DatabaseImportService.cs:232-263`

The current guard only rejects `.` and `..` at `:252-253`, then writes the entry to `Path.Combine(extractedArchiveDirectoryPath, ...)` at `:255-263`.

**Recommendation**

Resolve the final destination with `Path.GetFullPath(...)` and verify it stays under the extraction root before writing the file. Reject rooted, drive-qualified, or otherwise escaping entry paths even if they begin with the allowed `CheckinProofs/` prefix.

## 4. Service contracts leak persistence models into the Razor UI layer

**Consensus:** 1/3 reviewers  
**Severity:** Medium

**Why it matters**

The repository guidance for this repo says the repository layer should own persistence models and the service/UI boundary should operate on DTOs or view/input models. Today the service contracts expose `Habit` and `Checkin` directly, so Razor dialogs and pages construct and consume persistence models. That makes the UI more sensitive to storage-model churn and weakens the intended layering.

**Evidence**

- `src\Streak.Ui\Services\Interfaces\IHabitService.cs:17,30,46,78,101`
- `src\Streak.Ui\Services\Interfaces\ICheckinService.cs:23,45,90,116`
- `src\Streak.Ui\Components\Dialogs\NewHabitDialog.razor:122-128`
- `src\Streak.Ui\Components\Dialogs\NewHabitDialog.razor:187-195`
- `src\Streak.Ui\Components\Dialogs\EditHabitDialog.razor:150-152`
- `src\Streak.Ui\Components\Dialogs\EditHabitDialog.razor:219-227`

**Recommendation**

Introduce explicit input/result models for create, update, and read flows, then map inside the service layer. Keep `Habit` and `Checkin` as repository-facing types.

## 5. UI date handling bypasses `TimeProvider` in a couple of user-facing places

**Consensus:** 1/3 reviewers  
**Severity:** Medium

**Why it matters**

The app is intentionally local-time-first and already injects `TimeProvider` in key service paths, but two UI surfaces bypass it and call `DateTime.Now` directly. That creates avoidable inconsistency between what the UI highlights or displays as "today" and what the service layer persists as "today", especially in tests and around clock/timezone edge cases.

**Evidence**

- `src\Streak.Ui\Components\Pages\Home.razor:227-231`
- `src\Streak.Ui\Components\Pages\HabitDetails.razor:266-268`

`Home.razor` injects `TimeProvider`, but `DateBannerText` is `static` and uses `DateTime.Now`. `HabitDetails.razor` highlights the heatmap's current day with `DateTime.Now` and does not inject `TimeProvider`.

**Recommendation**

Use injected `TimeProvider` consistently for all user-visible "today" calculations and displays on these pages. The simplest fix is to drop the `static` banner property in `Home.razor` and inject `TimeProvider` into `HabitDetails.razor`.

## 6. Android proof saves finalize `MediaStore` records before the output stream is disposed

**Consensus:** 1/3 reviewers  
**Severity:** Medium

**Why it matters**

The Android proof store clears `IS_PENDING` while the destination stream is still in scope. This repo already follows the safer pattern for backup exports, and Android `MediaStore` finalization is sensitive to stream-disposal order.

**Evidence**

- `src\Streak.Ui\Platforms\Android\Services\AndroidCheckinProofFileStore.cs:41-47`
- `src\Streak.Ui\Platforms\Android\Services\AndroidMediaStoreBackupFileWriter.cs:48-58`

**Recommendation**

Match the backup-writer sequence: fully dispose the destination stream first, then clear `IS_PENDING`.

## 7. Registered FluentValidation validators and DTOs are effectively dead code today

**Consensus:** 1/3 reviewers  
**Severity:** Low

**Why it matters**

Validators are registered globally, but the current dialog and service flows do not appear to use them. That leaves the codebase with a second validation path and DTO set that looks intentional but is currently inert, which adds maintenance cost and makes the real source of truth less obvious.

**Evidence**

- `src\Streak.Ui\Misc\ExtensionMethods\MauiAppBuilderExtensions.cs:26-29`
- `src\Streak.Ui\Services\Validators\NewHabitDialogInputModelValidator.cs:1-37`
- `src\Streak.Ui\Services\Validators\CreateHabitRequestDtoValidator.cs:1-39`
- `src\Streak.Ui\Services\Validators\UpdateHabitRequestDtoValidator.cs:1-39`
- `src\Streak.Ui\Components\Dialogs\NewHabitDialog.razor:140-173`
- `src\Streak.Ui\Components\Dialogs\EditHabitDialog.razor:172-205`

A search under `src\` found no `IValidator<>`, `ValidateAsync(...)`, or `.Validate(...)` call sites.

**Recommendation**

Either wire the registered validators into the real dialog/service flows or remove the unused DTO/validator path so validation logic stays single-sourced.

## Areas that look solid

- DI and composition-root registration are centralized in `src\Streak.Ui\Misc\ExtensionMethods\MauiAppBuilderExtensions.cs`.
- Backup, restore, and diagnostics flows are generally aligned with the local-first requirements in `docs\specs\README.md`.
- UI decomposition is reasonably healthy overall; the codebase is not broadly suffering from monolithic page components.
