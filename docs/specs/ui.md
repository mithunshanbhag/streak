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

- **Font family:** Roboto on Android, resolved from the platform/system font stack rather than an externally hosted web font.
- **Implementation stack:** `Roboto, system-ui, "Segoe UI", Arial, sans-serif`
- **Regular:** 400
- **Medium:** 500
- **Bold:** 700

### Font loading rules

- Do not load Roboto from Google Fonts or any other external font CDN in the production app.
- On Android, the local system Roboto face is the intended default.
- On Windows development builds, allow the stack to fall back to `system-ui` / `Segoe UI`.
- Static mockups should use the same local/system font stack so visual inspection works offline without fetching Roboto.

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
- Check-in and undo-check-in dialog titles should use **Page title**.
- Check-in dialog body copy, warning copy, and empty-preview helper copy should use **Body**.
- Note-field labels, photo metadata rows, and preview helper text should use **Caption** or **Support** depending on prominence.
- Connected-provider names and important backup destinations should use **Body strong**.
- Connected account labels, backup timestamps, and quiet integration metadata should use **Support** or **Caption**.
- Settings card titles should use **Section title** and should lead each card directly without a separate small uppercase eyebrow above them. Smaller internal group titles inside cards such as **Daily automated backups** and **Manual backup** should use **Body strong** rather than repeating the full **Section title** scale for every nested row.
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
| Secondary outlined | `Surface` background, `Primary` text/icon, `BorderStrong` outline, 14sp / 500                                                                   |
| Text button        | Transparent background, `Primary` text, 14sp / 500                                                                                               |
| Destructive filled | `Danger` background, `TextOnPrimary` text                                                                                                        |
| Filled icon action | `Primary` background, `TextOnPrimary` icon, compact square/circular hit area, tooltip or accessible label required when no visible text is shown |
| Pill action        | Use `RadiusPill` only when the interaction is intentionally chip-like or compact                                                                 |

- Icon-only actions must still expose a clear accessible label and a visible tooltip on hover, focus, or press.
- When a row contains multiple related icon-only actions, keep their size, shape, fill, and icon weight visually matched.
- Visible-text buttons are still preferred when an action would otherwise appear without nearby context, but compact icon-only controls are acceptable on already-labeled Settings rows when the surrounding provider text makes the action obvious.
- Use icon-only maintenance actions primarily for compact, already-labeled local action groups such as local backup, diagnostics export/share, restore, and manual cloud backup.
- On the Settings **Cloud backup** provider row, the leading semantic cloud icon may double as the connect / disconnect button:
  - disconnected `CloudOff` in a subtle danger treatment launches the OneDrive OAuth flow
  - connected `CloudDone` in a success/healthy treatment starts the disconnect flow
- Manual OneDrive backup should use the same filled icon-only action treatment as the rest of the Settings maintenance controls, using `Backup` or `CloudUpload` with tooltip / accessible text such as **Back up to OneDrive**.

### Automated backup notifications

- Android automated backup notifications should use the same native backup notification channel for success and failure states so users have one predictable notification permission surface.
- Success notifications should stay calm and confirm the saved destination, for example **Nightly backup complete** with body text that points to `Downloads/Streak/Backups/Automated`.
- Failure notifications should use concise, destination-specific copy rather than generic error text:
  - local failure title: **Nightly backup failed**
  - cloud failure title: **OneDrive backup failed**
  - combined failure title: **Nightly backups failed**
- Failure notification body text should name the next useful action without sounding alarming:
  - local save failure: **Streak could not save the local backup. Open Settings to check backup options.**
  - network failure: **Streak could not reach OneDrive. It will try again tomorrow.**
  - reconnect required: **Reconnect OneDrive to resume cloud backups.**
  - quota exceeded: **OneDrive storage is full. Free up space to resume cloud backups.**
  - access denied or unknown: **Open Settings to check OneDrive backup.**
- When a nightly run has mixed results, the notification treatment should avoid collapsing the outcome into a single success message. For example, if local backup succeeds but OneDrive upload fails, show the cloud failure notification so the user knows cloud backup needs attention.
- Tapping a failure notification should open Streak, preferably to Settings. Tapping a success notification may keep the existing behavior of opening the automated backup folder.
- Failure notifications should be native notifications, not in-app banners, because the automated run may happen while the app is backgrounded or closed.

### Settings page cards

- The Settings page should stack **Daily reminder**, **Local backup**, **Cloud backup**, **Restore**, and **Diagnostic logs** as distinct cards rather than merging low-frequency actions into one large maintenance card.
- Each Settings card should have one primary title at the normal **Section title** scale.
- Settings cards should not use extra uppercase category eyebrows such as **Reminders**, **Backups**, **Maintenance**, or **Support** above the card title; the title itself should carry the hierarchy.
- Related controls inside a card may use one subtle divider, but that divider should organize a single card's content rather than imply that multiple separate sections still belong to one parent card.
- The **Local backup** card may group **Daily automated backups** and **Manual backup** with one quiet divider and **Body strong** internal headings.
- The **Cloud backup** card should keep provider identity, manual backup actions, and the daily automated OneDrive backup toggle visually grouped so the user reads them as one connected cloud-backup area.
- In the connected state, the **Cloud backup** card should usually read as **three stacked subsections** separated by quiet dividers:
  - provider / connection status
  - manual cloud backup
  - daily automated cloud backup
- **Restore** and **Diagnostic logs** should remain standalone cards with the same spacing and action rhythm as the other Settings cards instead of reading like rows inside a larger data container.
- When local and cloud backup cards both surface recency, prefer the same quiet **Last backup** treatment so one card does not feel more instrumented than the other.

### Settings provider and status panels

- Optional connected-service sections such as **OneDrive backup** should use one quiet provider area inside the **Cloud backup** card rather than stacking multiple emphasized nested boxes.
- The default resting provider area should prefer:
  - `SurfaceMuted`
  - a subtle `BorderSubtle` outline when a distinct inset surface helps scanning
  - `RadiusControl` or `RadiusSoft`
  - compact **12–16px** internal padding
- If provider identity, status, metadata, toggle, and actions are all shown together, prefer **one compact panel with internal subsection dividers** rather than detached nested cards for each sub-part.
- The top row should include:
  - one semantic provider status icon on the leading side
  - the provider label (for example **OneDrive**) using **Body strong**
  - the current account identity or neutral support copy using **Support**
- Use one semantic cloud status icon to reinforce the provider state:
  - connected / healthy: `CloudDone`
  - disconnected / signed out: `CloudOff`
  - automated / syncing affordance: `CloudSync`
- The leading provider status icon should expose the state through a tooltip and accessible label such as **Connected** or **Not connected**.
- Avoid pairing that leading status icon with a second trailing chip or duplicate icon+label treatment that repeats the same state.
- Cloud status icons may carry a slightly stronger semantic color than the surrounding copy:
  - connected: `Primary` or `Success`
  - disconnected: `TextSecondary` or a subtle `Danger` treatment
  - syncing / automated state: `Primary`
- Secondary metadata such as **Last backup**, destination details, or backup schedule should use **Caption** or **Support** styling and read as supportive state rather than primary CTA copy.
- Prefer collapsing quiet metadata into **one muted line** when that keeps the panel easier to scan; split into multiple rows only when clarity would suffer.
- When local and cloud backup areas both surface recency, prefer the same quiet **Last backup** treatment so one card does not feel more instrumented than the other.
- Static explanatory details that are already obvious from the section context or tooltip do not need their own always-visible metadata row.
- Connected-state toggles such as **Daily automated OneDrive backup** should usually appear as a simple inline setting row inside the same provider area, not as a separate highlighted panel or sibling card.
- Cloud backup copy should stay concise: prefer one short description per subsection and one quiet metadata line rather than stacked explanatory sentences.
- Disconnected states should show the tappable disconnected status icon plus provider identity, and should avoid rendering disabled secondary controls that are not yet actionable. Visible copy should usually stop at a short status line such as **Not connected**.
- The leading cloud status icon should be an actual button, not decorative chrome. Its tooltip and accessible label should combine both state and action, for example **Not connected. Connect OneDrive** or **Connected. Disconnect OneDrive**.
- When connected, the manual cloud backup trigger should appear as a separate filled icon-only button aligned with the page's other Settings action buttons rather than as a large text CTA row.
- Do not render extra visible-text **Connect OneDrive** or **Disconnect** buttons when the provider row already uses the leading cloud icon as that interaction.
- In the connected state, the manual cloud backup subsection should mirror the **Local backup > Manual backup** rhythm: internal heading, concise one-line description, quiet **Last backup** metadata, then a right-aligned action row.
- In the connected state, the automated cloud backup subsection should mirror the **Local backup > Daily automated backups** rhythm: internal heading, concise one-line description, optional tooltip, then a trailing toggle row without extra hero treatment.

### Dialogs

- Prefer standard `MudDialog` treatments for compact overlay flows instead of custom bottom sheets or full-screen modal patterns.
- Dialog surfaces should use `Surface`, `RadiusCard`, and `ShadowDialog`.
- Dialog headers should stay visually matched across quick-add, edit, delete, restore, check-in, and undo-check-in flows:
  - standard title row with the built-in close affordance
  - at most one short supporting paragraph when supporting copy is needed, and it may be omitted entirely for very small self-explanatory flows
  - no hero-style header treatments, stacked warning badges, or custom header color blocks
- Daily check-in dialogs should stay compact and action-oriented, with the homepage still visible behind a dimmed backdrop.
- Supporting dialog copy should stay brief and scannable. Prefer one sentence over multi-paragraph explanations or repeated helper panels.
- Use standard two- or three-action dialog footers with clear emphasis:
  - secondary text actions for cancel/skip flows
  - primary filled action for the main completion path
  - destructive filled action when the user is confirming data removal
- Destructive confirmation dialogs should keep the surface palette neutral. The destructive CTA is the main place to introduce `Danger`; avoid separate warning-colored icon wells, banners, or chip collections unless the dialog is surfacing a true error state.
- When a dialog is dismissed without confirming the primary action, the underlying homepage check-in state should remain unchanged.

### Form fields

- Use `BorderStrong` for standard field outlines.
- Use **12px** control radius.
- Multiline fields should preserve line breaks and avoid looking like a separate editor product.
- Field labels should use **Caption / 12sp / 400** or equivalent MudBlazor label styling.
- Helper/meta text under fields should use **Caption / 12sp / 400**.
- Short note-entry flows should prefer a standard text field with placeholder text and a compact inline counter rather than stacked label + helper rows when the field purpose is already obvious from context.

### Homepage check-in dialogs

- **Check-in dialog**:
  - opens when the user marks a homepage habit as done
  - uses a compact title; supporting copy is optional and should be omitted when the controls are already self-explanatory
  - should visually match other standard dialogs in the app: title-first header, standard close affordance, normal body copy, and no decorative hero treatment
  - contains one optional plain-text note field capped at **50 characters**
  - should prefer placeholder text such as **"Add a note (optional)"** plus a compact counter instead of a separate note label and helper sentence
  - includes a compact picture-proof area with camera/gallery actions and a single preview control
  - should place camera/gallery actions directly above the preview panel without extra subsection headings or captions
  - should avoid duplicate maintenance affordances in the selected-picture state; one replace action plus a compact remove affordance is enough
  - uses standard outlined inline action buttons for camera/gallery actions and preview maintenance actions, with `RadiusControl` rather than pill/chip styling
  - should offer **Cancel** and **Save check-in** footer actions
  - cancelling or dismissing the dialog leaves the habit unchecked
- **Undo check-in dialog**:
  - opens when the user tries to remove today's check-in from the homepage
  - uses calm but explicit copy warning that the saved note and picture-proof details will be lost from the app's check-in record
  - should stay as compact as delete confirmation dialogs elsewhere in the app: title, one short warning sentence, and footer actions only
  - should not introduce extra warning badges, impact panels, or saved-item chips inside the dialog body
  - should offer **Keep check-in** and **Remove check-in** actions
  - the destructive action should use `Danger`, while the rest of the dialog stays on the standard dialog surface and text palette

### Picture preview control

- The check-in dialog should use a **single-photo preview panel**, not a carousel or gallery grid.
- The preview container should use:
  - `SurfaceMuted` or `SurfaceAccent`
  - `RadiusControl`
  - a subtle `BorderStrong` outline
  - compact internal padding
- The picture action row may appear immediately above the preview panel without a separate **Picture proof** section label.
- Empty state treatment:
  - show a centered image placeholder icon
  - show one short helper line such as *"No picture selected"*
  - keep the panel visually quiet and clearly secondary to the main save action
- Populated state treatment:
  - show a single image preview with a roughly **4:3** rectangular crop area
  - keep corners rounded to match the surrounding control radius
  - show at most one short metadata line underneath or beside the preview, such as the file name
  - expose one compact replace/remove affordance pattern without turning the preview into a standalone browsing surface
- The preview control should feel like a supportive inline form control, not a full media gallery.

### Habit identity and descriptions

- Emoji/icon wells should use `SurfaceAccent`.
- Habit descriptions should appear only on details-oriented surfaces, not on homepage cards.
- Description blocks should use `SurfaceMuted`, a subtle border, and normal body typography with preserved line breaks.

### Date banner

- The homepage date banner should use `PrimarySoft` background with `PrimaryStrong` text.
- It should read as a non-interactive chip/pill, not a CTA.
- Other low-priority metadata surfaces may reuse this same treatment when they should feel informative rather than actionable. The Settings page version/build banner is one such reuse case.

## Iconography

- **Habit icons:** users may select a single emoji to represent each habit. If no emoji is selected, a default filled-circle icon is used.
- **App bar icons:** Material Design icons from MudBlazor's built-in icon set.
  - Settings: `Settings`
  - GitHub repo: `Icons.Custom.Brands.GitHub`
- **Homepage create CTA:** `Add` icon paired with `New Habit`.
- **Settings local data actions:** `Download`, `Share`, and `Upload` for local backup download, local backup share, diagnostics export, diagnostics share, and restore respectively.
- **Settings cloud backup actions:**
  - provider status / connect-disconnect button: `CloudDone` when connected, `CloudOff` when disconnected
  - manual cloud backup action: `Backup` or `CloudUpload`
  - daily automated / sync affordance: `CloudSync`
  - optional cloud retrieval / future restore reference: `CloudDownload`
  - The adjacent provider row should still name **OneDrive** and show account or support copy so the icon buttons are not floating without context.
  - Use semantic color with restraint: connected icons may use `Primary` or `Success`; disconnected icons may use `TextSecondary` or subtle `Danger`; upload/sync actions generally use `Primary`.
- **Habit detail actions:** `Edit` and `Delete`.
- **Checkin toggle:** use MudBlazor's `MudCheckBox` pattern with done/not-done icon states.
- **Check-in dialog proof actions:**
  - camera: `PhotoCamera` or `CameraAlt`
  - gallery / choose existing picture: `PhotoLibrary`
  - empty preview placeholder: `Image`
  - remove selected picture: `Close` or `DeleteOutline`
- The check-in dialog should use icon-leading actions for camera/gallery rather than oversized decorative illustration.

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
- The **Settings** page may reuse this same centered, non-interactive pill styling for app metadata such as **Version {display version} · Build {build number}**.

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
