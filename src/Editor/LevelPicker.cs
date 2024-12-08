using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using ModDemo.Util;

namespace ModDemo.Editor
{
    public partial class LevelPicker : Window
    {
        private ItemList _levelsList;
        private Button _loadButton;
        private Button _cancelButton;
        private Mod.Mod _mod;

        [Signal]
        public delegate void LevelSelectedEventHandler(string levelName);

        public override void _Ready()
        {
            // Get UI elements using unique names from the scene
            _levelsList = GetNode<ItemList>("%ItemList");
            _loadButton = GetNode<Button>("%Load Button");
            _cancelButton = GetNode<Button>("%Cancel Button");

            // Connect button signals
            _loadButton.Pressed += OnLoadPressed;
            _cancelButton.Pressed += OnCancelPressed;

            // Make window transient and exclusive
            Transient = true;
            Exclusive = true;

            // Connect visibility change
            VisibilityChanged += OnVisibilityChanged;
        }

        public void Initialize(Mod.Mod mod)
        {
            _mod = mod;
        }

        private void OnVisibilityChanged()
        {
            if (Visible)
            {
                UpdateLevelsList();
            }
        }

        private void UpdateLevelsList()
        {
            _levelsList.Clear();
            
            foreach (var levelName in _mod.LevelsNames)
            {
                _levelsList.AddItem(levelName);
            }

            // Disable load button if no levels
            _loadButton.Disabled = _levelsList.ItemCount == 0;
        }

        private void OnLoadPressed()
        {
            var selected = _levelsList.GetSelectedItems();
            if (selected.Length > 0)
            {
                var levelName = _levelsList.GetItemText((int)selected[0]);
                EmitSignal(SignalName.LevelSelected, levelName);
                Hide();
            }
        }

        private void OnCancelPressed()
        {
            Hide();
        }
    }
}