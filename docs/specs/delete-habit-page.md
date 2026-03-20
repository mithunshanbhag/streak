# Delete Habit Confirmation Page

> **Route**: `/habits/{habitId}/delete`

The delete habit confirmation page replaces the previous confirmation dialog with a dedicated routed page in the settings-led habit-management flow.

## Navigation

- Users arrive here by tapping the **delete** icon for a habit on the [Settings page](./settings-page.md).
- A **back arrow** in the app bar returns the user to the [Settings page](./settings-page.md).
- Android hardware/gesture back also returns the user to the [Settings page](./settings-page.md).

## Breadcrumbs

- Show breadcrumbs near the top of the page.
- Expected trail: **Home / Delete Habit Confirmation**
- **Home** is a tappable breadcrumb link.
- **Delete Habit Confirmation** is the current page and is not tappable.

## Layout

The page presents a focused destructive-action confirmation surface rather than a modal.

- Message: *"Delete '{habit name}'? All checkin history for this habit will be permanently lost."*
- The selected habit's emoji and name may be repeated near the top of the page to reinforce what is being deleted.

## Actions

- **Delete** button (destructive / red): Deletes the habit and all its associated checkin data, then navigates back to the [Settings page](./settings-page.md).
- **Cancel** button: Returns to the [Settings page](./settings-page.md) without deleting anything.

## Behavior Notes

- This page exists to make destructive navigation explicit and consistent with the rest of the routed app.
- Deleting a habit permanently removes the habit and all of its checkin history.
