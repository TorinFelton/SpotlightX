# SpotlightX
![The bar upon launch looks like this, and will disappear when you click off or press Esc:](https://i.imgur.com/ZsW1MnZ.png)


SpotlightX is a minimal, simplistic command bar that makes launching programs or searching things a lot easier than they are with the default Windows 10 search bar. In the future, I intend to go beyond the basic feature set of the Windows 10 search bar and make even more useful commands, while maintaining the simple look and feel of the program throughout (and trying to keep it lightweight).

The general motivation behind this was that sometimes the Windows 10 Search Bar can behave strangely and not do what the user would like, or doesn't have the shortcuts quickly available. For example, even if you type in the same few programs every day to launch, they take a while to be found and sometimes the first result that comes up is a Bing search instead of the program you have installed... SpotlightX doesn't do this, there is no weird determination of results or forcing Bing search down your throat, it just does what you need it to do and disappears after.

## Install
- Alpha V0.1 (Expect bugs!) https://github.com/TorinFelton/SpotlightX/tree/master/CleanUI/SpotlightSetup/Release

## Commands & Basic Usage

#### Press TAB to autocomplete a command or program name to run

### search <something to search>
  
Anything you type after "search" will be searched as a whole.

### <program>
  Typing in a program or a bit of a program name and pressing tab will autocomplete it (then run when pressing enter)
  Programs will only be recognised if they are in the start menu folder for the user or pc.
### settings <setting>
  Setting pages are autocompletable
### exit
  Fully kills the program, instead of just hiding it in the background
  

## Current Features

- All shortcuts in the System's start menu & the user's start menu will be loaded and can be ran directly from the bar
- Working tab-autocompletion based on available programs/commands
- Minimialistic Icons & No scary error messages
- Hotkey (Alt + S) to bring up SpotlightX, program runs quietly in the background without interruption and window disappears when not needed
- Search function for Google (though in future you'll be able to change search engine)
- Ability to launch windows settings from simplistic command ("settings <settingpage>"), all settings page names are autocompletable.
  The autocompletion for these setting names has used the ms-settings URI list for all locations [here](https://github.com/TorinFelton/SpotlightX/blob/master/CleanUI/CleanUI/config/ms-settings.txt)

## Goals/Upcoming Ideas

- Custom commands implemented via JSON (hopefully with a UI for the user to edit the JSON with)
- Custom actions for these commands to carry out (e.g run program, set setting, search something, etc.)
- Ability to customise icons + style of bar (gradient, colouring, etc.)

# General Look


## Example autocompletion & program run
![When typing in the start of one of the start menu programs, it will autocomplete:](https://i.imgur.com/ei8wNCW.gif)
Typing and autocomplete are <b>not</b> slowed by the program, I have just slowly typed for demonstration.


Pressing enter will run the program, and close the window.

## Search Function + Icon
![Search Function](https://i.imgur.com/DaagPV3.png)

## Launch Settings Function
![Launch Settings Function](https://i.imgur.com/p7wMNS6.gif)
Typing and autocomplete are <b>not</b> slowed by the program, I have just slowly typed for demonstration.

## Error Icon
![Simplistic Error Message - to show invalid command input](https://i.imgur.com/TibVPGY.png)
