using System.Collections.Generic;
using Godot;

namespace ModDemo.Editor;

public partial class PropertiesController : Node
{
    private LineEdit _idEdit;
    private LineEdit _nameEdit;
    private LineEdit _xEdit;
    private LineEdit _yEdit;
    private LineEdit _zEdit;
    private LineEdit _rxEdit;
    private LineEdit _ryEdit;
    private LineEdit _rzEdit;
    private LineEdit _tagsTextEdit;
    private LevelEditorObject? _selectedObject;
    private bool _updatingUI;

    public override void _Ready()
    {
        // Get UI controls
        _idEdit = GetNode<LineEdit>("%IdEdit");
        _nameEdit = GetNode<LineEdit>("%NameEdit");
        _xEdit = GetNode<LineEdit>("%XLineEdit");
        _yEdit = GetNode<LineEdit>("%YLineEdit");
        _zEdit = GetNode<LineEdit>("%ZLineEdit");
        _rxEdit = GetNode<LineEdit>("%RXLineEdit");
        _ryEdit = GetNode<LineEdit>("%RYLineEdit");
        _rzEdit = GetNode<LineEdit>("%RZLineEdit");
        _tagsTextEdit = GetNode<LineEdit>("%TagsEdit");
        _tagsTextEdit.TextChanged += OnTagsTextChanged;

        // Connect UI signals
        _nameEdit.TextChanged += OnNameChanged;
        _xEdit.TextChanged += OnPositionChanged;
        _yEdit.TextChanged += OnPositionChanged;
        _zEdit.TextChanged += OnPositionChanged;
        _rxEdit.TextChanged += OnRotationChanged;
        _ryEdit.TextChanged += OnRotationChanged;
        _rzEdit.TextChanged += OnRotationChanged;

        // Initialize UI state
        ClearFields();
        DisableFields();
    }

    private void OnTagsTextChanged(string text)
    {
        if (_selectedObject == null) return;
        
        var tags = new Dictionary<string, string>();
        var lines = _tagsTextEdit.Text.Split('\n');
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine)) continue;
            
            var parts = trimmedLine.Split('=', 2);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                if (!string.IsNullOrEmpty(key))
                {
                    tags[key] = value;
                }
            }
        }
        
        _selectedObject.Tags = tags;
    }

    public void SetSelectedObject(LevelEditorObject? obj)
    {
        _selectedObject = obj;

        if (_selectedObject != null)
        {
            // Connect to new object
            EnableFields();
            UpdateAllFields();
        }
        else
        {
            DisableFields();
            ClearFields();
        }
    }

    public void Update()
    {
        UpdateTransformFields();
    }

    private void EnableFields()
    {
        _nameEdit.Editable = true;
        _xEdit.Editable = true;
        _yEdit.Editable = true;
        _zEdit.Editable = true;
        _rxEdit.Editable = true;
        _ryEdit.Editable = true;
        _rzEdit.Editable = true;
    }

    private void DisableFields()
    {
        _nameEdit.Editable = false;
        _xEdit.Editable = false;
        _yEdit.Editable = false;
        _zEdit.Editable = false;
        _rxEdit.Editable = false;
        _ryEdit.Editable = false;
        _rzEdit.Editable = false;
    }

    private void ClearFields()
    {
        _updatingUI = true;
        _idEdit.Text = "";
        _nameEdit.Text = "";
        _tagsTextEdit.Text = "";
        _xEdit.Text = "0";
        _yEdit.Text = "0";
        _zEdit.Text = "0";
        _rxEdit.Text = "0";
        _ryEdit.Text = "0";
        _rzEdit.Text = "0";
        _updatingUI = false;
    }

    private void UpdateAllFields()
    {
        if (_selectedObject == null) return;

        _updatingUI = true;
        
        // Update basic info
        _idEdit.Text = _selectedObject.ObjectId;
        _nameEdit.Text = _selectedObject.Name;

        // Update transform
        UpdateTransformFields();
        var tagsText = new System.Text.StringBuilder();
        foreach (var tag in _selectedObject.Tags)
        {
            tagsText.AppendLine($"{tag.Key}={tag.Value}");
        }
        _tagsTextEdit.Text = tagsText.ToString();

        
        _updatingUI = false;
    }

    private void UpdateTransformFields()
    {
        if (_selectedObject == null) return;

        _updatingUI = true;
        
        // Position
        _xEdit.Text = _selectedObject.Position.X.ToString("F2");
        _yEdit.Text = _selectedObject.Position.Y.ToString("F2");
        _zEdit.Text = _selectedObject.Position.Z.ToString("F2");

        // Rotation (convert to degrees)
        _rxEdit.Text = (_selectedObject.Rotation.X * (180.0f / Mathf.Pi)).ToString("F2");
        _ryEdit.Text = (_selectedObject.Rotation.Y * (180.0f / Mathf.Pi)).ToString("F2");
        _rzEdit.Text = (_selectedObject.Rotation.Z * (180.0f / Mathf.Pi)).ToString("F2");
        
        _updatingUI = false;
    }

    private void OnNameChanged(string newText)
    {
        if (_updatingUI || _selectedObject == null) return;
        _selectedObject.Name = newText;
    }

    private void OnPositionChanged(string newText)
    {
        if (_updatingUI || _selectedObject == null) return;

        if (float.TryParse(_xEdit.Text, out float x) &&
            float.TryParse(_yEdit.Text, out float y) &&
            float.TryParse(_zEdit.Text, out float z))
        {
            _selectedObject.Position = new Vector3(x, y, z);
        }
    }

    private void OnRotationChanged(string newText)
    {
        if (_updatingUI || _selectedObject == null) return;

        if (float.TryParse(_rxEdit.Text, out float rx) &&
            float.TryParse(_ryEdit.Text, out float ry) &&
            float.TryParse(_rzEdit.Text, out float rz))
        {
            // Convert degrees to radians
            _selectedObject.Rotation = new Vector3(
                rx * (Mathf.Pi / 180.0f),
                ry * (Mathf.Pi / 180.0f),
                rz * (Mathf.Pi / 180.0f)
            );
        }
    }
}