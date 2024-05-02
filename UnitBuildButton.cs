using Godot;
using System;

public partial class UnitBuildButton : Button
{
	[Signal]
	public delegate void OnPressedEventHandler(Unit u);

	public Unit u;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// attach pressed signal
		Pressed += SendUnitData;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SendUnitData()
	{
		GD.Print("Pressed! " + u.unitName);
		EmitSignal(SignalName.OnPressed, u);
	}
}
