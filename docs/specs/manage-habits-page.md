# Manage Habits Page

> **Route**: `/manage-habits`

The manage habits page allows users to **add, edit, delete, and reorder** their habits. Users access this page by tapping the **+** icon in the app bar.

## Navigation

- Accessible from the **+** icon in the app bar (available on all pages).
- A **back arrow** in the app bar returns the user to the [Home page](./home-page.md).

## Layout

The page displays a **vertical list of existing habits** and an **"Add Habit" button** at the bottom.

### Habit List

Each habit in the list shows:

| Element       | Position    | Details                                                      |
| ------------- | ----------- | ------------------------------------------------------------ |
| Drag handle   | Left        | A grip icon (⠿ or similar) for reordering via drag-and-drop. |
| Emoji / Icon  | Center-left | The habit's emoji, or a default icon.                        |
| Habit name    | Center      | The habit's name label.                                      |
| Edit button   | Right       | An edit (pencil) icon. Opens the edit dialog for this habit. |
| Delete button | Far right   | A delete (trash) icon. Triggers the delete confirmation.     |

### Add Habit Button

- A prominent button at the bottom of the list: **"+ Add Habit"**.
- Disabled (greyed out) with a tooltip *"Maximum 6 habits reached"* when the user already has 6 habits.
- Tapping the button opens the **Add Habit Dialog**.

## Add Habit Dialog

A modal dialog (MudDialog) with the following fields:

| Field | Type                       | Required | Validation                                                                  |
| ----- | -------------------------- | -------- | --------------------------------------------------------------------------- |
| Name  | Text input                 | Yes      | 1–30 characters. Must be unique among the user's habits (case-insensitive). |
| Emoji | Emoji picker or text input | No       | Single emoji character. If left empty, a default icon is used.              |

**Buttons**:

- **Save**: Creates the habit and closes the dialog. The new habit appears at the bottom of the list on the home page.
- **Cancel**: Closes the dialog without creating anything.

## Edit Habit Dialog

Same layout as the Add Habit Dialog, but pre-populated with the existing habit's name and emoji.

- **Save**: Updates the habit and closes the dialog.
- **Cancel**: Closes the dialog without saving changes.

> Editing a habit's name or emoji does **not** affect its checkin history or streak.

## Delete Habit

- Tapping the delete icon shows a **confirmation dialog**:
  - Message: *"Delete '{habit name}'? All checkin history for this habit will be permanently lost."*
  - **Delete** button (destructive / red): Deletes the habit and all its associated checkin data.
  - **Cancel** button: Closes the dialog without deleting.

## Reorder Habits

- Users can **drag and drop** habits in the list to change their display order on the home page.
- The new order is persisted immediately.
- Use MudBlazor's `MudDropZone` or a similar drag-and-drop component.

## Empty State

When the user has no habits:

- Display a friendly message: *"You haven't added any habits yet."*
- Show a 🌱 emoji above the message.
- The **"+ Add Habit"** button is still visible and active.

## Constraints

| Rule         | Details                                                                                                                 |
| ------------ | ----------------------------------------------------------------------------------------------------------------------- |
| Max habits   | 6. The "Add Habit" button is disabled when the limit is reached.                                                        |
| Unique names | Habit names must be unique (case-insensitive). The Save button shows a validation error if a duplicate name is entered. |
| Name length  | 1–30 characters.                                                                                                        |
