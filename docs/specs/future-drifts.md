# Future Drifts

This document captures known drift and gap items identified during the cross-check between the current specs, UI mockups, tests, and implementation that are **not** being implemented in the current change.

## Priority 1

### Habit Details layout drift

- The [Habit Details spec](./habit-details-page.md) and the corresponding mockup expect the top summary surface to combine habit identity, current streak, and edit actions in one card.
- The current implementation splits this into:
  - identity and description in `src\Streak.Ui\Components\Pages\HabitDetails.razor`
  - streak and history in `src\Streak.Ui\Components\Cards\HabitStreakHistoryCard.razor`
- This changes the intended hierarchy of one of the app's primary secondary screens.

### Missing Habit Details page UI coverage

- There is currently no dedicated `HabitDetails` component/page test suite under `tests\Streak.Ui.UnitTests\Components\Pages`.
- That leaves edit, delete, load/not-found, and details-page rendering behavior under-covered at the UI layer.

## Priority 2

### Check-in dialog closes before the end-to-end save completes

- The spec treats the check-in dialog flow as complete only after the proof flow succeeds **and** the actual check-in persistence succeeds.
- Today `CheckinNoteDialog.razor` closes after proof persistence, and `Home.razor` performs the final `ToggleForTodayAsync(...)` call after the dialog has already closed.
- If that final save fails, the user loses the dialog context and entered input.

### Check-in picture replacement does not reopen the relevant source flow

- The spec says replacing a selected picture should reopen the relevant capture/selection flow.
- The current implementation always routes **Replace** through gallery selection instead of honoring the last source choice.

### Time-provider inconsistency across date-sensitive UI

- `Home.razor` shows the date banner from `DateTime.Now` while check-in persistence uses the injected `TimeProvider`.
- `HabitDetails.razor` also uses `DateTime.Now` for heatmap "today" state instead of `TimeProvider`.
- This creates avoidable inconsistency in local-time-sensitive UI and makes midnight-boundary/time-based tests harder to control.

### Time-sensitive test fragility in Home tests

- At least one `HomeTests` case builds its expected check-in date using raw `DateTime.Now` instead of a fixed shared `TimeProvider`.
- That makes the test more brittle around midnight boundaries and diverges from the production abstraction.

## Priority 3

### Quick Add flow behaves more like navigation than a lightweight overlay

- The spec frames Quick Add as a compact homepage-centered dialog that returns the user to the same homepage context after save.
- The current dialog force-navigates back to `/` after creation instead of updating the existing homepage state in place.

### New Habit validation coverage is stale relative to the live dialog

- There are validator tests for the new-habit flow, but the live dialog currently performs its own inline validation logic.
- That means some tests exercise a path that the UI is not currently using.

### Dialog component UI coverage is still thin

- The repo has strong Home and Settings page coverage, but no dedicated dialog-component test folder for:
  - `CheckinNoteDialog`
  - `RemoveCheckinDialog`
  - `NewHabitDialog`
  - `EditHabitDialog`
  - `DeleteHabitDialog`
- That leaves several dialog-specific states and validation paths weakly covered.

### Heatmap-specific coverage can be expanded

- The core/view-model heatmap logic has some coverage, but there is still room for stronger boundary and component-level coverage:
  - Monday/week-boundary alignment
  - exact 90-day window edges
  - component behavior around "scroll to latest" / client-side heatmap interaction wiring
