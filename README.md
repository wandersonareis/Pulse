# Pulse

Final Fantasy XIII resource editor.

## Supported games

- Final Fantasy XIII
- Final Fantasy XIII-2
- Lightning Returns: Final Fantasy XIII

## Text

Game texts live in the `txtres` folders (`.ztr` files, exported as `.strings`).
In Lightning Returns: Final Fantasy XIII, texts are also stored inside the
`db/ai/npc/pack` containers.

Pulse reads, edits and re-injects those texts, rebuilding the containers around
the new payload.

## Fonts

The game font sources (`.wfl` files inside the `.win32.imgb` archives) can be
edited, including the glyph tables and their textures.

## Images

Textures are stored as `txbh`/`vtex` records inside WPD archives and as DDS
images inside the `.win32.imgb` archives. Both can be viewed and replaced.

## Other formats

- Music and voice: `scd`
- Animation data: `ykd`
- Archives: encrypted filelists (Final Fantasy XIII-2 and Lightning Returns)
  plus WPD/IMGB containers with optional compression
