using System;
using Godot;

namespace ModDemo.Core;

[GlobalClass]
public partial class SignalBus : Node
{
    private static SignalBus? _instance;

    public static SignalBus Instance => _instance ?? throw new NullReferenceException("SignalBus instance is not initialized.");

    public SignalBus()
    {
        _instance = this;
    }

    [Signal]
    public delegate void SignalDispatchedEventHandler(string signalName, Variant args);

    public static void DispatchSignal(string signalName, Variant args)
    {
        Instance.EmitSignal(SignalName.SignalDispatched, signalName, args);
    }
}