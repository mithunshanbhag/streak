# Create Habit Page

> **Route**: `/habits/new`

The create habit page is a routed page for adding a new habit. It opens from the global **+** action in the app bar and may also be launched from optional add-habit CTAs on the [Habits page](./habits-page.md).

## Navigation

- Users arrive here by tapping the **+** icon in the app bar or by tapping **"Add Habit"** from the [Habits page](./habits-page.md) empty state.
- A **back arrow** in the app bar returns the user to the previous in-app page. If there is no in-app history, it falls back to the [Habits page](./habits-page.md).
- Android hardware/gesture back follows the same behavior.

## Layout

The page presents a simple form for creating a habit.

| Field | Type                       | Required | Validation                                                                  |
| ----- | -------------------------- | -------- | --------------------------------------------------------------------------- |
| Name  | Text input                 | Yes      | 1–30 characters. Must be unique among the user's habits (case-insensitive). |
| Emoji | Emoji picker or text input | No       | Single emoji character. If left empty, a default icon is used.              |

## Actions

- **Save**: Creates the habit and returns to the previous in-app page. The new habit appears on the [Habits page](./habits-page.md) in the correct alphabetical position.
- **Cancel**: Returns to the previous in-app page without creating anything. If there is no in-app history, it falls back to the [Habits page](./habits-page.md).

## Constraints

| Rule         | Details                                                                                                                 |
| ------------ | ----------------------------------------------------------------------------------------------------------------------- |
| Max habits   | 6. This page should not be reachable from the normal UI when the limit is already reached.                             |
| Unique names | Habit names must be unique (case-insensitive). The Save button shows a validation error if a duplicate name is entered. |
| Name length  | 1–30 characters.                                                                                                        |
