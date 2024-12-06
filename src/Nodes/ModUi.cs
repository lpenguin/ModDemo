using System;
using System.Collections.Generic;
using Godot;

namespace ModDemo.Nodes;

[GlobalClass]
public partial class ModUi : Control
{
    private Label _messageLabel;
    private List<string> _messages = new();
    
    private static ModUi? _instance;
    public static ModUi Instance => _instance ?? throw new InvalidOperationException("ModUi is not initialized");

    public override void _Ready()
    {
        _messageLabel = GetNode<Label>("%Messages");
        _instance = this;
    }

    public override void _Process(double delta)
    {
        _messageLabel.Text = string.Join("\n", _messages.ToArray());
        _messages.Clear();
    }

    public void AddMessage(string message)
    {
        _messages.Add(message);
    }
}