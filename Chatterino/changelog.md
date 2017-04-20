# changelog

## 1.2.4
- added streamlink support (thanks to cranken1337)
- fixed an issue that caused bttv and ffz global emotes not to load for some users
- made username colors more vibrant
- changed colors for highlights, whispers and resubs

## 1.2.3
- fixed crash when closing splits
- fixed sending whispers from /whispers and /mentions
- fixed the very important typo in the settings

## 1.2.2
- added option for rainbow username colors
- made the "jump to bottom" more obvious
- fixed the reconnecting issue

## 1.2.1
- fixed text being copied twice

## 1.2
- fixed channel ffz emotes being tagged as "global"
- added ap/pm timestamp format
- added "new" cheer badges

## 1.1
- fixed window size resetting to 200x200 px on start

## 1.0.9
- fixed an issue preventing users from starting chatterino
- fixed the icon having a santa hat (when you restart your pc/clean icon cache)

## 1.0.8
- added /r which expands to /w <last user you whispered with>
- added support for ffz emote margins
- chatterino now uses the proper 2x and 4x emotes for ffz and bttv

## 1.0.7.1
- temporarily disabled SoSnowy again because it was causing lag

## 1.0.7
- fixed gif emotes with hats
- fixed hat emotes going over others in the emote list

## 1.0.6
- added support for the bttv christmas emotes (unfortunately SoSnowy does not work)

## 1.0.5
- added emote scaling
- added live indicator to splits
- added button in the user info popup to disable/enable highlights for certain users
- added option to show messages from ignored users if you are mod or broadcaster

- fixes the user info popup going over the screen workspace
- fixed shift + arrow keys not selecting text by characters
- fixed not parting channel when closing split
- fixed copying spaces after emojis

## 1.0.4
- fixed the messages appearing multiple times after switching accounts

## 1.0.3
- disabled hardware acceleration to take less performance when playing games
- now also shows outgoing whispers in chat when inline whispers are enabled
- some messages now don't highlight tabs anymore
- emote list now gets brought to front when you click the button again
- added option to reload channel emotes without restarting
- timeout messages are now bundled

## 1.0.2
- fixed the broken updater, sorry for the inconvenience NotLikeThis

## 1.0.1
- fixed cache being saved to the wrong directory causing bttv emotes not to show

## 1.0
- moved all the settings to %appdata%
- added support for multiple accounts (aka the feature nobody asked for)
- added login via fourtf.com for users that can't open a tcp port
- added /mentions tab (thanks to pajlada)
- fixed gif emotes with 0s frames crashing chatterino
- /whispers got updated

## 0.3.1.1
- added ffz event emotes

## 0.3.1.0
- added option to make the window top-most
- added loyalty subscriber badges
- fixed cheers split up in multiple words
- fixed backgrounds for custom mod badges
- fixed spacing when switching fonts
- improved mouse wheel scrolling very long messages

## 0.3.0.3
- fixed subscriber badges not showing up
- fixed timeout button in the user popup

## 0.3.0.2
- disabled mentioning with @ in commands

## 0.3
- added a slider for the mouse scroll speed
- added option for a manual reconnect
- added a popup when you click on a name
- added an option to ignore messages
- added a rightclick context menu for links
- added an option to mention users with an @
- added twitch prime badge
- fixed emotes in sub messages
- fixed the "gray thing"

## 0.2.6.4
- fixed sub badges not showing up

## 0.2.6.3
- fixed commands not updating when one is deleted

## 0.2.6.2
- added CTRL + 1-9 to select tabs
- added ALT + arrow keys to switch tabs
- added ignored users
- added a settings for the message length counter
- added an emote list popup
- added an option to change the hue of the theme
- changed preferences so all the changes are immediate and cancel reverts them
- fixed tabing localized names
- removed 1 hour emote cache
- tweaked global bad prevention

## 0.2.3
- added FFZ channel emotes
- added a message length counter
- added custom commands
- added a donation page https://streamtip.com/t/fourtf
- fixed timeouts not being displayed sometimes
- fixed ctrl + backspace not deleting a word for some users

## 0.2.2
- added x button to tabs
- add option to disable twitch emotes

## 0.2
- added tabs
- added 4 themes (white and light are still work-in-progress)
- added an option to seperate messages
- added a filter for emotes
- added cheerxxx emotes
- added arrow up/down for previous/next message
- added mouse text selection in the input box

## 0.1.5
- added ratelimiting for messages
- added the option to ignore highlights from certain users
- fixed emote/name tabbing when no completion is available
- fixed subs/resubs
- fixed timeouts not showing up
- fixed name links not being clickable
- fixed a graphics issue with extremely high windows
- esc now closes some dialogs

## 0.1.4
- added twitch bit cheer badges
- replaced irc library with my own irc implementation
- fixed some notices not showing up

## 0.1.3
- added setting to change font
- added custom highlight sounds
- added keyboard shortcuts: ctrl+x (cut text), ctrl+enter (send message without clearing the input), end + home (move to start / end of input)
- added direct write support
- improved performance of word-wrapping
- updated icon (thanks to SwordAkimbo)

## 0.1.2
- fixed text caret disappearing
- improved animated gif draw performance

## 0.1.1
- added a changelog viewer
- made text input prettier
