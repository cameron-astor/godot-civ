using Godot;
using System;

public partial class Game : Node
{

	[Export]
	FastNoiseLite noise; // For testing purposes only

    public override void _EnterTree()
    {
		// Load unit textures
		Unit.LoadTextures();
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
