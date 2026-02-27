# VS Screen Alerts

A small **universal** code-mod for Vintage Story that allows admins to push **arbitrary text** to players' screens via chat commands.

## Commands (server)

- `/screenmsg <seconds> <text...>` — show to everyone
- `/screenmsgp <playername> <seconds> <text...>` — show to one player
- `/screenmsgclear` — clear for everyone
- `/screenmsgclearp <playername>` — clear for one player

Use `\n` inside the text to insert a line break.

Examples:
- `/screenmsg 10 Hello!\nSecond line`
- `/screenmsgp XRayFun 7 Only for you`
