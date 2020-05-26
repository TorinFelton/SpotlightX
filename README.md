# WinBar
WinBar is not a replacement for the Windows 10 Search Bar, but an improvement and general aide to the user that will be better at doing certain things than the Search Bar, and will eventually have custom commands that can be designed by the user and executed.

The general motivation behind this was that sometimes the Windows 10 Search Bar can behave strangely and not do what the user would like, or doesn't have the shortcuts quickly available (and searching Bing for them instead...).

## Current Features

- All shortcuts in the System's start menu & the user's start menu will be loaded and can be ran directly from the bar
- Working tab-autocompletion based on available programs/commands
- Minimialistic Icons & No scary error messages
- Hotkey (Alt + S) to bring up WinBar, program runs quietly in the background without interruption and window disappears when not needed
- Search function for Google (though in future you'll be able to change search engine)
- Ability to launch windows settings from simplistic command ("settings <settingpage>"), all settings page names are autocompletable.
  The autocompletion for these setting names has used the ms-settings URI list for all locations ![here](https://github.com/TorinFelton/WinBar/blob/master/CleanUI/CleanUI/config/ms-settings.txt)

## Goals/Upcoming Ideas

- Custom commands implemented via JSON (hopefully with a UI for the user to edit the JSON with)
- Custom actions for these commands to carry out (e.g run program, set setting, search something, etc.)
- Ability to customise icons + style of WinBar (gradient, colouring, etc.)

# General Look
![The bar upon launch looks like this, and will disappear when you click off or press Esc:](https://i.imgur.com/ZsW1MnZ.png)

# Example autocompletion & program run
![When typing in the start of one of the start menu programs, it will autocomplete:](https://i.imgur.com/ei8wNCW.gif)

Pressing enter will run the program, and close the WinBar.

# Search Function + Icon
![Search Function](https://i.imgur.com/DaagPV3.png)

# Launch Settings Function - Supports setting page autocompletion
![Launch Settings Function](https://i.imgur.com/p7wMNS6.gif)

# Error Icon
![Simplistic Error Message - to show invalid command input](https://i.imgur.com/TibVPGY.png)
