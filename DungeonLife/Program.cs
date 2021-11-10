using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;

namespace DungeonLife
{
    class Program
    {
        private static ControlsConsole _main;
        private static WorldConsole _worldConsole;
        private static Label _timeLabel;
        private static Button _runButton;
        private static Button _pauseButton;
        private static Button _stepButton;

        private static void Main(string[] args)
        {
            Settings.WindowTitle = LifeAppSettings.WindowTitle;
            Settings.UseDefaultExtendedFont = true;

            Game.Create(LifeAppSettings.ScreenWidth, LifeAppSettings.ScreenHeight);
            Game.Instance.OnStart = OnInit;
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

            var days = (int)_worldConsole.WorldTime.TotalDays;
            var hours = _worldConsole.WorldTime.Hours;
            var minutes = _worldConsole.WorldTime.Minutes;
            _timeLabel.DisplayText = $"Time: {days} days, {hours} hours, {minutes} minutes";
        }

        private static void OnInit()
        {
            _main = new ControlsConsole(LifeAppSettings.ScreenWidth, LifeAppSettings.ScreenHeight);
            Game.Instance.Screen = _main;
            Game.Instance.DestroyDefaultStartingConsole();

            const int DETAILS_WIDTH = 32;
            _worldConsole = new WorldConsole(LifeAppSettings.ScreenWidth - DETAILS_WIDTH, LifeAppSettings.ScreenHeight);

            //var cellInfoDialog = new CellInfoWindow();
            //_main.Children.Add(cellInfoDialog);

            var details = new DetailsPanel(LifeAppSettings.ScreenWidth - _worldConsole.Width, LifeAppSettings.ScreenHeight);
            details.Position = (LifeAppSettings.ScreenWidth - details.Width, 0);
            _main.Children.Add(details);

            _worldConsole.Position = (0, 1);
            _worldConsole.CellSelected += (sender, e) =>
            {
                var world = sender as IWorldState;
                //cellInfoDialog.SelectCell(sender as IWorldState, e);
                details.SelectedCell = world.Cells[e.WorldX, e.WorldY];
                details.SelectedEntity = world.GetEntityAt(e.WorldX, e.WorldY);
            };
            _main.Children.Add(_worldConsole);

            const int TOOLBAR_X = 0;
            const int TOOLBAR_Y = 0;

            _runButton = new Button(5);
            _runButton.Text = "Run";
            _runButton.Position = (TOOLBAR_X, TOOLBAR_Y);
            _runButton.IsEnabled = false;
            _runButton.Click += (sender, e) =>
            {
                (sender as Button).IsEnabled = false;
                _pauseButton.IsEnabled = true;
                _stepButton.IsEnabled = false;
                _worldConsole.IsRunning = true;
            };
            _main.Controls.Add(_runButton);

            _pauseButton = new Button(7);
            _pauseButton.Text = "Pause";
            _pauseButton.Position = (_runButton.Position.X + _runButton.Width + 1, TOOLBAR_Y);
            _pauseButton.IsEnabled = true;
            _pauseButton.Click += (sender, e) =>
            {
                (sender as Button).IsEnabled = false;
                _runButton.IsEnabled = true;
                _stepButton.IsEnabled = true;
                _worldConsole.IsRunning = false;
            };
            _main.Controls.Add(_pauseButton);

            _stepButton = new Button(6);
            _stepButton.Text = "Step";
            _stepButton.Position = (_pauseButton.Position.X + _pauseButton.Width + 1, TOOLBAR_Y);
            _stepButton.IsEnabled = false;
            _stepButton.Click += (sender, e) =>
            {
                _worldConsole.IsStepping = true;
            };
            _main.Controls.Add(_stepButton);

            _timeLabel = new Label($"Time: #### days, ## hours, ## minutes");
            _timeLabel.Position = (_stepButton.Position.X + _stepButton.Width + 1, TOOLBAR_Y);
            _main.Controls.Add(_timeLabel);
        }
    }
}