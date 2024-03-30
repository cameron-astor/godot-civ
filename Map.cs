using Godot;
using System;
using Godot.Collections;

public partial class Map : Node2D
{

	// Packed scenes
	private PackedScene hexTileScene;

	// We are storing the map with axial coordinates (cube
	// coordinate s may be derived: s = -q-r).
	private Dictionary<Vector2I, Tile> map;

	// Dimensions of the map (in tiles)
	private int WIDTH = 10;
	private int HEIGHT = 10;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Load packed scene
		this.hexTileScene = ResourceLoader.Load<PackedScene>("res://Tile.tscn");

		this.map = new Dictionary<Vector2I, Tile>();

		// test populate map
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
}
