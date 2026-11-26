# Kanban board — design foundation

Mode: extension of the open Figma mockup (frame Kanban Board). One product system, not a new brand.

## Typography

- Display / dialog titles / card titles: Newsreader, 500
- UI: DM Sans, 400 / 500 / 600

## Color

- Page surround: `oklch(0.32 0.02 280)`
- Canvas: `oklch(0.91 0.02 75)`
- Surface: `#ffffff`
- Ink: `oklch(0.22 0.02 50)`
- Muted: `oklch(0.52 0.02 60)`
- Line: `oklch(0.9 0.01 80)`
- Accent (add action, links, focus): `oklch(0.52 0.19 255)`
- Column chips: todo `oklch(0.94 0.03 250)`, progress `oklch(0.94 0.04 75)`, done `oklch(0.94 0.04 155)`
- Tags: ux `oklch(0.94 0.03 45)`, ui `oklch(0.94 0.03 290)`, test `oklch(0.94 0.03 155)`, pm `oklch(0.94 0.03 70)`

## Spacing, radius, elevation

- Base unit 4px. Card padding 20px. Dialog padding 28px. Column gap 24px.
- Board radius 28px. Card 14px. Dialog 20px. Controls 10px. Chips 8px.
- Cards: soft warm shadow. Dialogs sit one step higher on a dimmed board.
- Motion: `cubic-bezier(0.22, 1, 0.36, 1)`, 180–220ms. Honor `prefers-reduced-motion`.

## Product rules already on the board

- Top bar: mark, word “Board”, circular add, guest «Войти» or profile chip
- No notes on cards. Edit instead of an assignee
- Muted category tags. Column headers without emoji
- Sample card copy stays in English; window chrome is Russian, matching «Войти»

## Mark

The board mark is a warm gradient tile, not a logo file. Reuse that tile. Do not invent a wordmark.
