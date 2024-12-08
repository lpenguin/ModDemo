using Godot;

[Tool]
public partial class LevelExporterPlugin : EditorPlugin
{
    private MenuButton _menuButton;
    
    public override void _EnterTree()
    {
        _menuButton = new MenuButton
        {
            Text = "Level",
            SwitchOnHover = true
        };
        
        AddControlToContainer(CustomControlContainer.SpatialEditorMenu, _menuButton);
        
        var popup = _menuButton.GetPopup();
        popup.AddItem("Export Level", 0);
        popup.IdPressed += OnMenuItemPressed;
    }

    public override void _ExitTree()
    {
        if (_menuButton != null)
        {
            RemoveControlFromContainer(CustomControlContainer.Toolbar, _menuButton);
            _menuButton.QueueFree();
        }
    }

    private void OnMenuItemPressed(long id)
    {
        // Currently does nothing as per requirement
        GD.Print("Export Level menu item clicked");
    }
}