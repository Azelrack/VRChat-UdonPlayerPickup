# 0.2.0 - UI Toggles & fixes
## New
- UI to control pickup system activation and consent to its various features.
- Added a dependency to a small package of tools I will use with most of my projects.
## Change
- Grabber now sends one network even by player being grabbed, to communicate the offset calculated on grabber's side.
## Fix
- When having different players in different grabbers, releasing one grabber would release all players.
- Latency could implact player offset calculation.


# 0.1.0 - Pickup basics
This is an early release with a few bugs and quirks, but pickup is functional!

## New
- Hand grab.
- Feet grab.
- Move to break grab.
- Keep grab offset.
- Steal players from grab.
- Throw players when releasing them.
