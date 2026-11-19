# [WIP] VRChat Udon Player Pickup
Scripts and prefab to allow players to pickup each others, with a few extra options and features!

Current state of the repository is under development, an early release is available, but a few quirks and bugs are to be expected!

# Planned features

## Done

- [X] Hand grab
- [X] Feet grab
- [X] Move to break grab
- [X] Keep grab offset
- [X] Steal players from grab
- [X] Throw players when releasing them

## To Do

- [ ] Option to restrict grab relative to size
- [ ] Consent to be grabbed
- [ ] Consent to grab
- [ ] Make picked up players follow the grabber's limb rotation
- [ ] Allow world creator to chose between grab and use input to avoid collision with other features

## Known Issues
- [ ] When having different players in different hands, releasing one hand will release everyone
- [ ] Latency creates a bigger than necessary offset when grabbing up a player (need the offset to be calculated on grabber's side)

# Installation
## Requirements
- Unity 2022.3.22f1
- VRChat world SDK 3.10.6.

## Steps
1. Add the [package](https://azelrack.github.io/VRChat-AzelVPMListing/) to VRChat Creator Companion.
   - Alternatively you can download the latest release and import the .unitypackage in your project, before jumping to step 3.
2. In the "Manage Project" button on your world project, add the VRChat Udon Player Pickup package.
3. Drop the PlayerPickupHandsFeet Prefab in your scene, or the PlayerPickupHandsOnly if you don't want to allow your players to use their feet to pickup each other.

And that's all, your pickup system is ready to use!

# How to use ?
Use the grab gesture of your controllers to pickup a player, double grab to pick them up with your feet instead, if feet grabbers are installed on world.
Desktop players can use left and right mouse clicks.

# Debug
As only the local player can move himself, the pickup system relies on network events, **you cannot properly test it in the editor**, even with VRC's ClientSim.

That means tesitng it will require using the Build & Test/Reload and VRC Quick launcher to have propper clients with their own local context.