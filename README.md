# VideoLock

**A live video lock screen for Windows 10 and 11.**

Windows doesn't let you put a video on the lock screen. VideoLock gets you the same experience anyway: your own video plays full-screen with a lock-screen-style clock, and the moment you touch the mouse or keyboard, Windows locks and takes you straight to the PIN/password box.

> 📸 *Add a screenshot or GIF here:*  ' ![Uploading WindowsLW-1.gif…]()
'

---

## Features

- 🎬 **Your own video**: any MP4 plays full-screen, looping and muted
- 🖥️ **Every monitor**: the video covers all screens
- 🕒 **Lock-screen clock**: big time and date in your PC's own regional format
- 🔒 **Real Windows security**: touching anything triggers the actual Windows lock, so your password still protects everything
- 🚫 **No desktop flash**: the video stays up until Windows confirms the lock screen is showing
- ⚡ **Straight to password**: optionally skips the static lock picture, on both Home and Pro
- ⌨️ **Hotkey**: press **Ctrl+Alt+L** to lock with the video instantly
- 📦 **Nothing to download**: compiled on your PC by the C# compiler already built into Windows

## Installation

1. Download this repository (**Code → Download ZIP**) and extract it.
2. Double-click **`install.bat`**.
3. Pick your video when asked (H.264 MP4 works best).

That's it. VideoLock starts after **5 minutes** of idle time, or whenever you press **Ctrl+Alt+L**.

### Optional: go straight to the password box

Right-click **`optional-admin-tweaks.bat`** and choose **Run as administrator**, then restart. This:

- Skips the static lock screen picture, so clicking the video takes you directly to the PIN/password box
- Removes the 5-second grace period in which a screen saver can be dismissed without a password

## How it works

Windows draws its real lock screen on a protected, separate desktop that no app is allowed to draw on. So instead of hacking it, VideoLock works with Windows:

1. It's installed as a **secure screen saver** (`.scr`), which Windows launches when you're idle.
2. It plays your video plus a clock on every monitor.
3. On any input (mouse, keyboard, touch, Win key, Alt+Tab, Ctrl+Alt+Del), it calls Windows' own `LockWorkStation()`.
4. It waits until Windows reports the session is locked, and only then closes. That's why nothing behind it ever flashes.

The optional tweak uses the official `NoLockScreen` policy on Pro, plus a small scheduled task that sets `AllowLockScreen = 0` at every sign-in and unlock, which also makes it work on Home.

## Changing settings

| What | How |
|---|---|
| Change the video | Run `install.bat` again, or open **Screen saver settings → VideoLock → Settings** |
| Change the idle time | **Settings → Personalization → Lock screen → Screen saver** |
| Change the hotkey | Right-click **VideoLock Now** in the Start menu → **Open file location** → Properties → Shortcut key |

## Uninstall

Run **`uninstall.bat`**. To undo the admin tweaks, run the commands listed at the bottom of that file as administrator.

## Good to know

- **Idle vs. hotkey**: when started by idle time, Windows treats VideoLock as a secure screen saver. When started with Ctrl+Alt+L, your PC locks the instant someone touches it. For walking away in a public place, plain **Win+L** is the safest option.
- **Display sleep**: if your monitor turns off before the screen saver starts, you won't see the video. Adjust this in **Settings → System → Power**.
- **Video formats**: H.264 MP4 works everywhere. Some HEVC/MKV files need extra codecs.
- **Battery**: playing video uses more power than a static lock screen.

## Requirements

- Windows 10 or 11 (Home or Pro)
- .NET Framework 4.x (already included with Windows)

## License

[MIT](LICENSE). Use it, change it, share it.
