using Godot;
using System;

public partial class Tile : Node2D
{

	private Polygon2D hexagon;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		hexagon = GetNode<Polygon2D>("Hexagon");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetColor(Color c)
	{
		hexagon.Color = c;
	}
}
