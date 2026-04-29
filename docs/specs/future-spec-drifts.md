# Future Drifts

This document captures the current drift assessment from an independent three-reviewer cross-check of `docs\specs` against `docs\ui-mockups`, `tests`, and `src`.

Reviewers:

- Claude Sonnet 4.6
- GPT-5.4
- GPT-5.3-codex

Findings are prioritized first by reviewer consensus, then by product impact.

## Highest-confidence drifts (3 of 3 reviewers)

### Priority 1

#### Habit Details summary is still split across two top-level surfaces

- The [Habit Details spec](./habit-details-page.md) and the matching mockup expect one compact summary card that combines identity, streak, and actions.
- The current implementation keeps identity/description in `src\Streak.Ui\Components\Pages\HabitDetails.razor` and renders streak/history separately through `src\Streak.Ui\Components\Cards\HabitStreakHistoryCard.razor`.
- This remains the clearest layout mismatch on one of the app's primary secondary screens.

#### Habit Details page has no dedicated UI test suite

- `tests\Streak.Ui.UnitTests\Components\Pages` currently contains `HomeTests.cs` and `SettingsTests.cs`, but no `HabitDetails` page suite.
- That leaves the details route, load/not-found states, edit dialog entry, delete flow, and summary rendering without direct page-level bUnit coverage.

#### Check-in dialog closes before the end-to-end save completes

- The check-in dialog spec treats the flow as complete only after proof handling and the final check-in persistence both succeed.
- Today `src\Streak.Ui\Components\Dialogs\CheckinNoteDialog.razor` closes after proof persistence, and `src\Streak.Ui\Components\Pages\Home.razor` performs the final `ToggleForTodayAsync(...)` call afterward.
- If that later save fails, the user loses dialog context and entered input.

### Priority 2

#### Date-sensitive UI and tests still bypass the shared time abstraction

- The product is intentionally local-time-first, but date-sensitive UI should still stay internally consistent.
- `src\Streak.Ui\Components\Pages\Home.razor` uses `DateTime.Now` for the date banner while check-in persistence uses `TimeProvider`.
- `src\Streak.Ui\Components\Pages\HabitDetails.razor` also anchors the heatmap with `DateTime.Now`.
- `tests\Streak.Ui.UnitTests\Components\Pages\HomeTests.cs` still builds at least one expected date from raw `DateTime.Now`, keeping midnight-boundary fragility in the test suite.

#### Check-in picture replacement does not reopen the relevant source flow

- The spec says replacing a selected picture should relaunch the relevant capture or selection flow.
- The current implementation in `src\Streak.Ui\Components\Dialogs\CheckinNoteDialog.razor` routes **Replace** through `PickPhotoAsync()` every time, even when the original source was the camera.

#### Quick Add still behaves like navigation instead of a lightweight overlay return

- The [Quick Add spec](./create-habit-page.md) frames the flow as a compact Homepage dialog that returns the user to the same Homepage context after save.
- `src\Streak.Ui\Components\Dialogs\NewHabitDialog.razor` closes the dialog and then force-navigates back to `/`, which behaves more like a reload than an in-place update.

#### New Habit validation tests are stale relative to the live dialog path

- The repo still has `tests\Streak.Ui.UnitTests\Services\Validators\NewHabitDialogInputModelValidatorTests.cs`.
- The live dialog in `src\Streak.Ui\Components\Dialogs\NewHabitDialog.razor` now performs its own inline validation callbacks instead of using that validator, so those tests no longer cover the active UI path.

## Medium-confidence drifts (2 of 3 reviewers)

### Priority 3

#### Homepage mockup still shows obsolete check-in and undo dialog content

- The detailed dialog specs and dedicated dialog mockups now include picture-proof flows and habit-specific undo copy.
- `docs\ui-mockups\Homepage\index.html` still embeds an older note-only check-in dialog and a generic removal dialog that no longer matches the current dialog specs.

## Downweighted previous items

### Heatmap coverage is not currently a confirmed spec drift

- Existing `HabitHistoryHeatmapBuilder` tests already cover several core heatmap behaviors.
- More coverage could still help, but this is better tracked as a general test-improvement note than as a confirmed spec mismatch.

### Dialog coverage is thinner than page coverage, but not entirely absent

- The repo still lacks dedicated dialog-component test suites.
- However, `HomeTests.cs` already exercises part of the check-in and undo dialog behavior indirectly, so this is not as strong a drift signal as the missing Habit Details page suite.

## Adjacent stale spec note

### `ux-simplification-review.md` still says the habit cap is eight

- The source-of-truth specs and implementation use a 10-habit cap.
- `docs\specs\ux-simplification-review.md` still says eight, so that document should not be treated as current product truth until it is aligned.
