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
	// private Dictionary<Vector2I, Tile> map;

	// Dimensions of the map (in tiles)
	private int WIDTH = 10;
	private int HEIGHT = 10;

	// Radius of the outer circle of the hexagon tiles.
	// Decide once for the game and should not change.
	private const int TILE_SIZE = 144;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Load packed scene
		this.hexTileScene = ResourceLoader.Load<PackedScene>("res://Tile.tscn");

		this.map = new Tile[WIDTH, HEIGHT];
		// this.map = new Dictionary<Vector2I, Tile>();

		// test populate map
		for (int q = 0; q < WIDTH; q++) {
			for (int r = 0; r < HEIGHT; r++)
			{
				Tile t = hexTileScene.Instantiate() as Tile;

				// Calculate pixel coordinates of hex
				var pixelCoords = this.HexToPixel(new Vector2(q, r));

				AddChild(t);

				t.Position += pixelCoords;
				t.SetColor(this.GetRandomColor());

				this.map[q, r] = t;
			}
		}

		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	private Vector2 HexToPixel(Vector2 coords) {
		var x = TILE_SIZE * (Math.Sqrt(3) * coords.X + Math.Sqrt(3)/2 * coords.Y);
		var y = TILE_SIZE * (coords.Y * 3.0/2);
		return new Vector2((float) x, (float) y);
	}

	private Color GetRandomColor()
	{
		Random r = new Random();
		return new Color(r.Next(255)/255.0f, r.Next(255)/255.0f, r.Next(255)/255.0f);
	}
}
