# Quick Add Habit Dialog

> **Trigger**: **`+ New Habit`** on the [Homepage](./homepage.md)

The create habit experience is a **compact dialog** launched over the [Homepage](./homepage.md). It is used for adding a new habit without taking the user away from the primary daily check-in surface.

## Navigation

- Users open the dialog by tapping the **`+ New Habit`** CTA below the habit list or by tapping the Homepage empty-state CTA.
- The [Homepage](./homepage.md) remains visible behind the dialog, dimmed.
- The dialog may be dismissed with an explicit close/cancel action. Android back should dismiss the dialog and return focus to the Homepage.

## Layout

The dialog presents a compact form for creating a habit.

- The top of the dialog includes:
  - a compact title such as **"New habit"**
  - a close action
- The required **Name** field appears first.
- The **Emoji** field is optional and visually secondary.
- Prefer `MudDialog`, `MudTextField`, `MudButton`, and `MudPaper` before introducing any custom structure.
- Prefer spacing and border-radius utility classes before adding custom dialog CSS. A small inline width or max-height adjustment is acceptable if needed.

| Field | Type                       | Required | Validation                                                                  |
| ----- | -------------------------- | -------- | --------------------------------------------------------------------------- |
| Name  | Text input                 | Yes      | 1–30 characters. Must be unique among the user's habits (case-insensitive). |
| Emoji | Emoji picker or text input | No       | Single emoji character. If left empty, a default icon is used.              |

## Actions

- **Save**: Creates the habit, dismisses the dialog, and returns the user to the same [Homepage](./homepage.md) context. The new habit appears in the correct alphabetical position.
- **Cancel / Close**: Dismisses the dialog without creating anything and keeps the user on the [Homepage](./homepage.md).

## Constraints

| Rule         | Details                                                                                                                 |
| ------------ | ----------------------------------------------------------------------------------------------------------------------- |
| Max habits   | 6. This dialog should not be reachable from the normal UI when the limit is already reached.                           |
| Unique names | Habit names must be unique (case-insensitive). The Save button shows a validation error if a duplicate name is entered. |
| Name length  | 1–30 characters.                                                                                                        |
