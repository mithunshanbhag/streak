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
- **Streak number**: Roboto Bold, 32sp — used for the large streak counter on the Habit Details page.

## Iconography

- **Habit icons**: Users may select a single emoji to represent each habit. If no emoji is selected, a default filled-circle icon is used.
- **App bar icons**: Material Design icons from MudBlazor's built-in icon set:
  - Create habit: `Add` (plus) icon
  - Settings: `Settings` (gear) icon
- **Habit detail actions**:
  - Edit habit: `Edit` (pencil) icon
  - Delete habit: `Delete` (bin) icon
- **Checkin toggle**: Use MudBlazor's `MudToggleIconButton` for the checkin toggle on the Habits page. Done = green filled check-circle icon. Not done = empty circle icon.
- Icon sizes follow MudBlazor defaults. App bar icons should be sized at 24dp.

## App Bar

The app uses a **top app bar** that is compact/dense to maximize the content area.

| Position               | Element              | Behavior                                                                                               |
| ---------------------- | -------------------- | ------------------------------------------------------------------------------------------------------ |
| Left                   | "Streak" logo text   | Tapping navigates to the Habits page. Tooltip on hover: *"Let your habits compound"*.                  |
| Right (1st from right) | Settings icon (gear) | Navigates to the [Settings page](./settings-page.md).                                                  |
| Right (2nd from right) | Add icon (plus)      | Opens the [Quick Add Habit sheet](./create-habit-page.md) anchored to the Habits page.                |

- The app bar is **fixed** at the top and does not scroll with content.
- The full `Streak` + `Add` + `Settings` action set is present on the **Habits** page.
- Routed secondary screens may keep a simpler app bar pattern, but the quick-add flow itself does **not** introduce a separate full-screen app bar.

## Navigation

- The app uses a **shallow routed navigation model**:
  - **Habits page** is the landing page and the root of the navigation stack. It is accessible via both `/` and `/habits`.
  - **Habit Details** and **Settings** are one level deep from Habits.
  - **Quick Add Habit** is launched from the global **+** action (and may also be reached from Habits empty-state CTAs) as an anchored bottom sheet.
- Habit edit is performed **inline on the Habit Details page**.
- Habit delete is confirmed with a **dialog launched from the Habit Details page**, not a dedicated route.
- Every non-Habits routed page displays a **Back arrow** in the app bar (replacing the logo position).
  - From **Settings**, the back arrow returns the **Habits** page.
  - From **Habit Details**, the back arrow returns the previous in-app page; if there is no in-app history, it falls back to **Habits**.
  - From the **Quick Add Habit** sheet, Android back dismisses the sheet and returns focus to **Habits**.
- Android hardware/gesture back follows the same route hierarchy.
- Navigation transitions should be fast with no perceptible delay.

## Route Inventory

| Surface        | Route / Trigger     |
| -------------- | ------------------- |
| Habits         | `/`, `/habits`      |
| Habit Details  | `/habits/{habitId}` |
| Quick Add Habit| `+` from Habits     |
| Settings       | `/settings`         |

## Breadcrumbs

- Breadcrumbs are **not required** in the simplified flow.
- Clear page titles plus the app-bar back action are sufficient for navigation.

## Layout

- The app targets **mobile (Android)** screens only. No tablet or desktop layout is needed.
- Content is laid out in a **single column**, full-width, with consistent horizontal padding (16dp).
- Cards (`MudCard`) are used to group related content (e.g., each habit on the Habits page).
- Vertical spacing between cards: 12dp.

## Empty States

- When there is no content to display (e.g., no habits created), the page shows:
  - A friendly illustration or emoji (e.g., 🌱).
  - A short message (e.g., "No habits yet. Tap + to add one.").
  - Optionally, a call-to-action button that opens the Quick Add Habit sheet.
