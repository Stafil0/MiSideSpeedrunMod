# MiSide SpeedrunMod
This mod is a tool intended to be used for practicing speedruns or testing theories.

# Features
This mod has multiple features that can be useful for messing around with the game or giving you the chance to practice individual parts.  

## Practice modes
In the main menu you can go to `Settings->Mods->Speedrun Mod->Practice` to play specific parts of the game to practice them.  
These parts will automatically restart when you reach the end.

## Chapter selector
In the in-game pause menu, open `CHAPTERS` to load any playable story chapter.  
Chapters are split across pages (use `NEXT` / `PREVIOUS` at the bottom of each page).  
This always does a full reload (main menu, then load), including the correct chapter entry for shared scenes (e.g. Cappie vs Beyond the World).

## Fast reset
In the main menu you can go to `Settings->Mods->Speedrun Mod->Fast Reset` to configure in-game chapter resets.  
Hold the reset key (default `R`) for the configured time (default `1` second); a preview notification shows what will happen.  
While holding the reset key, use a modifier for other actions (defaults: `Left Shift` = new game, `Left Ctrl` = previous chapter, `Left Alt` = next chapter).  
Leaving the Novels chapter for another chapter always uses a full reload.

## Refresh rate settings
In the main menu you can go to `Settings->Mods->Speedrun Mod->Refresh Rate` to configure a reported refresh rate target.  
Using refresh rate above `540 Hz` will invalidate the run.  
The mod also shows periodic refresh-rate notifications in-game.

## Overlay
In the main menu you can go to `Settings->Mods->Speedrun Mod->Overlay` to configure and inspect the debug overlay.  
By default the overlay toggle hotkey is `F4` (configurable in the overlay menu).

## Trigger reveal (hitboxes)
Press `Alt + O` in-game to toggle visible trigger volumes.  
Each trigger type is drawn with a color-coded shape that matches its real bounds when possible.
If no size can be resolved, a default cube is shown and the label is marked `[default cube]`.  
Volumes stay visible across scene loads while the toggle is on.

## Collider reveal (physics hitboxes)
Press `Alt + H` in-game to cycle visible physics collider volumes.  
Each press advances: primitives (box, sphere, capsule) → mesh colliders → all colliders → off.  
Volumes stay visible across scene loads while a mode is active.

## Texture hide
Press `Alt + T` in-game to cycle texture hiding for route checking.  
Each press advances: primitive colliders → mesh colliders → all colliders → all renderers → restored.  
Hides renderers on matching objects (player and mod visuals are excluded).  
State is restored on the main menu and re-applied after scene loads while active.

## Interactable reveal
Press `Alt + I` in-game to toggle visible interactable volumes.  
Each volume is recoloured every frame to show its current interaction state relative to the player:

- **Purple** — out of range.
- **Blue** — within reach on the ground plane (the game would accept an interaction at this distance).
- **Yellow** — aimed at but not yet interactable.
- **Green** — the game has locked onto it; the interact prompt will fire.

Labels show the live floor distance to the object.

## Softlocks
In the main menu you can go to `Settings->Mods->Speedrun Mod->Softlocks` to toggle Softlock Fixes.  
Softlock Fixes are **on by default**. Use **All Softlocks** as a master gate, or turn individual Softlocks off to bisect Softlock vs Softlock Fix without restarting. Softlock Debug (Debug builds only) follows the same Softlock toggle as its Softlock Fix.

## Fixes
- When pressing the start with a clean slate button the achievements will also be reset if this mod is enabled.
- When toggling `skipdialogue` the mod will remember the state of this toggle. Meaning when you restart the game you will keep this toggle in the state that you had before you quit the game.

## Toggles
 <table>
  <tr>
    <th>Shortcut</th>
    <th>Name</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>Alt + O</td>
    <td>Toggle trigger reveal</td>
    <td>In-game: show or hide color-coded trigger hitbox volumes. See Trigger reveal (hitboxes).</td>
  </tr>
  <tr>
    <td>Alt + H</td>
    <td>Toggle collider reveal</td>
    <td>In-game: cycle color-coded physics collider volumes. See Collider reveal (physics hitboxes).</td>
  </tr>
  <tr>
    <td>Alt + T</td>
    <td>Toggle texture hide</td>
    <td>In-game: cycle texture hiding by collider scope or all renderers. See Texture hide.</td>
  </tr>
  <tr>
    <td>Alt + I</td>
    <td>Toggle interactable reveal</td>
    <td>In-game: show or hide interactable volumes with state colour-coding (purple/blue/yellow/green). See Interactable reveal.</td>
  </tr>
  <tr>
    <td>Alt + L</td>
    <td>Toggle Running</td>
    <td>Toggle whether or not you are allowed to run.</td>
  </tr>
  <tr>
    <td>F1 (configurable)</td>
    <td>Target FPS toggle</td>
    <td>In-game: switch between your configured target FPS and your previous FPS. Rebind under FPS settings.</td>
  </tr>
  <tr>
    <td>F2 (configurable)</td>
    <td>Uncap FPS toggle</td>
    <td>In-game: switch between uncapped FPS with VSync disabled and your previous FPS/VSync settings. Rebind under FPS settings.</td>
  </tr>
  <tr>
    <td>F4 (configurable)</td>
    <td>Overlay toggle</td>
    <td>In-game: toggle overlay on/off. Rebind under Overlay settings.</td>
  </tr>
  <tr>
    <td>R hold (configurable)</td>
    <td>Fast reset</td>
    <td>In-game: hold to restart chapter; combine with modifier keys for previous/next chapter or new game. See Fast Reset settings.</td>
  </tr>
</table> 

# Installing
Before you can install this mod you need to have BepInEx with Il2Cpp support installed, this can be downloaded on their [Bleeding Edge download page](https://builds.bepinex.dev/projects/bepinex_be).  
You then need to extract the zip file to your game directory.  
Then you can download the most recent version on github through the releases section and put `SliceCraft.SpeedrunMod.dll` and `SliceCraft.MenuLib.dll` in the plugin folder.  
You can find this folder at `MiSide/BepInEx/plugins`.

# Contributing
Thanks for being interested in contributing to this mod!  
To setup your dev environment make sure to clone this repository and copy the interop files from your BepInEx folder (should be in `MiSide/BepInEx/interop`) to the `Dependencies` folder.  
Then download the most recent version from the [MenuLib](https://github.com/SliceCraft/MiSideMenuLib/releases) and also place this dll file in the Dependencies folder.  
You can find some good first issues over [here](https://github.com/SliceCraft/MiSideSpeedrunMod/issues?q=is%3Aissue%20state%3Aopen%20label%3A%22good%20first%20issue%22), feel free to ask for help although I won't teach you how to code.

## Build And Deploy
You can clean, build, and copy mod DLLs to your game/plugin folder with `dotnet` commands. Copy step runs only when you pass `DeployOutputDir`.

Default deploy flow:

```sh
dotnet clean -c Release
dotnet build -c Release -p:DeployOutputDir=/path/to/MiSide/BepInEx/plugins
```

By default this deploys:

- `SliceCraft.SpeedrunMod.dll`
- `SliceCraft.MenuLib.dll`

To deploy different files, pass `DeployAssemblies` as comma-separated list:

```sh
dotnet clean -c Release
dotnet build -c Release \
  -p:DeployOutputDir=/path/to/MiSide/BepInEx/plugins \
  -p:DeployAssemblies="SliceCraft.SpeedrunMod.dll,SliceCraft.MenuLib.dll"
```