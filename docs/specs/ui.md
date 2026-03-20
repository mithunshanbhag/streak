# Common UI Specifications

This document covers shared UI conventions that apply across all screens in the Streak app.

## Theme

- The app follows the **device system theme** (light or dark mode). There is no in-app theme toggle.
- MudBlazor's built-in light and dark theme palettes are used as the baseline.

## Color Palette

| Role       | Light Mode              | Dark Mode                   | Usage                                   |
| ---------- | ----------------------- | --------------------------- | --------------------------------------- |
| Primary    | Deep Purple (`#6200EE`) | Deep Purple 200 (`#BB86FC`) | App bar, active toggles, streak badges  |
| Secondary  | Teal (`#03DAC6`)        | Teal 200 (`#03DAC6`)        | Accent highlights, heatmap active cells |
| Surface    | White (`#FFFFFF`)       | Dark Grey (`#121212`)       | Cards, page backgrounds                 |
| On Surface | Black (`#000000`)       | White (`#FFFFFF`)           | Text, icons                             |
| Success    | Green (`#4CAF50`)       | Green 300 (`#81C784`)       | Checked-in / done state                 |
| Muted      | Grey (`#9E9E9E`)        | Grey 600 (`#757575`)        | Not-done state, disabled elements       |

> These are starting-point values. Exact hex codes may be refined during implementation.

## Typography

- **Font family**: Roboto (already included in the project via Google Fonts).
- **Heading**: Roboto Medium, 20sp — used for page titles.
- **Subheading**: Roboto Regular, 16sp — used for section labels.
- **Body**: Roboto Regular, 14sp — used for descriptions, helper text.
- **Caption**: Roboto Regular, 12sp — used for timestamps, secondary info.
- **Streak number**: Roboto Bold, 32sp — used for the large streak counter on the trends page.

## Iconography

- **Habit icons**: Users may select a single emoji to represent each habit. If no emoji is selected, a default filled-circle icon is used.
- **App bar icons**: Material Design icons from MudBlazor's built-in icon set:
  - Create habit: `Add` (plus) icon
  - Settings: `Settings` (gear) icon
  - More actions: `MoreVert` (vertical ellipsis) icon
- **Checkin toggle**: Use MudBlazor's `MudToggleIconButton` for the checkin toggle on the home page. Done = green filled check-circle icon. Not done = empty circle icon.
- Icon sizes follow MudBlazor defaults. App bar icons should be sized at 24dp.

## App Bar

The app uses a **top app bar** that is compact/dense to maximize the content area.

| Position               | Element              | Behavior                                                                                               |
| ---------------------- | -------------------- | ------------------------------------------------------------------------------------------------------ |
| Left                   | "Streak" logo text   | Tapping navigates to the home page. Tooltip on hover: *"Let your habits compound"*.                    |
| Right (1st from right) | More icon            | Opens an overflow menu with the **GitHub** action.                                                      |
| Right (2nd from right) | Settings icon (gear) | Navigates to the [Settings page](./settings-page.md).                                                  |
| Right (3rd from right) | Add icon (plus)      | Navigates to the [Create Habit page](./create-habit-page.md).                                          |

- The app bar is **fixed** at the top and does not scroll with content.
- The app bar is present on **all** pages.
- Overflow menu items:
  - **GitHub** opens the [Streak GitHub repository](https://github.com/mithunshanbhag/streak) in an external browser.

## Navigation

- The app uses a **shallow routed navigation model**:
  - **Home page** is the landing page and the root of the navigation stack.
  - **Trends** and **Settings** are one level deep from Home.
  - **Create Habit**, **Edit Habit**, and **Delete Habit Confirmation** are launched from **Settings** as part of the same maintenance flow, but their URLs do not use a `/settings` prefix.
- Habit management create/edit/delete flows use **regular routed pages**, not dialogs.
- Every non-home page displays a **Back arrow** in the app bar (replacing the logo position).
  - From **Trends** and **Settings**, the back arrow returns the user to the Home page.
  - From **Create Habit**, the back arrow returns the user to the previous in-app page; if there is no in-app history, it falls back to **Settings**.
  - From **Edit Habit** and **Delete Habit Confirmation**, the back arrow returns the **Settings** page.
- Android hardware/gesture back follows the same route hierarchy.
- Navigation transitions should be fast with no perceptible delay.

## Route Inventory

| Page                      | Route                             |
| ------------------------- | --------------------------------- |
| Home                      | `/`                               |
| Trends                    | `/trends/{habitId}`               |
| Create Habit              | `/habits/new`                    |
| Edit Habit                | `/habits/{habitId}/edit`         |
| Delete Habit Confirmation | `/habits/{habitId}/delete`       |
| Settings                  | `/settings`                       |

## Breadcrumbs

- Breadcrumbs are required on the habit-management flow to make the routed CRUD flow explicit.
- Place breadcrumbs near the top of the page content, below the app bar and above the main page heading.
- Breadcrumb labels should match the page names used in the route inventory.
- Expected breadcrumb trails:
  - **Create Habit**: `Home / Create Habit`
  - **Edit Habit**: `Home / Edit Habit`
  - **Delete Habit Confirmation**: `Home / Delete Habit Confirmation`
- Earlier breadcrumb items are tappable links. The current page breadcrumb is not tappable.

## Layout

- The app targets **mobile (Android)** screens only. No tablet or desktop layout is needed.
- Content is laid out in a **single column**, full-width, with consistent horizontal padding (16dp).
- Cards (`MudCard`) are used to group related content (e.g., each habit on the home page).
- Vertical spacing between cards: 12dp.

## Empty States

- When there is no content to display (e.g., no habits created), the page shows:
  - A friendly illustration or emoji (e.g., 🌱).
  - A short message (e.g., "No habits yet. Tap + to add your first!").
  - Optionally, a call-to-action button that navigates to the Create Habit page.
