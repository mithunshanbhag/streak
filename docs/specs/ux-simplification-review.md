# UX Simplification Review

This document captures brainstorming ideas for making Streak feel even lighter.

The current direction is already strong: one primary screen, binary check-ins, an eight-habit cap, and local-only data are all simplicity-friendly choices. The remaining opportunities are mostly about reducing **tap cost** and **visual weight**.

## Decision Update

After review, the following decisions were made:

- **Accepted**: remove the Homepage summary/header chrome so the list starts immediately
- **Accepted**: replace the create habit full-page concept with a compact quick-add dialog
- **Accepted**: favor MudBlazor controls and built-in CSS utilities over bespoke CSS where possible
- **Rejected**: compact Habit Details sheet

The accepted directions now live in the primary `Homepage` and `CreateHabitPage` mockups.

## Goal

Optimize for the two priorities captured during planning:

- **Fewer taps in the daily flow**
- **Less visual clutter**

## What Already Feels Simple

- The app's mental model is small: habits, today's check-in, streaks, reminders.
- The habit limit keeps the list short enough to scan quickly.
- The Homepage is already intended to be the default landing surface.
- Streak logic is binary and easy to understand.

These should remain intact.

## Where the UX Still Feels Heavier Than It Needs To

| Surface                             | Why it still feels heavy                                                                                       | Simplification opportunity                                                     |
| ----------------------------------- | -------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| Homepage header                     | Explanatory copy, status summaries, and decorative chips add scan cost to the most frequently used screen.     | Start directly with the habit list and remove extra header chrome.             |
| App-bar chrome on secondary screens | Global actions on every screen compete with the main task and add visual noise.                                | Show the full action set only on the root screen.                              |
| Create Habit flow                   | A dedicated routed page is clear, but it costs a full navigation step for a tiny form.                         | Consider a compact dialog for quick add.                                       |
| Habit Details layout                | Identity, streak, actions, and heatmap each claim their own visual block, so the page feels longer and busier. | Simplify within the existing page rather than replacing the page with a sheet. |
| Settings                            | Only one real preference exists, but it still occupies a full screen.                                          | Keep the route for clarity or collapse it into a lighter presentation later.   |

## Simplification Ideas

### 1. Quick Wins

These are the safest changes and best fit the current product direction.

#### A. Strip the Homepage down to the essentials

Recommended:

- Remove the separate Homepage summary/header section entirely.
- Remove the visible `A-Z` chip.
- Remove explanatory helper text like "Tap a habit for details. Toggle today on the right."

Why:

- The Homepage is the highest-frequency screen.
- Returning users do not need the interaction model explained on every visit.
- This reduces both reading effort and visual density.

#### B. Keep the full app-bar action set only on the Homepage

Recommended:

- On **Homepage**: keep `Streak`, `+`, and `Settings`.
- On **Habit Details**, **Create Habit**, and **Settings**: show only `Back` + page title.

Why:

- Secondary screens should feel focused.
- Repeating global actions everywhere makes the app bar noisier without helping the core task.

#### C. Keep Habit Details as a page, but simplify within it

Recommended:

- Keep the dedicated Habit Details page.
- Combine identity, streak, and edit access into one compact top section.
- Move destructive actions into an overflow menu or a lower-priority area.

Why:

- The user should not have to visually parse multiple top-level cards before reaching the important information.
- It lowers the feeling of "screen length" even if the same information remains available.

#### D. Hide history by default

Recommended:

- Keep the heatmap, but gate it behind a `Show history` disclosure or make it the second section after the streak summary.

Why:

- Most daily visits do not need history.
- The heatmap is useful but visually dense.

### 2. Medium-Scope Simplifications

These reduce taps more aggressively while preserving the app's core model.

#### A. Replace the Create Habit page with a quick-add dialog

Recommended concept:

- Open `+` into a compact dialog from the Homepage.
- Show a required name field first.
- Keep emoji optional and visually secondary.
- Save returns the user to the same scroll position on the Homepage.

Why:

- Creating a habit becomes a lightweight interruption instead of a route change.
- It keeps the user mentally anchored on the main list.

Trade-off:

- A routed page is easier to deep-link and simpler to implement.
- A compact dialog is closer to MudBlazor's built-in primitives and keeps the flow lightweight without needing a custom sheet treatment.

#### B. Unify create and edit into the same form pattern

Recommended:

- Use the same dialog or compact form structure for both `create` and `edit`.

Why:

- Repeating the same interaction model reduces mental overhead.
- It makes the app feel smaller and more coherent.

### 3. Larger, More Opinionated Ideas

These can simplify the app further, but they also change the feel of the product more significantly.

#### A. Move reminder settings into a lightweight panel instead of a full page

Possible approach:

- Open Settings as a small sheet from the Homepage app bar.

Why:

- There is only one actual setting cluster today.

Trade-off:

- A dedicated route still communicates "this is a real screen" and remains clearer for future growth.

#### B. Show only pending habits by default

Possible approach:

- Display incomplete habits first or hide completed habits behind a compact "Show completed" affordance.

Why:

- It reduces noise later in the day when the user has already checked off most habits.

Trade-off:

- It weakens the stable alphabetical scan pattern.
- Reordering completed items can make the list feel less predictable.

Because Streak already has a very small list size, this should be treated as optional rather than default.

## Recommended Direction

If the goal is to improve the app without overcomplicating the redesign, the strongest next direction is:

### Recommended Package: "Single-Screen First"

1. Keep **Homepage** as the only full-screen view most users need daily.
2. Start the Homepage directly with the habit list.
3. Remove extra app-bar actions from secondary screens.
4. Turn **Create Habit** into a quick-add dialog.
5. Keep **Habit Details** as a dedicated page for now and simplify within that page if needed later.
6. Keep **Settings** as a route for now, but visually reduce it to a single clean card.

Why this package fits Streak:

- It directly targets the user's two priorities.
- It removes navigation weight without changing the core product model.
- It keeps the app feeling calm rather than feature-rich.

## What Should Not Be Simplified Further

These choices are already doing good simplicity work:

- Binary daily check-ins
- Maximum of eight habits
- Local-only storage
- Stable habit naming rules
- A single reminder concept

Removing or weakening these would likely make the product less clear, not more simple.

## Suggested Adoption Order

1. Trim high-frequency screen chrome first.
2. Simplify create/edit interactions second.
3. Re-evaluate whether a full details route is still necessary.
4. Only then decide whether Settings should stay a route or become a lightweight panel.

## Companion Mockups

The accepted directions now live in the primary mockups:

- [`../ui-mockups/Homepage/index.html`](../ui-mockups/Homepage/index.html)
- [`../ui-mockups/CreateHabitPage/index.html`](../ui-mockups/CreateHabitPage/index.html)

The decision summary page lives here:

- [`../ui-mockups/UXSimplificationConcept/index.html`](../ui-mockups/UXSimplificationConcept/index.html)
