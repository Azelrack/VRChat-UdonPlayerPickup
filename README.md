# [WIP] VRChat Udon Player Pickup
Scripts and prefab to allow players to pickup each others, with a few extra options and features!

Current state of the repository is under development, some things may be broken.

# How to use ?
Drop the PlayerPickupHandsFeet Prefab in your scene, or the PlayerPickupHandsOnly if you don't want to allow people to pickup player with their feet.
Use the grab gesture to pickup a pl

# Planned features

## Done

- [X] Hand grab
- [X] Feet grab
- [X] Move to break grab
- [X] Keep grab offset
- [X] Steal players from grab
- [X] Throw players when releasing them

## To Do

- [ ] Release players from one grabber instead of all at the same time
- [ ] Option to restrict grab relative to size
- [ ] Consent to be grabbed
- [ ] Consent to grab
- [ ] Make picked up players follow the grabber's limb rotation
- [ ] Allow world creator to chose between grab and use input to avoid collision with other features

## Known Issues
- [ ] When having different players in different hands, releasing one hand will release everyone
- [ ] Latency creates a bigger than necessary offset when grabbing up a player (need the offset to be calculated on grabber's side)

# Debug
As only the local player can move himself, the pickup system relies on network events, **you cannot properly test it in the editor**, even with VRC's ClientSim.

That means tesitng it will require using the Build & Test/Reload and VRC Quick launcher to have propper clients with their own local context.