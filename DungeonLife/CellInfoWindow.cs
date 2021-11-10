using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;

namespace DungeonLife
{
    class CellInfoWindow : Window
    {
        const int DIALOG_WIDTH = 19;
        const int DIALOG_HEIGHT = 8;

        private IWorldState _world;
        private Point _selectedCell;

        private Label _algaeLabel;
        private Label _temperatureLabel;
        private Label _humidityLabel;

        public CellInfoWindow()
            : base(DIALOG_WIDTH, DIALOG_HEIGHT)
        {
            Title = "Cell Info";

            var closeText = "Close";
            var closeButton = new Button(closeText.Length + 2);
            closeButton.Text = closeText;
            closeButton.Click += (sender, e) => Hide();
            closeButton.Position = ((DIALOG_WIDTH - closeButton.Width) / 2, DIALOG_HEIGHT - 1);
            Controls.Add(closeButton);

            _algaeLabel = new Label("Algae: 0.000");
            _algaeLabel.Position = (1, 1);
            Controls.Add(_algaeLabel);

            _temperatureLabel = new Label("Temperature: 000C");
            _temperatureLabel.Position = (1, 2);
            Controls.Add(_temperatureLabel);

            _humidityLabel = new Label("Humidity: 000%");
            _humidityLabel.Position = (1, 3);
            Controls.Add(_humidityLabel);
        }

        public void SelectCell(IWorldState world, CellSelectedEventArgs e)
        {
            _world = world;
            _selectedCell = (e.WorldX, e.WorldY);

            Position = (e.ScreenX, e.ScreenY);
            if (Position.X + DIALOG_WIDTH > LifeAppSettings.ScreenWidth)
            {
                Position = (LifeAppSettings.ScreenWidth - DIALOG_WIDTH, Position.Y);
            }
            if (Position.Y + DIALOG_HEIGHT > LifeAppSettings.ScreenHeight)
            {
                Position = (Position.X, LifeAppSettings.ScreenHeight - DIALOG_HEIGHT);
            }

            Show();
        }

        public override void Update(TimeSpan delta)
        {
            if (IsVisible && (_world != null))
            {
                var cell = _world.Cells[_selectedCell.X, _selectedCell.Y];
                var algae = (cell as FloorWorldCell)?.AlgaeLevel ?? 0;

                _algaeLabel.DisplayText = $"Algae: {algae}";
                _temperatureLabel.DisplayText = $"Temperature: {(int)cell.Temperature}C";
                _humidityLabel.DisplayText = $"Humidity: {(int)(cell.Humidity * 100)}%";
            }

            base.Update(delta);
        }
    }
}
