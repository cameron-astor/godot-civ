using Godot;
using System;

public partial class Camera : Camera2D
{

	[Export]
	int velocity = 15;
	[Export]
	float zoom_speed = 0.05f;

	HexTileMap map; // Reference to the tilemap.

	float leftBound;
	float rightBound;
	float topBound;
	float bottomBound;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		map = GetNode<HexTileMap>("../HexTileMap");

		// Calculate camera boundaries from map dimensions
		leftBound = ToGlobal(map.MapToLocal(new Vector2I(0, 0))).X + 100;
		rightBound = ToGlobal(map.MapToLocal(new Vector2I(map.width, 0))).X - 100;
		topBound = ToGlobal(map.MapToLocal(new Vector2I(0, 0))).Y + 50;
		bottomBound = ToGlobal(map.MapToLocal(new Vector2I(0, map.height))).Y - 50;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// GD.Print(Engine.GetFramesPerSecond());

		// Map controls
		if (Input.IsActionPressed("map_right"))
		{
			if (this.Position.X < rightBound)
				this.Position += new Vector2(velocity, 0);
		}
		if (Input.IsActionPressed("map_left"))
		{
			if (this.Position.X > leftBound)
				this.Position += new Vector2(-velocity, 0);
		}
		if (Input.IsActionPressed("map_down"))
		{
			if (this.Position.Y < bottomBound)
				this.Position += new Vector2(0, velocity);
		}
		if (Input.IsActionPressed("map_up"))
		{
			if (this.Position.Y > topBound)
				this.Position += new Vector2(0, -velocity);
		}

		// Zoom controls
		if (Input.IsActionPressed("map_zoom_in"))
		{
			if (this.Zoom < new Vector2(3f, 3f)) {
				this.Zoom += new Vector2(zoom_speed, zoom_speed);
			}
			// GD.Print(this.Zoom);
		}
		if (Input.IsActionPressed("map_zoom_out"))
		{
			if (this.Zoom > new Vector2(0.1f, 0.1f)) {
				this.Zoom -= new Vector2(zoom_speed, zoom_speed);
			}
			// GD.Print(this.Zoom);
		}
		
	}

}
