# Manage Habits Page

> **Route**: `/manage-habits`

The manage habits page allows users to **add, edit, delete, and reorder** their habits. Users access this page by tapping the **+** icon in the app bar.

## Navigation

- Accessible from the **+** icon in the app bar (available on all pages).
- A **back arrow** in the app bar returns the user to the [Home page](./home-page.md).

## Breadcrumbs

- Show breadcrumbs near the top of the page.
- Expected trail: **Home / Manage Habits**
- **Home** is tappable.
- **Manage Habits** is the current page and is not tappable.

## Layout

The page displays a **vertical list of existing habits** and an **"Add Habit" button** at the bottom.

### Habit List

Each habit in the list shows:

| Element       | Position    | Details                                                      |
| ------------- | ----------- | ------------------------------------------------------------ |
| Drag handle   | Left        | A grip icon (⠿ or similar) for reordering via drag-and-drop. |
| Emoji / Icon  | Center-left | The habit's emoji, or a default icon.                        |
| Habit name    | Center      | The habit's name label.                                      |
| Edit button   | Right       | An edit (pencil) icon. Navigates to the [Edit Habit page](./edit-habit-page.md) for this habit. |
| Delete button | Far right   | A delete (trash) icon. Navigates to the [Delete Habit Confirmation page](./delete-habit-page.md) for this habit. |

### Add Habit Button

- A prominent button at the bottom of the list: **"+ Add Habit"**.
- Disabled (greyed out) with a tooltip *"Maximum 6 habits reached"* when the user already has 6 habits.
- Tapping the button navigates to the [Create Habit page](./create-habit-page.md).

## Child Pages

Habit CRUD flows remain part of the manage habits area, but they now use dedicated routes instead of dialogs:

| Page                      | Route                             | Purpose                                               |
| ------------------------- | --------------------------------- | ----------------------------------------------------- |
| [Create Habit](./create-habit-page.md) | `/manage-habits/new`              | Create a new habit                                    |
| [Edit Habit](./edit-habit-page.md) | `/manage-habits/{habitId}/edit`   | Edit an existing habit without affecting its history |
| [Delete Habit Confirmation](./delete-habit-page.md) | `/manage-habits/{habitId}/delete` | Confirm destructive deletion                          |

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

| Rule       | Details                                                                          |
| ---------- | -------------------------------------------------------------------------------- |
| Max habits | 6. The "+ Add Habit" button is disabled when the limit is reached.               |
| Reordering | Drag-and-drop changes are persisted immediately and affect the Home page order. |
