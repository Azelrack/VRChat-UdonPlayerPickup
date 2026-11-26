# Next Update
## Change
- Disabling the system will get free of any grabber.
## Fix
- Disabling the system with someone in hand could let them stuck.

# 0.3.0 - Grab size difference restriction and important fixes
# New
- Restrict pickup to a minimum size difference (default: grabber needs to be 40% bigger than their target to grab them).
# Change
- Common config stored in one behavior.
# Fix
- Grab contacts would resize to the size of the last player that changed size.
- Updating physics in FixedUpdate as it should.
- Double press and Foot pickup were not working.

# 0.2.1
# Fix
- Updated dependency to Common Tools to fix a missing assembly.

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