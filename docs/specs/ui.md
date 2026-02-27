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
  - Manage habits: `Add` (plus) icon
  - Settings: `Settings` (gear) icon
  - GitHub: `GitHub` brand icon (from MudBlazor icons or a custom SVG)
- **Checkin toggle**: Use MudBlazor's `MudToggleIconButton` for the checkin toggle on the home page. Done = green filled check-circle icon. Not done = empty circle icon.
- Icon sizes follow MudBlazor defaults. App bar icons should be sized at 24dp.

## App Bar

The app uses a **top app bar** that is compact/dense to maximize the content area.

| Position               | Element              | Behavior                                                                                               |
| ---------------------- | -------------------- | ------------------------------------------------------------------------------------------------------ |
| Left                   | "Streak" logo text   | Tapping navigates to the home page. Tooltip on hover: *"Let your habits compound"*.                    |
| Right (1st from right) | GitHub icon          | Opens the [Streak GitHub repository](https://github.com/mithunshanbhag/streak) in an external browser. |
| Right (2nd from right) | Settings icon (gear) | Navigates to the [Settings page](./settings-page.md).                                                  |
| Right (3rd from right) | Add icon (plus)      | Navigates to the [Manage Habits page](./manage-habits-page.md).                                        |

- The app bar is **fixed** at the top and does not scroll with content.
- The app bar is present on **all** pages.

## Navigation

- The app uses a **flat navigation model** with minimal depth:
  - **Home page** is the landing page and the root of the navigation stack.
  - **Trends**, **Manage Habits**, and **Settings** are one level deep from Home.
  - There are no nested pages beyond this.
- Every non-home page displays a **Back arrow** in the app bar (replacing the logo position) that returns the user to the Home page.
- Android hardware/gesture back also returns to the Home page from any secondary screen.
- Navigation transitions should be fast with no perceptible delay.

## Layout

- The app targets **mobile (Android)** screens only. No tablet or desktop layout is needed.
- Content is laid out in a **single column**, full-width, with consistent horizontal padding (16dp).
- Cards (`MudCard`) are used to group related content (e.g., each habit on the home page).
- Vertical spacing between cards: 12dp.

## Empty States

- When there is no content to display (e.g., no habits created), the page shows:
  - A friendly illustration or emoji (e.g., 🌱).
  - A short message (e.g., "No habits yet. Tap + to add your first!").
  - Optionally, a call-to-action button that navigates to the Manage Habits page.
