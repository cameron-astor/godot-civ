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

	// Mouse controls
	bool mouseWheelScrollingUp = false;
	bool mouseWheelScrollingDown = false;


    public override void _Ready()
    {
		map = GetNode<HexTileMap>("../HexTileMap");

		// Calculate camera boundaries from map dimensions
		leftBound = ToGlobal(map.MapToLocal(new Vector2I(0, 0))).X + 100;
		rightBound = ToGlobal(map.MapToLocal(new Vector2I(map.width, 0))).X - 100;
		topBound = ToGlobal(map.MapToLocal(new Vector2I(0, 0))).Y + 50;
		bottomBound = ToGlobal(map.MapToLocal(new Vector2I(0, map.height))).Y - 50;
    }

	// Note that the camera logic is in _PhysicsProcess() rather than _Process().
	// This is because we need a consistent tick rate to avoid subtle stuttering and movement'
	// artifacts with the camera. 
	//
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
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
		if (Input.IsActionPressed("map_zoom_in") || mouseWheelScrollingUp)
		{
			if (this.Zoom < new Vector2(3f, 3f)) {
				this.Zoom += new Vector2(zoom_speed, zoom_speed);
			}
		}
		if (Input.IsActionPressed("map_zoom_out") || mouseWheelScrollingDown)
		{
			if (this.Zoom > new Vector2(0.1f, 0.1f)) {
				this.Zoom -= new Vector2(zoom_speed, zoom_speed);
			}
		}

		if (Input.IsActionJustReleased("mouse_zoom_in"))
		{
			mouseWheelScrollingUp = true;
		}

		if (!Input.IsActionJustReleased("mouse_zoom_in"))
		{
			mouseWheelScrollingUp = false;
		}

		if (Input.IsActionJustReleased("mouse_zoom_out"))
		{
			mouseWheelScrollingDown = true;
		}

		if (!Input.IsActionJustReleased("mouse_zoom_out"))
		{
			mouseWheelScrollingDown = false;
		}
		
	}

	public void SetPos(Vector2 pos)
	{
		Position = pos;
	}

}
