# Quick Add Habit Sheet

> **Trigger**: Global **+** action on the [Habits page](./habits-page.md)

The create habit experience is a **bottom sheet** anchored to the [Habits page](./habits-page.md). It is used for adding a new habit without taking the user away from the primary daily check-in surface.

## Navigation

- Users open the sheet by tapping the **+** icon in the app bar or by tapping **"Add Habit"** from the [Habits page](./habits-page.md) empty state.
- The [Habits page](./habits-page.md) remains visible behind the sheet, dimmed.
- The sheet may be dismissed with an explicit close/cancel action. Android back should dismiss the sheet and return focus to the Habits page.

## Layout

The sheet presents a compact form for creating a habit.

- The top of the sheet includes:
  - a drag handle
  - a compact title such as **"New habit"**
  - a close action
- The required **Name** field appears first.
- The **Emoji** field is optional and visually secondary.
- A lightweight preview card may appear below the fields to show how the habit will appear in the list.

| Field | Type                       | Required | Validation                                                                  |
| ----- | -------------------------- | -------- | --------------------------------------------------------------------------- |
| Name  | Text input                 | Yes      | 1–30 characters. Must be unique among the user's habits (case-insensitive). |
| Emoji | Emoji picker or text input | No       | Single emoji character. If left empty, a default icon is used.              |

## Actions

- **Save**: Creates the habit, dismisses the sheet, and returns the user to the same [Habits page](./habits-page.md) context. The new habit appears in the correct alphabetical position.
- **Cancel / Close**: Dismisses the sheet without creating anything and keeps the user on the [Habits page](./habits-page.md).

## Constraints

| Rule         | Details                                                                                                                 |
| ------------ | ----------------------------------------------------------------------------------------------------------------------- |
| Max habits   | 6. This sheet should not be reachable from the normal UI when the limit is already reached.                            |
| Unique names | Habit names must be unique (case-insensitive). The Save button shows a validation error if a duplicate name is entered. |
| Name length  | 1–30 characters.                                                                                                        |
