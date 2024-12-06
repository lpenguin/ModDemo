using System;
using System.Collections.Generic;
using Godot;

namespace ModDemo.Nodes;

[GlobalClass]
public partial class KeyValuesStorage : Node
{
    private static KeyValuesStorage? _instance;
    public static KeyValuesStorage Instance => _instance ?? throw new InvalidOperationException("KeyValuesStorage is not initialized");

    private Dictionary<string, object> _keyValues = new();

    public override void _Ready()
    {
        _instance = this;
    }

    public void Set(string key, object value)
    {
        _keyValues[key] = value;
    }

    public object Get(string key)
    {
        return _keyValues[key];
    }
}