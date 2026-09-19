# CelesteArchipelago
This mod is for compatibility with the Celeste Modded archipelago world. It currently supports all of the vanilla game plus strawberry jam.

## How to use
### Mod
1. Get Olympus and Everest for your Celeste installation if you don't have them already
2. Put the .zip into the Mods folder in your Celeste directory
3. Make sure the mod is enabled under "Manage Installed Mods" in Olympus
### APWorld
1. Install the Archipelago launcher
2. Press the "Install APWorld" button and select the celeste_modded apworld file.
3. Press "Generate Template Options" and find the celeste_modded.yaml.
4. Fill out this yaml as desired and send it to your host. Once you have an archipelago server address you can connect to it with the mod.


## Important notes
- In order to keep the mod working consistently, it requires that specific versions of strawberry jam and many of its dependencies be used. This can be handled by Olympus, but may require downgrading some mods.
- Forsaken City A-Side will always be unlocked and included in the archipelago at the start of the game. I could not find a better way to make sphere 1 consistently work, as just about every strawberry jam level requires some mechanic to progress.

## Features
- Archipelago support for every map in strawberry jam and the vanilla game
- Customizable starting levels
- Unlocked levels highlighted in journal
- Up to 177 different in-game mechanics start disabled with items to find that enable them
- Checkpoint and room checks
- Optional toggle for easter egg rooms
- Customizable win condition: Require moon berry, strawberries, and the completion of any heartside/vanilla game ending
- Functions as a vanilla Celeste archipelago: Strawberry jam can be entirely disabled to allow for vanilla-only gameplay

## Planned features
- Better journal tracking, right now it is very difficult to keep track of which levels you can progress in.
- All levels unlocked at the start setting. Should have added this to begin with but I will do it in a future release.
- Binocular checks
- Monika's D-Sides support
- Settings to reduce the number of different mechanics that block progress
- Setting to open all the heart gates/lock the heartsides behind normal level unlock requirements

## Known issues
- After a failed attempt to connect to an AP server, the game will sometimes continue to immediately fail the connection without trying. Restarting should fix this.
- All levels are showing as complete in the journal, even when they aren't

Let me know if you have any suggestions for additions/changes! I can't promise I will implement them all but I am open to hear ideas to make the mod play as well as possible.
