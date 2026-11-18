# [WIP] VRChat-UdonPlayerPickup
Scripts and prefab to allow players to pickup each others, with a few extra options and features!

Current state of the repository is under development, some things may be broken.

# Planned features

## Done

- [X] Hand pickup
- [X] Feet pickup
- [X] Move to break grab
- [X] Keep grab offset
- [X] Steal players from grab

## To Do

- [ ] Throw players when releasing them
- [ ] Release players from one grabber instead of all at the same time
- [ ] Option to allow grab only relative to size
- [ ] Consent to be picked up
- [ ] Consent to pickup

# Debug
As only the local player can move himself, the pickup system relies on network events, **you cannot properly test it in the editor**, even with VRC's ClientSim.

That means tesitng it will require using the Build & Test/Reload and VRC Quick launcher to have propper clients with their own local context.