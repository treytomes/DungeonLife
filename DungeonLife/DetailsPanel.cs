﻿using SadConsole.UI;
using SadConsole.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;
using SadRogue.Primitives;
using SadConsole;

namespace DungeonLife
{
    class DetailsPanel : ControlsConsole
    {
        private Label _cellDetailsHeader;
        private Label _algaeLabel;
        private Label _temperatureLabel;
        private Label _humidityLabel;

        private Label _entityDetailsHeader;
        private Label _entityName;
        private Label _entityAge;
        private Label _entityHunger;
        private Label _entityThirstLabel;
        //private ProgressBar _entityThirst;
        private Label _entityMetabolism;
        private Label _entityIsChild;
        private Label _entityState;

        public DetailsPanel(int width, int height)
            : base(width, height)
        {
            _cellDetailsHeader = new Label("Cell Details (256, 256)");
            _cellDetailsHeader.Position = (0, 0);
            _cellDetailsHeader.TextColor = Color.AnsiWhiteBright;
            Controls.Add(_cellDetailsHeader);

            _algaeLabel = new Label("Algae: 0.000");
            _algaeLabel.Position = (1, 1);
            Controls.Add(_algaeLabel);

            _temperatureLabel = new Label("Temperature: 000C");
            _temperatureLabel.Position = (1, 2);
            Controls.Add(_temperatureLabel);

            _humidityLabel = new Label("Humidity: 000%");
            _humidityLabel.Position = (1, 3);
            Controls.Add(_humidityLabel);

            _entityDetailsHeader = new Label("Entity Details (256, 256)");
            _entityDetailsHeader.Position = (0, 4);
            _entityDetailsHeader.TextColor = Color.AnsiWhiteBright;
            Controls.Add(_entityDetailsHeader);

            _entityName = new Label("Name:                 ");
            _entityName.Position = (1, 5);
            Controls.Add(_entityName);

            _entityAge = new Label("Age:    ");
            _entityAge.Position = (1, 6);
            Controls.Add(_entityAge);

            _entityHunger = new Label("Hunger:     ");
            _entityHunger.Position = (1, 7);
            Controls.Add(_entityHunger);

            _entityThirstLabel = new Label("Thirst:     ");
            _entityThirstLabel.Position = (1, 8);
            Controls.Add(_entityThirstLabel);

            //_entityThirst = new ProgressBar(width - 2 - _entityThirstLabel.Width, 1, HorizontalAlignment.Left);
            //_entityThirst.Position = (_entityThirstLabel.Position.X + _entityThirst.Width + 1, _entityThirstLabel.Position.Y);
            //Controls.Add(_entityThirst);

            _entityMetabolism = new Label("Metabolism:       ");
            _entityMetabolism.Position = (1, 9);
            Controls.Add(_entityMetabolism);

            _entityIsChild = new Label("Is child? yes");
            _entityIsChild.Position = (1, 10);
            Controls.Add(_entityIsChild);

            _entityState = new Label("State:                 ");
            _entityState.Position = (1, 11);
            Controls.Add(_entityState);
        }

        public WorldCell SelectedCell { get; set; }
        public Entity SelectedEntity { get; set; }

        public override void Update(TimeSpan delta)
        {
            if (SelectedCell != null)
            {
                var algae = (SelectedCell as FloorWorldCell)?.AlgaeLevel ?? 0;

                _cellDetailsHeader.DisplayText = $"Cell Details ({SelectedCell.X}, {SelectedCell.Y})";
                _algaeLabel.DisplayText = $"Algae: {algae}";
                _temperatureLabel.DisplayText = $"Temperature: {(int)SelectedCell.Temperature}C";
                _humidityLabel.DisplayText = $"Humidity: {(int)(SelectedCell.Humidity * 100)}%";
            }

            if (SelectedEntity != null)
            {
                _entityDetailsHeader.DisplayText = $"Entity Details ({(int)SelectedEntity.X}, {(int)SelectedEntity.Y})";
                _entityName.DisplayText = $"Name: {SelectedEntity.GetType().GetCustomAttribute<DisplayNameAttribute>().DisplayName}";
                _entityAge.DisplayText = $"Age: {(int)SelectedEntity.Age.TotalDays}";
                _entityHunger.DisplayText = $"Hunger: {(int)(SelectedEntity.Hunger * 100)}%";
                _entityThirstLabel.DisplayText = $"Thirst: {(int)(SelectedEntity.Thirst * 100)}%";
                //_entityThirst.Progress = SelectedEntity.Thirst;
                _entityMetabolism.DisplayText = $"Metabolism: {SelectedEntity.Metabolism}";
                _entityIsChild.DisplayText = $"Is child? {(SelectedEntity.IsChild ? "yes" : "no")}";
                _entityState.DisplayText = $"State: {SelectedEntity.ActiveBehavior}";
            }

            base.Update(delta);
        }
    }
}
