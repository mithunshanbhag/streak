# Create Habit Page

> **Route**: `/habits/new`

The create habit page replaces the previous add-habit dialog with a dedicated routed page within the settings-led habit-management flow. It is also the direct destination of the global **+** action in the app bar.

## Navigation

- Users arrive here by tapping the **+** icon in the app bar or by tapping **"+ Add Habit"** on the [Settings page](./settings-page.md).
- A **back arrow** in the app bar returns the user to the previous in-app page. If there is no in-app history, it falls back to the [Settings page](./settings-page.md).
- Android hardware/gesture back follows the same behavior.

## Breadcrumbs

- Show breadcrumbs near the top of the page.
- Expected trail: **Home / Settings / Create Habit**
- **Home** and **Settings** are tappable breadcrumb links.
- **Create Habit** is the current page and is not tappable.

## Layout

The page uses the same form content that previously appeared in the add-habit dialog, but presented as a full routed page.

| Field | Type                       | Required | Validation                                                                  |
| ----- | -------------------------- | -------- | --------------------------------------------------------------------------- |
| Name  | Text input                 | Yes      | 1–30 characters. Must be unique among the user's habits (case-insensitive). |
| Emoji | Emoji picker or text input | No       | Single emoji character. If left empty, a default icon is used.              |

## Actions

- **Save**: Creates the habit and returns to the previous in-app page. If launched from the [Settings page](./settings-page.md), the new habit appears at the bottom of that list. If launched from the [Home page](./home-page.md), the new habit appears on Home immediately.
- **Cancel**: Returns to the previous in-app page without creating anything. If there is no in-app history, it falls back to the [Settings page](./settings-page.md).

## Constraints

| Rule         | Details                                                                                                                 |
| ------------ | ----------------------------------------------------------------------------------------------------------------------- |
| Max habits   | 6. This page should not be reachable from the normal UI when the limit is already reached.                             |
| Unique names | Habit names must be unique (case-insensitive). The Save button shows a validation error if a duplicate name is entered. |
| Name length  | 1–30 characters.                                                                                                        |
