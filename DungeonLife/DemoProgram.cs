using System;
using SadConsole;
using SadRogue.Primitives;
using Console = SadConsole.Console;

namespace DungeonLife
{
    class DemoProgram
    {
        private static ScreenObject mainConsole;

        private static void DemoMain(string[] args)
        {
            var SCREEN_WIDTH = 80;
            var SCREEN_HEIGHT = 25;

            Settings.WindowTitle = "Dungeon Life";
            Settings.UseDefaultExtendedFont = true;

            Game.Create(SCREEN_WIDTH, SCREEN_HEIGHT);
            Game.Instance.OnStart = Init;
            Game.Instance.FrameUpdate += Instance_FrameUpdate;
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        private static void Instance_FrameUpdate(object sender, GameHost e)
        {
            if (GameHost.Instance.Keyboard.IsKeyReleased(SadConsole.Input.Keys.F11))
            {
                Game.Instance.ToggleFullScreen();
            }
        }

        private static void Init()
        {
            mainConsole = Game.Instance.StartingConsole;
        }

        private static void Demo2Init()
        {
            //SadConsole.Game.Instance.SetSplashScreens(new SadConsole.SplashScreens.PCBoot());

            mainConsole = new ScreenObject();
            Game.Instance.Screen = mainConsole;
            Game.Instance.DestroyDefaultStartingConsole();

            // First console.
            var console1 = new Console(60, 14);
            console1.Position = new Point(3, 2);
            console1.DefaultBackground = Color.AnsiCyan;
            console1.Clear();
            console1.Print(1, 1, "Type on me!");
            console1.Cursor.Position = new Point(1, 2);
            console1.Cursor.IsEnabled = true;
            console1.Cursor.IsVisible = true;
            console1.Cursor.MouseClickReposition = true;
            console1.FocusOnMouseClick = true;
            console1.MoveToFrontOnMouseClick = true;
            console1.IsFocused = true;

            var surfaceObject = new ScreenSurface(5, 3);
            surfaceObject.Surface.FillWithRandomGarbage(surfaceObject.Font);
            surfaceObject.Position = console1.Area.Center - (surfaceObject.Surface.Area.Size / 2);
            surfaceObject.UseMouse = false;

            console1.Children.Add(surfaceObject);

            mainConsole.Children.Add(console1);

            // First console.
            var console2 = new Console(58, 12);
            console2.Position = new Point(19, 11);
            console2.DefaultBackground = Color.AnsiRed;
            console2.Clear();
            console2.Print(1, 1, "Type on me!");
            console2.Cursor.Position = new Point(1, 2);
            console2.Cursor.IsEnabled = true;
            console2.Cursor.IsVisible = true;
            console2.FocusOnMouseClick = true;
            console2.MoveToFrontOnMouseClick = true;

            mainConsole.Children.Add(console2);
            mainConsole.Children.MoveToBottom(console2);
        }

        private static void Demo1Init()
        {
            // This code uses the default console created for you at start
            var startingConsole = Game.Instance.StartingConsole;

            //startingConsole.FillWithRandomGarbage(SadConsole.Game.Instance.StartingConsole.Font);
            startingConsole.Fill(new Rectangle(3, 3, 23, 3), Color.Violet, Color.Black, 0, Mirror.None);
            //startingConsole.DrawBox(new Rectangle(3, 3, 23, 3), ShapeParameters.CreateBorder(new ColoredGlyph(Color.Violet, Color.Black, 176)));
            startingConsole.DrawBox(new Rectangle(3, 3, 23, 3), ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThick, new ColoredGlyph(Color.Violet, Color.Black, 176)));
            startingConsole.Print(4, 4, "Hello from SadConsole");

            startingConsole.DrawCircle(new Rectangle(5, 8, 16, 8), ShapeParameters.CreateFilled(new ColoredGlyph(Color.Violet, Color.Black, 176), new ColoredGlyph(Color.White, Color.Black)));

            startingConsole.DrawLine(new Point(60, 5), new Point(66, 20), '$', Color.AnsiBlue, Color.AnsiBlueBright, Mirror.None);

            startingConsole.SetForeground(15, 4, Color.DarkGreen);
            startingConsole.SetBackground(18, 4, Color.DarkCyan);
            startingConsole.SetGlyph(4, 4, '@');
            startingConsole.SetMirror(10, 4, Mirror.Vertical);

            startingConsole.Cursor.PrintAppearanceMatchesHost = false;
            startingConsole.Cursor
                .Move(0, 21)
                .Print("Kato is my favorite dog.")
                .SetPrintAppearance(Color.Green)
                .NewLine()
                .Print("No, Birdie is my favorite dog.");
            startingConsole.Cursor.IsVisible = true;
            startingConsole.Cursor.IsEnabled = true;

            // --------------------------------------------------------------
            // This code replaces the default starting console with your own.
            // If you use this code, delete the code above.
            // --------------------------------------------------------------
            /*
            var console = new Console(Game.Instance.ScreenCellsX, SadConsole.Game.Instance.ScreenCellsY);
            console.FillWithRandomGarbage(console.Font);
            console.Fill(new Rectangle(3, 3, 23, 3), Color.Violet, Color.Black, 0, 0);
            console.Print(4, 4, "Hello from SadConsole");

            Game.Instance.Screen = console;

            // This is needed because we replaced the initial screen object with our own.
            Game.Instance.DestroyDefaultStartingConsole();
            */
        }
    }
}
