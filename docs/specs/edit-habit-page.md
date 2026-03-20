# Edit Habit Page

> **Route**: `/settings/habits/{habitId}/edit`

The edit habit page replaces the previous edit-habit dialog with a dedicated routed page inside the settings hierarchy.

## Navigation

- Users arrive here by tapping the **edit** icon for a habit on the [Settings page](./settings-page.md).
- A **back arrow** in the app bar returns the user to the [Settings page](./settings-page.md).
- Android hardware/gesture back also returns the user to the [Settings page](./settings-page.md).

## Breadcrumbs

- Show breadcrumbs near the top of the page.
- Expected trail: **Home / Settings / Edit Habit**
- **Home** and **Settings** are tappable breadcrumb links.
- **Edit Habit** is the current page and is not tappable.

## Layout

The page uses the same form layout as the create habit page, pre-populated with the selected habit's current values.

| Field | Type                       | Required | Validation                                                                  |
| ----- | -------------------------- | -------- | --------------------------------------------------------------------------- |
| Name  | Text input                 | Yes      | 1–30 characters. Must be unique among the user's habits (case-insensitive), excluding the current habit. |
| Emoji | Emoji picker or text input | No       | Single emoji character. If left empty, a default icon is used.              |

## Actions

- **Save**: Updates the habit and navigates back to the [Settings page](./settings-page.md).
- **Cancel**: Returns to the [Settings page](./settings-page.md) without saving changes.

> Editing a habit's name or emoji does **not** affect its checkin history or streak.

## Constraints

| Rule         | Details                                                                                                                               |
| ------------ | ------------------------------------------------------------------------------------------------------------------------------------- |
| Unique names | Habit names must be unique (case-insensitive), excluding the habit being edited. The Save button shows a validation error otherwise. |
| Name length  | 1–30 characters.                                                                                                                      |
