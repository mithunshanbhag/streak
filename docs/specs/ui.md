# Common UI Specifications

This document defines the shared visual language for the Streak app. It is the reference point for mockups and implementation work, so colors, typography, spacing, shapes, and surface treatments should stay aligned with the guidance here unless a later spec explicitly overrides it.

## Design principles

- **Fast to scan:** the UI should privilege habit name, streak state, and the primary action over decorative chrome.
- **Consistent, not clever:** repeated patterns should reuse the same typography, radii, shadows, and control treatments across pages.
- **MudBlazor-first:** prefer MudBlazor components, semantic parameters, and utility classes before introducing custom markup or CSS.
- **Supportive hierarchy:** secondary content such as helper text, descriptions, and maintenance actions should feel calm and visually lighter than the daily check-in flow.
- **Local-time-first:** when the app refers to **today**, **day**, or **daily**, it uses the device's current local date/time rather than UTC.

## Theme behavior

- The app follows the **device system theme** (light or dark mode). There is no in-app theme toggle.
- MudBlazor's theme system remains the implementation source of truth.
- The static HTML mockups in `docs\ui-mockups` use the **light theme token values** below to make inspection straightforward in a browser.

## MudBlazor-first implementation guidance

- Prefer **MudBlazor controls and parameters first** before introducing custom HTML structure or CSS.
- Prefer MudBlazor's built-in semantic parameters such as `Color`, `Variant`, `Typo`, `Size`, and `Dense` over hard-coded visual styling.
- Prefer MudBlazor **CSS utility classes** for layout and spacing rather than page-specific custom CSS:
  - spacing: `pa-*`, `px-*`, `py-*`, `ma-*`, `mx-*`, `my-*`
  - display: `d-flex`, `d-inline-flex`, `d-none`
  - flex sizing: `flex-1`, `flex-auto`, `flex-none`
  - border radius: `rounded`, `rounded-lg`, `rounded-pill`
- Layouts should typically be composed from `MudContainer`, `MudStack`, `MudPaper`, `MudCard`, `MudText`, `MudIconButton`, `MudButton`, and `MudTooltip`.
- For simple long-form copy such as habit descriptions, prefer standard multiline text fields and read-only body text instead of Markdown or rich-text editors.
- Keep bespoke CSS to a minimum. The likely exceptions remain:
  - heatmap cell sizing and positioning
  - fine-tuning the compact Quick Add dialog width / max-height
  - small cosmetic adjustments for static mockup inspection

## Design tokens

### Color system

Use semantic roles rather than one-off hex values.

| Token           | Light value | Usage                                                               |
| --------------- | ----------- | ------------------------------------------------------------------- |
| `Primary`       | `#6200EE`   | App bar, primary filled actions, active time chip, primary emphasis |
| `PrimarySoft`   | `#EDE7F6`   | Tinted chips, date banner backgrounds, soft branded backgrounds     |
| `PrimaryStrong` | `#4527A0`   | Text/icons placed on `PrimarySoft` backgrounds                      |
| `Secondary`     | `#03DAC6`   | History heatmap done cells and secondary accent moments             |
| `Success`       | `#4CAF50`   | Completed check-in states and success accents                       |
| `Warning`       | `#E65100`   | High-energy streak emphasis or warning-style supporting states      |
| `Danger`        | `#D32F2F`   | Destructive actions and delete confirmation emphasis                |
| `Surface`       | `#FFFFFF`   | Cards, dialogs, main surfaces                                       |
| `SurfaceSubtle` | `#FAFAFA`   | Page background behind cards                                        |
| `SurfaceMuted`  | `#F9FAFB`   | Secondary inset panels such as description blocks                   |
| `SurfaceAccent` | `#F6F1FF`   | Emoji containers and subtle branded highlight surfaces              |
| `BorderSubtle`  | `#E0E0E0`   | Dividers and faint separators                                       |
| `BorderStrong`  | `#D1D5DB`   | Input borders and stronger control outlines                         |
| `TextPrimary`   | `#1F1F1F`   | Primary headings, titles, labels                                    |
| `TextSecondary` | `#5F6368`   | Supporting copy, subtitles, streak helper text                      |
| `TextMuted`     | `#757575`   | Eyebrows, captions, low-emphasis metadata                           |
| `TextOnPrimary` | `#FFFFFF`   | Content on primary-filled surfaces                                  |
| `Canvas`        | `#ECECEC`   | Browser/mockup presentation background outside the device frame     |

### Color usage rules

- Do not introduce new page-local accent colors unless there is a semantic reason.
- Default text should use **`TextPrimary`** or **`TextSecondary`**; reserve `TextMuted` for captions, chips, or low-priority metadata.
- Tinted surfaces should come from the primary family (`PrimarySoft`, `SurfaceAccent`) rather than arbitrary purples/greys.
- Destructive UI should use **`Danger`** consistently; warning/help affordances can use **`Warning`** or `TextSecondary` depending on severity.

## Typography

### Font family and weights

- **Font family:** Roboto
- **Regular:** 400
- **Medium:** 500
- **Bold:** 700

### Type scale

| Role           | Size | Weight | Usage                                                                 |
| -------------- | ---- | ------ | --------------------------------------------------------------------- |
| Brand wordmark | 20sp | 700    | Homepage `Streak` logo text                                           |
| Page title     | 20sp | 500    | Routed-page titles and dialog titles                                  |
| Section title  | 20sp | 500    | Primary card or section headers                                       |
| Body strong    | 15sp | 500    | Habit names, major setting labels, CTA text when used in body context |
| Body           | 14sp | 400    | Habit descriptions, dialog body copy, general content                 |
| Body action    | 14sp | 500    | Button labels, filled/text action labels                              |
| Support        | 13sp | 400    | Streak support text, helper rows, supporting descriptions             |
| Caption        | 12sp | 400    | Helper text, field metadata, time/date support                        |
| Eyebrow        | 11sp | 500    | Uppercase section eyebrows and category labels                        |
| Hero number    | 32sp | 700    | Streak count                                                          |

### Typography usage rules

- Homepage habit names should use **Body strong**.
- Supporting streak lines and descriptive copy should use **Support**.
- Optional habit descriptions should render as standard **Body** text with preserved line breaks.
- Eyebrows should stay uppercase with light letter spacing; do not use them for large blocks of copy.
- Avoid mixing 700 weight into ordinary labels unless it is truly a hero or brand moment.

## Shape, spacing, and elevation

### Spacing scale

| Token    | Value | Typical usage                                  |
| -------- | ----- | ---------------------------------------------- |
| `Space2` | 8px   | Tight inline spacing                           |
| `Space3` | 12px  | Internal card spacing, row gaps                |
| `Space4` | 16px  | Standard page padding, card padding            |
| `Space5` | 20px  | Dialog padding, larger section separation      |
| `Space6` | 24px  | Outer mockup framing and roomy overlay padding |

- Standard page padding should stay at **16px**.
- Vertical spacing between habit cards should stay at **12px**.
- Dialogs should feel compact, typically using **18–20px** internal padding.

### Radius scale

| Token           | Value   | Usage                                                   |
| --------------- | ------- | ------------------------------------------------------- |
| `RadiusFrame`   | 24px    | Mock phone frame                                        |
| `RadiusCard`    | 16px    | Cards and dialogs                                       |
| `RadiusControl` | 12px    | Inputs, emoji wells, compact chips                      |
| `RadiusSoft`    | 14px    | Larger inset panels such as the habit description block |
| `RadiusPill`    | `999px` | Chips, pill buttons, rounded badges                     |

### Elevation

| Token          | Value                             | Usage                       |
| -------------- | --------------------------------- | --------------------------- |
| `ShadowFrame`  | `0 12px 32px rgba(0, 0, 0, 0.16)` | Device frame                |
| `ShadowCard`   | `0 2px 6px rgba(0, 0, 0, 0.08)`   | Cards and standard surfaces |
| `ShadowDialog` | `0 16px 32px rgba(0, 0, 0, 0.20)` | Dialog overlays             |

- Avoid inventing new shadow styles per page.
- Use border and subtle tint before adding extra elevation.

## Common component treatments

### App bar

| Property          | Value           |
| ----------------- | --------------- |
| Height            | 48px            |
| Background        | `Primary`       |
| Foreground        | `TextOnPrimary` |
| Icon size         | 24px            |
| Homepage title    | Brand wordmark  |
| Routed-page title | Page title      |

- The app bar is compact by default to maximize content space.
- Homepage uses the `Streak` wordmark; secondary screens use **Back + title only**.

### Cards and surfaces

- Primary cards use `Surface`, `RadiusCard`, and `ShadowCard`.
- Page backgrounds use `SurfaceSubtle`.
- Inset supportive surfaces such as description panels should use `SurfaceMuted` or `SurfaceAccent`, not full card elevation unless they are acting as standalone cards.

### Buttons

| Variant            | Treatment                                                                                                                                        |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| Primary filled     | `Primary` background, `TextOnPrimary` text, 14sp / 500                                                                                           |
| Text button        | Transparent background, `Primary` text, 14sp / 500                                                                                               |
| Destructive filled | `Danger` background, `TextOnPrimary` text                                                                                                        |
| Filled icon action | `Primary` background, `TextOnPrimary` icon, compact square/circular hit area, tooltip or accessible label required when no visible text is shown |
| Pill action        | Use `RadiusPill` only when the interaction is intentionally chip-like or compact                                                                 |

- Icon-only actions must still expose a clear accessible label and a visible tooltip on hover, focus, or press.
- When a row contains multiple related icon-only actions, keep their size, shape, fill, and icon weight visually matched.

### Form fields

- Use `BorderStrong` for standard field outlines.
- Use **12px** control radius.
- Multiline fields should preserve line breaks and avoid looking like a separate editor product.
- Field labels should use **Caption / 12sp / 400** or equivalent MudBlazor label styling.
- Helper/meta text under fields should use **Caption / 12sp / 400**.

### Habit identity and descriptions

- Emoji/icon wells should use `SurfaceAccent`.
- Habit descriptions should appear only on details-oriented surfaces, not on homepage cards.
- Description blocks should use `SurfaceMuted`, a subtle border, and normal body typography with preserved line breaks.

### Date banner

- The homepage date banner should use `PrimarySoft` background with `PrimaryStrong` text.
- It should read as a non-interactive chip/pill, not a CTA.

## Iconography

- **Habit icons:** users may select a single emoji to represent each habit. If no emoji is selected, a default filled-circle icon is used.
- **App bar icons:** Material Design icons from MudBlazor's built-in icon set.
  - Settings: `Settings`
  - GitHub repo: `Icons.Custom.Brands.GitHub`
- **Homepage create CTA:** `Add` icon paired with `New Habit`.
- **Settings data actions:** `Download`, `Share`, and `Upload` for backup download, backup share, and restore respectively.
- **Habit detail actions:** `Edit` and `Delete`.
- **Checkin toggle:** use MudBlazor's `MudCheckBox` pattern with done/not-done icon states.

## Navigation

- The app uses a **shallow routed navigation model**:
  - **Homepage** is the landing page and root.
  - **Habit Details** and **Settings** are one level deep.
  - **Quick Add Habit** opens as a compact dialog over the Homepage.
- Habit edit and delete remain dialog-driven, not routed.
- Android back should follow the same shallow hierarchy.

## Route inventory

| Surface         | Route / Trigger                      |
| --------------- | ------------------------------------ |
| Homepage        | `/`                                  |
| Habit Details   | `/habits/{habitId}`                  |
| Quick Add Habit | `+ New Habit` from Homepage (dialog) |
| Settings        | `/settings`                          |

## Layout

- The product is designed mobile-first.
- Content is single-column with **16px** horizontal padding.
- Use cards to group related content, but keep the overall page visually light.
- Prefer MudBlazor spacing utilities and stack gaps over custom margin rules.
- App bars, routed content, dialog chrome, and bottom CTAs must remain fully visible inside the platform safe drawing area. They must not render underneath Android status bars, navigation bars, or similar system UI overlays.
- Prefer dynamic viewport sizing (`dvh`) and/or host-level safe-area handling for full-height layouts instead of fixed viewport assumptions that can clip content near mobile system bars.

## Date banner

- The **Homepage** displays the current calendar day above the habit list.
- Prefer a `MudChip` or an equivalent pill-style non-interactive treatment.
- Center it horizontally and let it scroll with the page content.
- The displayed date and any same-screen check-in state must agree on the device's current **local** day.

## Time and timezone notes

- Check-in state, streaks, heatmap highlighting, reminder timing, and any other **today**-based UX should all use the device's current **local** date/time.
- UTC should not be used to decide whether a habit is done **today** in the UI.
- Some edge cases are accepted:
  - traveling to another timezone changes what the app considers **today**
  - manually changing the device clock or timezone changes the app's day-based behavior
  - around travel/timezone changes, the app may behave differently from a hypothetical UTC-based timeline, but that is intentional because the product is centered on the user's current local day

## Empty states

- When there is no content to display, use a centered `+ New Habit` action.
- Empty states should stay simple and action-oriented rather than explanatory.
