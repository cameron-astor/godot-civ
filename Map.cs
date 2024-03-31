using Godot;
using System;
using Godot.Collections;

public partial class Map : Node2D
{

	// Packed scenes
	private PackedScene hexTileScene;

	// We are storing the map with axial coordinates (cube
	// coordinate s may be derived: s = -q-r).
	private Tile[,] map;
	private int arrayOffset;

	// Dimensions of the map (in tiles)
	[Export]
	private int WIDTH = 7;
	[Export]
	private int HEIGHT = 7;

	// Radius of the outer circle of the hexagon tiles.
	// Decide once for the game and should not change.
	private const int TILE_SIZE = 144;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Load packed scene
		this.hexTileScene = ResourceLoader.Load<PackedScene>("res://Tile.tscn");

		this.arrayOffset = (int) Math.Floor((HEIGHT - 1)/2f); // Calculate max offset
		// GD.Print("Array max offset: " + this.arrayOffset);

		// Initialize 2D array to store map data
		this.map = new Tile[WIDTH + this.arrayOffset, HEIGHT]; // Need extra spaces width-wise in array for hex grid

		PopulateMap();

		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	private void PopulateMap()
	{
		// test populate map
		for (int q = 0; q < WIDTH; q++) {
			for (int r = 0; r < HEIGHT; r++)
			{
				Tile t = hexTileScene.Instantiate() as Tile; // Instantiate new tile

				// Adjust coordinates for rectangular offset
				int q_offset = q;
				if (r > 1) // Before r > 1, coordinates are not offset in axial space
				{
					// For r > 1, q_offset = q - Math.floor(r/2f)
					q_offset -= (int) Math.Floor(r/2f); // Apply offset
				}

				// Calculate pixel coordinates of hex
				var pixelCoords = this.HexToPixel(new Vector2(q_offset, r));

				AddChild(t); // Add to scene tree

				t.Position += pixelCoords; // Set position based on pixel coord calculation
				t.SetColor(this.GetRandomColor());

				// Convert axial coordinate to array coordinate to store in representation
				this.map[q_offset + this.arrayOffset, r] = t; // Add to map representation

				// GD.Print("[" + (q_offset + this.arrayOffset) + ", " + r + "]");
			}
		}
	}

	private Vector2 HexToPixel(Vector2 coords) {
		var x = TILE_SIZE * (Math.Sqrt(3) * coords.X + Math.Sqrt(3)/2 * coords.Y);
		var y = TILE_SIZE * (coords.Y * 3.0/2);
		return new Vector2((float) x, (float) y);
	}

	// Converts axial coordinates (q, r) to 2d array indices.
	private Vector2 AxialToArray(Vector2 axialcoords) {
		return new Vector2(axialcoords.X + this.arrayOffset, axialcoords.Y);
	}

	private Color GetRandomColor()
	{
		Random r = new Random();
		return new Color(r.Next(255)/255.0f, r.Next(255)/255.0f, r.Next(255)/255.0f);
	}
}
