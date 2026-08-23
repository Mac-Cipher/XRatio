---
name: XRatio
description: Native desktop observation console for local tracker interception and controlled torrent simulation.
colors:
  canvas-light: "#F4F6FA"
  topbar-light: "#FFFFFF"
  sidebar-light: "#E9EEF7"
  surface-light: "#FFFFFF"
  raised-light: "#F8FAFD"
  ink-light: "#122034"
  muted-light: "#5C6B7E"
  subtle-light: "#74849A"
  border-light: "#D8E1EC"
  accent-light: "#1D4ED8"
  accent-soft-light: "#E8F0FF"
  positive-light: "#0B8F68"
  positive-soft-light: "#DCF7EB"
  warning-light: "#A66A00"
  danger-light: "#B42333"
  danger-soft-light: "#FDE9EC"
  danger-border-light: "#E9A9B2"
  canvas-dim: "#253247"
  topbar-dim: "#2D3A50"
  sidebar-dim: "#202C40"
  surface-dim: "#31405A"
  raised-dim: "#384963"
  ink-dim: "#F2F6FB"
  muted-dim: "#C0CBD9"
  subtle-dim: "#A7B5C8"
  border-dim: "#52637A"
  accent-dim: "#3B82F6"
  accent-soft-dim: "#273A5B"
  canvas-dark: "#0B1120"
  topbar-dark: "#101827"
  sidebar-dark: "#0D1625"
  surface-dark: "#131F30"
  raised-dark: "#172638"
  ink-dark: "#F4F8FD"
  muted-dark: "#9FB2C8"
  subtle-dark: "#7086A0"
  border-dark: "#25364A"
  accent-dark: "#60A5FA"
  accent-soft-dark: "#172B4D"
  positive-dark: "#61E6B0"
  positive-soft-dark: "#153A34"
  warning-dark: "#F3C56C"
  danger-dark: "#FF7A8A"
  danger-soft-dark: "#40222B"
  danger-border-dark: "#7D3848"
  on-accent-dark: "#07151A"
typography:
  display:
    fontFamily: "Segoe UI Variable Text, Segoe UI, sans-serif"
    fontSize: "29px"
    fontWeight: 700
    lineHeight: 1.1
  headline:
    fontFamily: "Segoe UI Variable Text, Segoe UI, sans-serif"
    fontSize: "25px"
    fontWeight: 700
    lineHeight: 1.15
  title:
    fontFamily: "Segoe UI Variable Text, Segoe UI, sans-serif"
    fontSize: "14px"
    fontWeight: 700
    lineHeight: 1.25
  body:
    fontFamily: "Segoe UI Variable Text, Segoe UI, sans-serif"
    fontSize: "12px"
    fontWeight: 400
    lineHeight: 1.4
  label:
    fontFamily: "Segoe UI Variable Text, Segoe UI, sans-serif"
    fontSize: "9px"
    fontWeight: 700
    lineHeight: 1.2
    letterSpacing: "1.25px"
rounded:
  sm: "4px"
  md: "6px"
  lg: "9px"
spacing:
  xs: "4px"
  sm: "8px"
  md: "14px"
  lg: "18px"
  xl: "30px"
components:
  button-primary:
    backgroundColor: "{colors.accent-light}"
    textColor: "#FFFFFF"
    rounded: "{rounded.sm}"
    padding: "7px 13px"
    height: "36px"
  button-secondary:
    backgroundColor: "{colors.raised-light}"
    textColor: "{colors.ink-light}"
    rounded: "5px"
    padding: "7px 13px"
    height: "36px"
  input:
    backgroundColor: "{colors.raised-light}"
    textColor: "{colors.ink-light}"
    rounded: "{rounded.sm}"
    padding: "7px 11px"
    height: "36px"
  nav-selected:
    backgroundColor: "transparent"
    textColor: "{colors.accent-light}"
    rounded: "0"
    padding: "9px 12px"
    height: "42px"
  panel:
    backgroundColor: "{colors.surface-light}"
    textColor: "{colors.ink-light}"
    rounded: "{rounded.md}"
    padding: "18px"
---

# Design System: XRatio

## Overview

**Creative North Star: “The Local Observation Console”**

XRatio is a native, compact supervision surface for advanced BitTorrent users. It should read like a calm operations instrument: status first, controls close at hand, and enough technical density to verify proxy interception, simulation sessions, and activity without visual noise. The visual language is deliberately desktop-native and functional rather than decorative.

The system uses a pale blue-gray light canvas, a slate middle theme, and a deep navy dark canvas, with fine borders and a blue action voice by default. A turquoise alternative remains available in Settings. All three themes preserve the same hierarchy and geometry while changing the ambient contrast. The observation-console direction is the durable reference for future screens: make runtime state legible in seconds, separate local interception from controlled simulation, and keep critical actions explicit.

**Key Characteristics:**

- Compact observation console with a fixed navigation rail and top control bar.
- Flat, bordered surfaces; depth comes from tonal layering, not shadows.
- Three ambient modes: light, dim slate, and dark navy.
- Blue accent reserved for selection, primary actions, and operational emphasis; the alternate turquoise accent follows the same roles.
- Tabular numerals and terse labels support scanning of runtime metrics.

## Colors

The palette is a cool technical blue-gray system with light, dim slate, and dark navy surfaces, a blue signal color by default, an optional turquoise signal, and semantic green, amber, and red states. Values in the frontmatter are the normative palette from `XRatioPalette`.

### Primary

- **Blue signal** (`accent-light` / `accent-dark`): primary actions, selected navigation, section eyebrows, and operational emphasis. Settings can switch the signal to turquoise without changing hierarchy.
- **Dim slate** (`canvas-dim` / `surface-dim`): the middle-contrast theme for lower ambient brightness without the full dark treatment.

### Secondary

- **Positive green** (`positive-light` / `positive-dark`): healthy/running state and the controlled simulation label.
- **Warning amber** (`warning-light` / `warning-dark`): paused or caution state.
- **Danger red** (`danger-light` / `danger-dark`): startup failure, destructive emphasis, and attention indicators.

### Neutral

- **Canvas:** `canvas-light` / `canvas-dark` is the open work area.
- **Chrome:** `topbar-*` and `sidebar-*` define the top bar and navigation rail.
- **Surfaces:** `surface-*`, `raised-*`, and metric surfaces create restrained tonal steps.
- **Ink:** `ink-*` is primary text; `muted-*` supports descriptions; `subtle-*` is tertiary metadata.
- **Border:** `border-*` is the one-pixel structural divider used throughout.

**The Signal-Rarity Rule.** Use the selected blue/turquoise accent as a signal, not as decoration: it should identify action, selection, or state and remain visually scarce on any given surface.

## Typography

**Display Font:** Segoe UI Variable Text (with Segoe UI fallback)  
**Body Font:** Segoe UI Variable Text (with Segoe UI fallback)  
**Label/Mono Font:** Segoe UI Variable Text with `tnum` for metrics

**Character:** Neutral, compact, and highly legible at desktop densities. Weight and spacing do the hierarchy work; there is no display face. Runtime values use tabular numerals so columns and counters remain stable while changing.

### Hierarchy

- **Display** (bold, 29px, 1.1): primary runtime KPI such as Active, Paused, or Stopped.
- **Headline** (bold, 25px, 1.15): tab title such as Overview.
- **Title** (bold, 14px, 1.25): panel titles and short operating-mode statements.
- **Body** (regular, 12px, 1.4): descriptions, helper text, activity details, and status copy.
- **Label** (bold, 9px, 1.2, 1.25px tracking): uppercase eyebrow labels such as PROXY CHANNEL and OPERATING MODES.

**The Stable-Metrics Rule.** Apply tabular numerals to counters, rates, sizes, timestamps, and other values that update in place; never use decorative numerals that cause layout drift.

## Layout

The window opens at 1280×800 with a 980×640 minimum. A 66px top bar spans the window. Below it, a 216px left rail anchors six destinations: Overview, Interception, Simulation, Activity, Settings, and Platform. The content area stretches beside the rail and scrolls where a surface requires it.

Content uses a compact 4/8 rhythm: common gaps are 8, 12, 14, 16, and 18px; the main Overview surface has 30px outer padding, 18px row padding, 16px column gaps, and a 980px maximum content width. The Overview uses a 1.45:1 split between the runtime card and operating-modes card, with the failure banner and trust note spanning both columns.

Navigation items are at least 200×44px with 8px side insets. Small uppercase section labels establish Monitoring, Control, and System groups; rows stay flat and selection is carried by a compact marker, accent text, and a one-pixel rule rather than a filled card. Preserve the left-rail + top-bar model on future screens; responsive behavior may reduce content density, but must not collapse the distinction between global controls and work-area navigation.

## Elevation & Depth

The system is flat by default. It uses one-pixel borders and tonal surface changes (`canvas` → `surface` → `raised`/`metric`) instead of drop shadows. Dividers are structural and sparse: panel outlines, section rules, and the top-bar/rail boundary. Hover and focus may strengthen an existing border or accent, but should not introduce floating cards or ornamental glow.

**The Flat Console Rule.** Depth communicates containment and hierarchy through fill and border contrast; do not add generic shadows, gradients, or glass effects to native work surfaces.

## Shapes

Forms are gently compact rather than pill-shaped. Standard inputs and panels use 4–6px radii; selected navigation and status badges may use 8–9px. Borders are one pixel. Buttons use a 5px radius, while circular dots communicate live status. Keep corners consistent within a component family and avoid oversized rounding.

## Components

### Buttons

- **Shape:** compact rectangular control with a 5px radius and 36px minimum height.
- **Primary:** teal/cyan fill, on-accent text, semibold 11.5px type, 13×7px padding.
- **Secondary:** raised surface fill with border and ink text; use for reversible or supporting actions such as Open Settings and Pause.
- **Danger:** soft red fill/border for recoverable destructive actions; strong red fill for irreversible emphasis.
- **Quiet:** transparent, borderless text action for low-priority controls such as To tray.
- **Guide footer:** compact icon-only help action anchored to the lower-left of the rail; its circular outline picks up the signal accent on hover without introducing a filled tile.
- **Disabled:** preserve the geometry and hierarchy while reducing contrast; never make state legible by color alone.

### Cards / Containers

- **Runtime card:** white/navy surface, 1px border, 6px radius; metric hero, KPI rows, and reported-upload footer separated by one-pixel rules.
- **Mode card:** raised surface, 1px border, 6px radius; an uppercase eyebrow, a short thesis, and two mode rows with explicit LOCAL / CONTROLLED tags.
- **Internal padding:** 18px for primary content, 14–16px for compact rows, 20px for larger settings sections.

### Inputs / Fields

- **Style:** raised surface, one-pixel border, 4px radius, 36px minimum height, 11×7px padding, 13px text.
- **Focus:** retain the native Avalonia keyboard focus treatment and make the border/accent state clearly visible in both themes.
- **Error / Disabled:** pair semantic red or reduced contrast with explanatory text; do not rely on a red tint alone.

### Navigation

The navigation rail uses one continuous list of six equal-height rows, with Segoe Fluent Icons / Segoe MDL2 Assets at 14px beside semibold 12.5px labels. There are no text dividers between menu items. Unselected rows are transparent with ink text and muted icons; the selected row stays transparent and relies on one native one-pixel selection rule plus accent text and icon color. The top bar carries the XRatio mark, LOCAL / MONITORING context, status badge, and Start/Pause/To tray controls.

### Observation Console

Every operational surface should answer “what is happening, what changed, and what can I do next?” at a glance. Prefer KPI rows, terse state labels, source-aware activity entries, and contextual recovery actions. Keep Interception (real-client local proxy path) and Simulation (independent controlled sessions) visually and semantically distinct.

### States & Accessibility

Represent running, paused, stopped, healthy, warning, and failure states with a color plus a text label or icon. Preserve native Avalonia focus, keyboard navigation, disabled behavior, and minimum target sizes. Maintain readable contrast in both theme variants; do not encode a critical state solely through a dot or color. Keep helper/error text adjacent to the control it explains.

### Localization

Settings exposes a persistent English/French selector. Translation covers navigation, forms, guides, dialogs, activity entries, simulation rows, and runtime status copy; dynamic messages keep their values and remain wrapped instead of being clipped when the French wording is longer.

## Do's and Don'ts

### Do:

- **Do** preserve the fixed top-bar and left-rail observation-console structure.
- **Do** use the exact light/dark palette roles and keep hierarchy invariant across themes.
- **Do** keep borders, spacing, and text compact enough for operational scanning.
- **Do** use explicit labels for proxy, simulation, activity, and recovery states.
- **Do** use tabular numerals for live metrics and retain native focus/keyboard affordances.

### Don't:

- **Don't** turn the desktop console into a marketing dashboard or decorative glassmorphism surface.
- **Don't** merge Interception and Simulation into one ambiguous status stream.
- **Don't** use teal as a general background or apply gradients/shadows without a state-driven reason.
- **Don't** remove textual state labels in favor of color-only indicators.
- **Don't** replace native Avalonia controls with web-shaped interaction patterns.
