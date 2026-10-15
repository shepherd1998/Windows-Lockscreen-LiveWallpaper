// VideoLock - a video "live lock screen" for Windows 10/11.
// Built as a screen saver (.scr). Plays a looping, muted video on every monitor,
// and the moment you touch the mouse/keyboard it calls the real Windows lock.
//
//   VideoLock.scr /s   -> run (fullscreen video)
//   VideoLock.scr /c   -> choose the video file
//   VideoLock.scr /p   -> preview (ignored)
//
// Compiled by install.bat with the C# compiler that ships with Windows (.NET Framework 4.x).

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WinForms = System.Windows.Forms;

class VideoLock
{
    [DllImport("user32.dll")]
    static extern bool LockWorkStation();

    static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VideoLock");
    static readonly string ConfigFile = Path.Combine(ConfigDir, "video.txt");

    static Application app;
    static bool exiting = false;

    [STAThread]
    static void Main(string[] args)
    {
        string mode = args.Length > 0 ? args[0].Trim().ToLowerInvariant() : "/c";
        if (mode.StartsWith("/p")) return;
        if (mode.StartsWith("/c")) { Configure(); return; }
        Run();
    }

    static void Configure()
    {
        var dlg = new WinForms.OpenFileDialog();
        dlg.Title = "VideoLock - choose your lock screen video";
        dlg.Filter = "Video files|*.mp4;*.m4v;*.wmv;*.mov;*.avi|All files|*.*";
        if (dlg.ShowDialog() == WinForms.DialogResult.OK)
        {
            Directory.CreateDirectory(ConfigDir);
            File.WriteAllText(ConfigFile, dlg.FileName);
            WinForms.MessageBox.Show("Video saved:\n" + dlg.FileName, "VideoLock");
        }
    }

    static void Run()
    {
        string video = File.Exists(ConfigFile) ? File.ReadAllText(ConfigFile).Trim() : "";

        app = new Application();
        app.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        double scale;
        using (var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero)) scale = g.DpiX / 96.0;

        var windows = new List<Window>();
        Window primary = null;
        foreach (var screen in WinForms.Screen.AllScreens)
        {
            var w = MakeWindow(screen.Bounds, scale, video);
            windows.Add(w);
            if (screen.Primary) primary = w;
        }
        foreach (var w in windows) w.Show();
        if (primary != null) primary.Activate();

        app.Run();
    }

    static Window MakeWindow(System.Drawing.Rectangle b, double scale, string video)
    {
        var w = new Window();
        w.WindowStyle = WindowStyle.None;
        w.ResizeMode = ResizeMode.NoResize;
        w.Topmost = true;
        w.ShowInTaskbar = false;
        w.Background = Brushes.Black;
        w.Cursor = Cursors.None;
        w.WindowStartupLocation = WindowStartupLocation.Manual;
        w.Left = b.Left / scale;
        w.Top = b.Top / scale;
        w.Width = b.Width / scale;
        w.Height = b.Height / scale;
        w.Loaded += delegate { w.WindowState = WindowState.Maximized; };

        var grid = new Grid();
        grid.Background = Brushes.Black;
        if (video.Length > 0 && File.Exists(video))
        {
            var me = new MediaElement();
            me.Source = new Uri(video);
            me.LoadedBehavior = MediaState.Manual;
            me.UnloadedBehavior = MediaState.Manual;
            me.IsMuted = true;
            me.Stretch = Stretch.UniformToFill;
            me.MediaEnded += delegate { me.Position = TimeSpan.Zero; me.Play(); };
            me.Loaded += delegate { me.Play(); };
            grid.Children.Add(me);
        }
        grid.Children.Add(MakeClock(w));
        w.Content = grid;

        // Any real input -> lock.
        Point? start = null;
        w.MouseMove += delegate(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(w);
            if (start == null) { start = p; return; }
            if (Math.Abs(p.X - start.Value.X) > 10 || Math.Abs(p.Y - start.Value.Y) > 10) Exit();
        };
        w.MouseDown += delegate { Exit(); };
        w.MouseWheel += delegate { Exit(); };
        w.KeyDown += delegate { Exit(); };
        w.TouchDown += delegate { Exit(); };

        // Focus leaves all our windows (Win key, Ctrl+Alt+Del, Alt+Tab...) -> lock.
        w.Deactivated += delegate
        {
            w.Dispatcher.BeginInvoke(new Action(delegate
            {
                foreach (Window x in app.Windows) if (x.IsActive) return;
                Exit();
            }), DispatcherPriority.Background);
        };

        return w;
    }

    // Big time + date over the video, styled like the Windows 11 lock screen.
    static UIElement MakeClock(Window w)
    {
        var font = new FontFamily("Segoe UI Variable Display, Segoe UI");
        double h = w.Height;

        var time = new TextBlock();
        time.FontFamily = font;
        time.FontWeight = FontWeights.SemiBold;
        time.FontSize = Math.Max(72, h * 0.12);
        time.Foreground = Brushes.White;
        time.HorizontalAlignment = HorizontalAlignment.Center;

        var date = new TextBlock();
        date.FontFamily = font;
        date.FontSize = Math.Max(20, h * 0.025);
        date.Foreground = Brushes.White;
        date.HorizontalAlignment = HorizontalAlignment.Center;

        var clock = new StackPanel();
        clock.HorizontalAlignment = HorizontalAlignment.Center;
        clock.VerticalAlignment = VerticalAlignment.Top;
        clock.Margin = new Thickness(0, h * 0.10, 0, 0);
        clock.IsHitTestVisible = false;
        clock.Effect = new System.Windows.Media.Effects.DropShadowEffect
            { BlurRadius = 20, ShadowDepth = 0, Opacity = 0.6, Color = Colors.Black };
        clock.Children.Add(time);
        clock.Children.Add(date);

        // Use the user's own regional format, without AM/PM (like Windows does).
        var fmt = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
        string timePattern = fmt.ShortTimePattern.Replace("tt", "").Replace("t", "").Trim();
        string datePattern = "dddd, " + fmt.MonthDayPattern;

        Action update = delegate
        {
            DateTime now = DateTime.Now;
            time.Text = now.ToString(timePattern);
            date.Text = now.ToString(datePattern);
        };
        update();

        var timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += delegate { update(); };
        timer.Start();

        return clock;
    }

    // Lock first, and keep the video covering the screen until Windows confirms
    // the lock screen is up. Only then close, so the desktop is never exposed.
    static void Exit()
    {
        if (exiting) return;
        exiting = true;

        Microsoft.Win32.SystemEvents.SessionSwitch += delegate(object s, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLock)
                app.Dispatcher.BeginInvoke(new Action(CloseAfterLock));
        };

        // Safety net: if the lock is never reported, close after 5 seconds anyway.
        var fallback = new DispatcherTimer();
        fallback.Interval = TimeSpan.FromSeconds(5);
        fallback.Tick += delegate { fallback.Stop(); CloseAfterLock(); };
        fallback.Start();

        LockWorkStation();
    }

    static bool closing = false;
    static void CloseAfterLock()
    {
        if (closing) return;
        closing = true;
        // Small extra delay so the lock screen is fully drawn before our windows disappear.
        var t = new DispatcherTimer();
        t.Interval = TimeSpan.FromMilliseconds(700);
        t.Tick += delegate { t.Stop(); app.Shutdown(); };
        t.Start();
    }
}
