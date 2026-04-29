# Future Drifts

This document captures the current drift assessment from an independent three-reviewer cross-check of `docs\specs` against `docs\ui-mockups`, `tests`, and `src`.

Reviewers:

- Claude Sonnet 4.6
- GPT-5.4
- GPT-5.3-codex

Findings are prioritized first by reviewer consensus, then by product impact.

## Resolved since the last assessment

- `docs\specs\ux-simplification-review.md` now matches the 10-habit cap used by the source-of-truth specs and implementation.
- Date-sensitive code and the touched tests now use the shared `TimeProvider` abstraction instead of raw `DateTime.Now`.
- `HabitDetails` now has dedicated page-level bUnit coverage under `tests\Streak.Ui.UnitTests\Components\Pages\HabitDetailsTests.cs`.
- New Habit validation coverage now exercises the live dialog path through `tests\Streak.Ui.UnitTests\Components\Dialogs\NewHabitDialogTests.cs`, and the stale validator-only test file has been removed.

## Highest-confidence drifts (still open)

### Priority 1

#### Habit Details summary is still split across two top-level surfaces

- The [Habit Details spec](./habit-details-page.md) and the matching mockup expect one compact summary card that combines identity, streak, and actions.
- The current implementation keeps identity/description in `src\Streak.Ui\Components\Pages\HabitDetails.razor` and renders streak/history separately through `src\Streak.Ui\Components\Cards\HabitStreakHistoryCard.razor`.
- This remains the clearest layout mismatch on one of the app's primary secondary screens.

#### Check-in dialog closes before the end-to-end save completes

- The check-in dialog spec treats the flow as complete only after proof handling and the final check-in persistence both succeed.
- Today `src\Streak.Ui\Components\Dialogs\CheckinNoteDialog.razor` closes after proof persistence, and `src\Streak.Ui\Components\Pages\Home.razor` performs the final `ToggleForTodayAsync(...)` call afterward.
- If that later save fails, the user loses dialog context and entered input.

### Priority 2

#### Check-in picture replacement does not reopen the relevant source flow

- The spec says replacing a selected picture should relaunch the relevant capture or selection flow.
- The current implementation in `src\Streak.Ui\Components\Dialogs\CheckinNoteDialog.razor` routes **Replace** through `PickPhotoAsync()` every time, even when the original source was the camera.

#### Quick Add still behaves like navigation instead of a lightweight overlay return

- The [Quick Add spec](./create-habit-page.md) frames the flow as a compact Homepage dialog that returns the user to the same Homepage context after save.
- `src\Streak.Ui\Components\Dialogs\NewHabitDialog.razor` closes the dialog and then force-navigates back to `/`, which behaves more like a reload than an in-place update.

## Lower-priority drift being deferred

### Priority 3

#### Homepage mockup still shows obsolete check-in and undo dialog content

- The detailed dialog specs and dedicated dialog mockups now include picture-proof flows and habit-specific undo copy.
- `docs\ui-mockups\Homepage\index.html` still embeds an older note-only check-in dialog and a generic removal dialog that no longer matches the current dialog specs.
- This remains intentionally untouched in the current change.
