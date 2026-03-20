# Delete Habit Confirmation Page

> **Route**: `/manage-habits/{habitId}/delete`

The delete habit confirmation page replaces the previous confirmation dialog with a dedicated routed page in the manage habits flow.

## Navigation

- Users arrive here by tapping the **delete** icon for a habit on the [Manage Habits page](./manage-habits-page.md).
- A **back arrow** in the app bar returns the user to the [Manage Habits page](./manage-habits-page.md).
- Android hardware/gesture back also returns the user to the [Manage Habits page](./manage-habits-page.md).

## Breadcrumbs

- Show breadcrumbs near the top of the page.
- Expected trail: **Home / Manage Habits / Delete Habit Confirmation**
- **Home** and **Manage Habits** are tappable breadcrumb links.
- **Delete Habit Confirmation** is the current page and is not tappable.

## Layout

The page presents a focused destructive-action confirmation surface rather than a modal.

- Message: *"Delete '{habit name}'? All checkin history for this habit will be permanently lost."*
- The selected habit's emoji and name may be repeated near the top of the page to reinforce what is being deleted.

## Actions

- **Delete** button (destructive / red): Deletes the habit and all its associated checkin data, then navigates back to the [Manage Habits page](./manage-habits-page.md).
- **Cancel** button: Returns to the [Manage Habits page](./manage-habits-page.md) without deleting anything.

## Behavior Notes

- This page exists to make destructive navigation explicit and consistent with the rest of the routed app.
- Deleting a habit permanently removes the habit and all of its checkin history.
