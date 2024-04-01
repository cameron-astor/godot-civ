using Godot;
using System;

public partial class Camera : Camera2D
{

	[Export]
	int velocity = 10;
	[Export]
	float zoom_speed = 0.05f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// GD.Print(Engine.GetFramesPerSecond());

		// Map controls
		if (Input.IsActionPressed("map_right"))
		{
			this.Position += new Vector2(velocity, 0);
		}
		if (Input.IsActionPressed("map_left"))
		{
			this.Position += new Vector2(-velocity, 0);
		}
		if (Input.IsActionPressed("map_down"))
		{
			this.Position += new Vector2(0, velocity);
		}
		if (Input.IsActionPressed("map_up"))
		{
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
