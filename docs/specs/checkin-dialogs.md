# Check-in Dialogs

> **Trigger**: Habit toggle on the [Homepage](./homepage.md)

This document defines the two dialog flows that sit between the homepage check-in toggle and the underlying check-in mutation:

1. **Check-in dialog** when the user marks a habit done for today
2. **Undo check-in dialog** when the user removes today's check-in

These dialogs are intentionally compact and homepage-centered. The habit card should not visually commit to the new state until the relevant dialog flow is completed successfully.

## Check-in Dialog

### Purpose

- Collect any optional user input needed before today's check-in is persisted.
- Keep the homepage visible behind a dimmed backdrop so the flow still feels lightweight.
- Allow the user to save a plain check-in, or enrich it with a short note and one optional picture proof.

### Entry Conditions

- Opens when the user toggles an unchecked homepage habit to **done**.
- The originating homepage card remains visually unchanged until the dialog is successfully completed.

### Content

| Element                 | Type                     | Details                                                                                                                                                |
| ----------------------- | ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Dialog title            | Text                     | Uses the selected habit name, for example **"Check in 'Read'"**.                                                                                       |
| Supporting copy         | Body text                | Short explanatory sentence such as *"Add an optional note or picture proof for today's check-in."*                                                     |
| Note field              | Single-line text field   | Optional plain-text note, capped at **50 characters**. Empty is allowed.                                                                               |
| Proof section label     | Caption / support text   | Short label such as **"Picture proof (optional)"**.                                                                                                    |
| Camera action           | Icon-leading button      | Opens the system camera flow to capture a new picture. Available on Android.                                                                           |
| Gallery action          | Icon-leading button      | Opens the system gallery / file picker flow to choose one existing picture. Available on Android and Windows.                                          |
| Picture preview control | Inline preview container | Shows the currently selected picture when present, plus compact metadata and a remove/replace affordance. Empty state is shown when no picture is set. |
| Footer actions          | Buttons                  | **Cancel** and **Save check-in**.                                                                                                                      |

### Behavior

- The note is optional.
- The picture proof is optional.
- At most **one** picture proof may be attached to a single check-in in this iteration.
- The user may save the check-in with:
  - no note and no picture
  - a note only
  - a picture only
  - both a note and a picture
- If a picture is selected, the check-in is considered complete only after:
  1. the image selection/capture succeeds
  2. any required proof-file processing succeeds
  3. the check-in record is persisted successfully
- If picture processing fails, the check-in must remain unchecked and the user should stay in the dialog with a clear error message.
- Dismissing or cancelling the dialog leaves the underlying homepage habit unchecked.

### Picture Preview Control

- The dialog shows a **single-picture preview panel** rather than a gallery grid.
- In the empty state:
  - show a muted inset panel with an image placeholder icon and brief text such as *"No picture selected"*
  - keep the panel visually compact so it does not overwhelm the note field
- In the populated state:
  - show one thumbnail/preview image
  - show a short metadata line beneath or beside it, such as the selected file name
  - allow the user to remove the current picture or replace it with a new capture/selection
- The preview is for confirmation only; it is not a browsing surface and does not navigate away from the dialog.

### Action Outcomes

| User action           | Result                                                                                             |
| --------------------- | -------------------------------------------------------------------------------------------------- |
| Cancel / dismiss      | Close dialog, keep homepage habit unchecked, persist nothing                                       |
| Save check-in         | Persist today's check-in plus any optional note and selected picture-proof metadata                |
| Remove picture        | Clear the current selection from the dialog, but keep the dialog open                              |
| Replace picture       | Launch the relevant capture/selection flow again and update the preview if a new picture is chosen |
| Camera/gallery cancel | Keep the dialog open and preserve the last selected picture state, if any                          |

## Undo Check-in Dialog

### Purpose

- Confirm destructive removal of today's check-in before mutating state.
- Warn that associated note and picture-proof details will also be removed from the app's check-in record.

### Entry Conditions

- Opens when the user toggles a checked homepage habit back to **not done**.
- The originating homepage card remains visually checked until the removal is confirmed.

### Content

| Element        | Type      | Details                                                                                                             |
| -------------- | --------- | ------------------------------------------------------------------------------------------------------------------- |
| Dialog title   | Text      | Uses the habit name, for example **"Remove 'Read' check-in?"**                                                      |
| Warning copy   | Body text | Calm but explicit warning that removing today's check-in also removes any saved note and picture-proof association. |
| Footer actions | Buttons   | **Keep check-in** and **Remove check-in**                                                                           |

### Behavior

- **Keep check-in** closes the dialog and leaves the homepage state unchanged.
- **Remove check-in** deletes today's check-in record and any associated optional note / picture-proof metadata, then updates the homepage card.
- Dismissing the dialog without confirming removal behaves the same as **Keep check-in**.

## Cross-cutting Rules

- Both dialogs should follow the shared guidance in [ui.md](./ui.md).
- Neither dialog should navigate away from the homepage.
- Both dialogs should preserve the user's current local-day semantics; they only operate on **today's** check-in.
- Neither saved notes nor saved picture proofs are shown on the homepage card after completion in this scope.
