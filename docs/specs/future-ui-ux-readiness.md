# UI/UX Production Readiness Assessment

This report assesses the current Streak UI/UX direction against the product specs in `docs/specs`, the mockups in `docs/ui-mockups`, and the current Blazor/MudBlazor implementation in `src/Streak.Ui`.

## Executive Summary

Streak has a strong product foundation: a small mental model, shallow navigation, a focused homepage, local-first data, and a consistent enough visual direction to keep the app understandable. The concept is production-worthy.

The current UI/UX is **not production ready yet**. The gap is not a missing redesign; it is the missing production layer around the existing design:

- consistent visual execution across every route, dialog, state, and theme
- verified mobile ergonomics on real Android device sizes
- accessibility and touch usability hardening
- reduced friction in the high-frequency check-in flow
- trustworthy error, loading, permission, and recovery states
- visual regression coverage so future implementation work does not drift from the specs

The app is close enough that the right next step is a focused polish and validation pass, not a broad reimagining.

## What Is Working Well

- **Clear primary job:** The homepage is correctly treated as the daily surface where users check in and leave.
- **Shallow information architecture:** Homepage, Habit Details, Settings, and compact dialogs are appropriate for a lightweight habit tracker.
- **Good simplicity constraints:** Binary check-ins, a hard habit cap, alphabetical ordering, local-only data, and optional proof capture keep scope controlled.
- **Specs are unusually complete:** `ui.md`, route-specific specs, and mockups define a coherent design language and interaction model.
- **MudBlazor-first direction is sensible:** The implementation mostly uses standard controls, which should help accessibility, theming, and maintenance.
- **Several recent UX decisions are right:** Secondary app bars are focused, history is behind disclosure, Settings groups low-frequency actions, and check-in state does not visually commit before confirmation.

## Production Readiness Verdict

**Verdict:** Not production ready.

**Why:** The current app can support core workflows, but it has not yet reached the level of visual consistency, mobile validation, accessibility confidence, and state handling expected from a production mobile app.

**Gap:** A production UX hardening pass. This should include implementation polish, real-device review, accessibility review, screenshot baselines, and explicit acceptance criteria for loading, empty, error, permission, disabled, success, and dark-mode states.

## Highest Priority UX Gaps

### 1. Daily Check-in Flow Has Too Much Default Friction

The app's philosophy says the daily flow should take seconds. The current check-in design opens a dialog for every checked habit, even when the user wants a plain check-in with no note or picture.

This is understandable because notes and picture proof are now part of the model, but it makes the most frequent action slower than the product promise.

**Recommended actions**

- Keep the current dialog, but optimize it for rapid completion:
  - primary button should read as the obvious default action
  - focus and keyboard behavior should support immediate save
  - camera/gallery controls should feel secondary to the save path
- Consider a future quick-check option:
  - tap once to mark done
  - overflow, long-press, or secondary affordance to add note/proof
  - or a user setting such as "Ask for note/photo when checking in"
- Measure the tap count and time-to-complete for checking in 3 to 5 habits with no note/photo.

**Acceptance criteria**

- A no-note/no-photo check-in feels immediate and does not require reading the dialog.
- The dialog does not visually compete with the homepage or make plain check-ins feel like data entry.
- A user can check in several habits in under 10 seconds once familiar with the app.

### 2. Visual Execution Is Not Yet Consistent With The UI Spec

The implementation generally follows the spec, but there are several signs of drift:

- Some colors use unspecified semantic roles such as `Color.Tertiary`, `Color.Warning`, and raw CSS variables instead of the documented design tokens.
- The loading screen uses a separate visual palette and custom gradient treatment that does not match the restrained app theme.
- Some button labels still use all-caps copy such as `SAVE`, while the specs and mockups use calmer sentence-style actions.

**Recommended actions**

- Replace non-spec color roles with documented semantic tokens.
- Align the loading screen with the app palette and reduce decorative visual noise.
- Normalize dialog action labels to sentence case: `Save`, `Save changes`, `Create habit`, `Save check-in`.

**Acceptance criteria**

- Light and dark theme screenshots match the intended design language.
- Button copy and color semantics are consistent across create, edit, check-in, restore, and delete flows.

### 3. Settings Relies Too Heavily On Icon-Only Actions And Tooltips

The Settings data actions are icon-only buttons whose meaning depends on tooltips. That can work on desktop, but tooltips are weak on mobile and less reliable for accessibility. These actions are also high-risk because backup, restore, share, and diagnostics affect user trust.

**Recommended actions**

- Reassess whether data actions should remain icon-only in production.
- Prefer compact action rows with visible text for destructive or trust-critical operations:
  - `Download data`
  - `Share data`
  - `Export logs`
  - `Upload data`
- If icon-only buttons remain, add visible subsection-level labels, disabled-state explanations, and touch-friendly affordances.
- Make disabled platform-specific actions explain why they are disabled without requiring hover.

**Acceptance criteria**

- A touch-only Android user can understand every Settings action without hover.
- Screen reader output makes action purpose, disabled state, and risk clear.
- Restore remains visibly more serious than backup/export without using excessive warning chrome.

### 4. Accessibility Needs A Dedicated Pass

The app has many good accessibility basics: aria labels, standard MudBlazor controls, dialog components, and keyboard handlers. It still needs a structured review before production.

Key risks:

- Habit cards behave like buttons while containing an interactive checkbox, which may create confusing focus and screen-reader behavior.
- Essential help is sometimes only available through tooltips.
- Focus styling and focus order are not explicitly verified.
- Dynamic type / font scaling behavior is not defined.
- Color contrast has not been validated for light and dark themes.
- Heatmap cells are small tap targets and may be difficult for motor accessibility.

**Recommended actions**

- Run a screen-reader pass for Homepage, Check-in Dialog, Habit Details, and Settings.
- Verify focus order, focus visible state, Escape/back behavior, and dialog focus restoration.
- Define minimum touch target sizes for toggles, icon buttons, heatmap cells, and app-bar actions.
- Validate WCAG contrast for text, icons, success states, warning states, and disabled states.
- Decide how the heatmap should behave for users who cannot reliably tap 14px cells.

**Acceptance criteria**

- All primary workflows are usable with keyboard and screen reader.
- All touch targets meet mobile minimum sizing, or have an equivalent accessible interaction.
- No essential meaning depends on color alone, hover alone, or tooltip-only text.

### 5. Error, Loading, Success, And Recovery States Are Underspecified

The happy path is well specified. Production UX needs the uncomfortable states too.

Examples:

- Loading states currently use generic alerts rather than skeletons or lightweight progress states.
- Check-in proof errors can expose raw exception text.
- Export, share, import, and reminder failures use generic error copy without clear recovery guidance.
- Empty state is intentionally minimal, but it does not help a fresh user understand restore/import or the first habit creation moment.
- Offline behavior is a core promise, but the UI does not yet visibly reassure users about local-only/offline operation when relevant.

**Recommended actions**

- Add a shared state model for:
  - loading
  - empty
  - success
  - warning
  - recoverable error
  - destructive confirmation
  - permission denied
  - disabled by platform
- Replace raw exception messages with friendly, actionable copy.
- Add success feedback for check-in, restore, diagnostics export, and reminder/backup preference changes where useful.
- Add retry guidance when actions fail.

**Acceptance criteria**

- No raw technical exception text is shown to end users.
- Every data-impacting action has success, cancel, and failure states.
- Permission-denied states explain what still works and what the user can do next.

### 6. Mockups, Specs, And Implementation Need A Drift Cleanup

The mockups are useful, but some are now out of sync with the current specs and implementation:

- The Homepage mockup still contains explanatory outer copy and an older check-in dialog concept.
- Habit Details mockup says history is expanded only for demonstration, while the production spec says collapsed by default.
- Settings mockup and implementation differ in details around reminder helper copy and visible row structure.
- Separate Check-in Dialog mockups appear newer than the embedded Homepage dialog mockup.

This is not a fatal issue, but it raises the cost of future implementation work because contributors may follow the wrong artifact.

**Recommended actions**

- Pick one source of truth for each surface:
  - route spec for behavior
  - `ui.md` for tokens and shared components
  - mockup for layout/visual reference only
- Update or archive stale mockup sections that conflict with accepted specs.
- Add a short "Mockup status" note to each mockup folder:
  - current
  - partial
  - historical/reference only

**Acceptance criteria**

- A contributor can open any mockup and know whether it is current.
- No current mockup shows a materially different flow from the accepted route spec.

## Medium Priority UX Gaps

### 7. Habit Details Could Be More Scannable

The current direction is sound: identity, description, edit/delete, streak, and history are grouped into a details page. However, the current implementation splits identity/description and streak/history into separate top-level surfaces. This can still feel heavier than necessary on a small phone.

**Recommended actions**

- Consider merging the current streak block into the top summary card so the first viewport communicates identity plus streak immediately.
- Keep history collapsed by default.
- Make destructive delete discoverable but quiet in the overflow menu.

**Acceptance criteria**

- On a typical phone viewport, the user can see habit identity and current streak without scrolling.
- History is available without making the page feel like an analytics dashboard.

### 8. Dark Mode Needs Design-Level Validation

The app supports MudBlazor light/dark palettes, but dark mode is not fully specified at the same level as light mode.

**Recommended actions**

- Add dark-mode token guidance to `ui.md`.
- Capture dark-mode screenshots for Homepage, Check-in Dialog, Habit Details, Settings, and Restore.
- Validate contrast for muted text, disabled controls, heatmap cells, and success/warning states.

**Acceptance criteria**

- Dark mode feels intentionally designed, not just automatically inverted.
- Heatmap, check-in states, and muted helper text remain legible.

### 9. Responsive Behavior Beyond The Primary Phone Width Is Not Proven

The design is mobile-first, but production usage may include small Android devices, large phones, landscape, foldables, and Windows debug/dev use.

**Recommended actions**

- Verify at minimum:
  - 360x640 small phone
  - 393x852 common Android phone
  - 430x932 large phone
  - landscape phone
  - Windows desktop debug window
- Check app-bar title truncation, bottom spacing, dialog height, keyboard overlap, and Settings action layout.

**Acceptance criteria**

- No text overlaps, clips, or becomes unreadable at target viewports.
- Dialogs remain usable when the keyboard is open.
- App bars and bottom actions stay inside safe areas.

### 10. Production UX Needs Visual Regression Coverage

Existing UI tests cover behavior and markup, which is valuable. They do not prove that the app looks right on device-like viewports.

**Recommended actions**

- Add screenshot baselines for the static mockups or rendered app surfaces.
- Add viewport-specific checks for critical screens and dialogs.
- Add a simple accessibility smoke test where feasible.

**Acceptance criteria**

- Pull requests that accidentally break spacing, layout, or visibility are caught before manual review.
- Screenshot coverage exists for the main production surfaces in light and dark themes.

## Suggested Delivery Plan

### Phase 1: Production Polish Sweep

1. Normalize button copy and semantic color usage.
2. Align loading screen visuals with the app theme.
3. Replace raw exception messages with friendly error copy.

### Phase 2: Mobile UX Validation

1. Run the app on Android.
2. Capture screenshots for core routes and dialogs.
3. Validate safe areas, keyboard behavior, small screens, dark mode, and disabled states.
4. Log visual bugs as specific issues.

### Phase 3: Accessibility Hardening

1. Review semantic structure of habit cards and check-in toggles.
2. Validate focus order and dialog focus behavior.
3. Replace tooltip-only essential help where needed.
4. Verify contrast and touch target sizing.

### Phase 4: Frequent-Flow Optimization

1. Time the no-note/no-photo check-in flow.
2. Reduce perceived friction inside the current dialog.
3. Decide whether the product should support one-tap check-in with optional enrichment.

### Phase 5: Design Artifact Cleanup

1. Mark mockups as current, partial, or historical.
2. Update stale embedded mockup dialogs.
3. Add dark-mode and state guidance to `ui.md`.

## Production Readiness Checklist

Before calling the UI production ready, the app should satisfy the following:

- Core daily check-in flow is fast, obvious, and tested on Android.
- Homepage, dialogs, Habit Details, and Settings match the accepted specs.
- All important actions are understandable without hover.
- Light and dark themes are visually validated.
- Empty, loading, error, success, disabled, permission, and destructive states are specified and implemented.
- Data-impacting flows provide clear confirmation, cancellation, failure, and recovery behavior.
- Screen reader and keyboard navigation work for all primary flows.
- Touch targets are adequate on small phones.
- Static mockups and specs no longer conflict.
- Visual regression or screenshot checks cover the main surfaces.

## Bottom Line

Streak does not need a bigger UI. It needs a more dependable one.

The product direction is strong: small, local, fast, and calm. The production gap is the layer of polish and verification that makes those qualities survive real devices, real errors, real accessibility needs, and repeated daily use.
