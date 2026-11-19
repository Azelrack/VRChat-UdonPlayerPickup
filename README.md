# [WIP] Udon Player Pickup
Scripts and prefab to allow players to pickup each others, with a few extra options and features!

Current state of the repository is under development, an early release is available, but a few quirks and bugs are to be expected!

# Planned features

## Done

- [X] Hand grab
- [X] Feet grab
- [X] Move to break grab
- [X] Keep grab offset
- [X] Steal players from grab
- [X] Consent to be grabbed
- [X] Consent to grab
- [X] *Throw players when releasing them

## To Do

- [ ] Option to restrict grab relative to size
- [ ] Make picked up players follow the grabber's limb rotation
- [ ] Allow world creator to chose between grab and use input to avoid collision with other features

## Known Issues
- [ ] *Throwing a player doesn't works well

# Installation
## Requirements
- Unity 2022.3.22f1
- VRChat world SDK 3.10.6.

## Steps
1. Add the [package](https://azelrack.github.io/VRChat-AzelVPMListing/) to VRChat Creator Companion.
   - Alternatively you can download the latest release and import the .unitypackage in your project, before jumping to step 3.
2. In the "Manage Project" button on your world project, add the Udon Player Pickup package.
3. Load your project and wait for the new packages to be installed.
3. In Runtime -> Prefabs -> Drag N Drop Install drag and drop in your scene the full install or the no feet pickup variant.

And that's all, your pickup system is ready to use!

# How to use ?
Use the grab gesture of your controllers to pickup a player, double grab to pick them up with your feet instead, if feet grabbers are installed on world.
Desktop players can use left and right mouse clicks.

# Debug
As only the local player can move himself, the pickup system relies on network events, **you cannot properly test it in the editor**, even with VRC's ClientSim.

That means tesitng it will require using the Build & Test/Reload and VRC Quick launcher to have propper clients with their own local context.