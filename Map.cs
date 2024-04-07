using Godot;
using System;
using Godot.Collections;
using System.Linq;



public partial class Map : Node2D
{

	// FLAGS
	public bool MAP_GENERATED = false;

	// Packed scenes
	private PackedScene hexTileScene;
	private PackedScene mapChunkScene;

	// Camera
	private Camera2D camera;

	// We are storing the map with axial coordinates (cube
	// coordinate s may be derived: s = -q-r).
	public Tile[,] map;
	private int arrayOffset;

	private MapChunk[] chunks;

	// Dimensions of the map (in tiles)
	[Export]
	private int WIDTH = 7;
	[Export]
	private int HEIGHT = 7;
	[Export]
	private int CHUNK_SIZE = 32;

	// Radius of the outer circle of the hexagon tiles.
	// Decide once for the game and should not change.
	[Export]
	private const int TILE_SIZE = 128;
	// base hexagon vertices for this tile size
	public Vector2[] baseHexVertices;



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Load packed scene
		this.hexTileScene = ResourceLoader.Load<PackedScene>("res://Tile.tscn");
		this.mapChunkScene = ResourceLoader.Load<PackedScene>("res://MapChunk.tscn");

		this.arrayOffset = (int) Math.Floor((HEIGHT - 1)/2f); // Calculate max offset

		// Initialize 2D array to store map data
		this.map = new Tile[WIDTH + this.arrayOffset, HEIGHT]; // Need extra spaces width-wise in array for hex grid

		// Calculate hexagon vertices for this tile size
		baseHexVertices = CalculateHexagonVertices(TILE_SIZE);

		this.camera = GetNode<Camera2D>("../Camera");

		PopulateMap();
		GenerateChunks();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("map_rotate_left")) {
			GD.Print("redraw");
			this.QueueRedraw();
		}

		if (Input.IsActionJustPressed("left_click")) {
			var coords = GetGlobalMousePosition();
			GD.Print(coords);
			GD.Print(PixelToHex(coords));
		}
	}

    public override void _Draw()
    {
		if (MAP_GENERATED)
		{
			for (int x = 0; x < this.map.GetLength(0); x++) 
			{
				for (int y = 0; y < this.map.GetLength(1); y++)
				{					
					if (this.map[x, y] != null)
					{
						// Apply transform to vertices to get them to the right pixel coord
						Vector2[] hexVertices = baseHexVertices.Select(v => v + this.map[x, y].Position).ToArray();
						DrawColoredPolygon(hexVertices, this.GetRandomColor());
					}
				}
			}
		}

    }

	// Given a center coordinate of a hex in pixels and a size for the hex,
	// returns an array of coordinates representing the vertices of a hex
	// of that size about a (0, 0) origin.
	public Vector2[] CalculateHexagonVertices(int size) 
	{
		Vector2[] v = new Vector2[6];

		float width = size/2f * (float) Math.Sqrt(3);

		v[0] = new Vector2(0, -size); // Top of pointy top hex
		v[1] = new Vector2(width, -size/2f); // Going clockwise around the hex
		v[2] = new Vector2(width, size/2f);
		v[3] = new Vector2(0, size);
		v[4] = new Vector2(-width, size/2f);
		v[5] = new Vector2(-width, -size/2f);

		return v;
	}

    private void PopulateMap()
	{
		// test populate map
		for (int q = 0; q < WIDTH; q++) {
			for (int r = 0; r < HEIGHT; r++)
			{
				Tile t = new Tile();

				// Adjust coordinates for rectangular offset
				int q_offset = ApplyQOffset(q, r);

				// Calculate pixel coordinates of hex
				var pixelCoords = this.HexToPixel(new Vector2(q_offset, r));

				t.Position += pixelCoords; // Set position based on pixel coord calculation

				// Convert axial coordinate to array coordinate to store in representation
				this.map[q_offset + this.arrayOffset, r] = t; // Add to map representation
			}
		}

		MAP_GENERATED = true;
		QueueRedraw();
	}

	private void GenerateChunks()
	{

	}

	// Adjust coordinates for rectangular offset.
	// Returns adjusted q value.
	private int ApplyQOffset(int q, int r)
	{
		if (r > 1)
		{
			q = q - (int) Math.Floor(r/2f);
			GD.Print("floored r: " + Math.Floor(r/2f));
			GD.Print(q);
		}
		return q;
	}

	private Vector2 HexToPixel(Vector2 coords) 
	{
		var x = TILE_SIZE * (Math.Sqrt(3) * coords.X + Math.Sqrt(3)/2 * coords.Y);
		var y = TILE_SIZE * (coords.Y * 3f/2);
		return new Vector2((float) x, (float) y);
	}

	private Vector2 PixelToHex(Vector2 coords) 
	{
		var q = (float) (Math.Sqrt(3)/3 * coords.X + 1f/3 * coords.Y) / TILE_SIZE;
		var r = (float) coords.Y * 2f/3 / TILE_SIZE;

		Vector2 rounded = HexRound(new Vector2(q, r));
		GD.Print("rounded r: " + rounded.Y);
		rounded.X = ApplyQOffset((int) rounded.X, (int) rounded.Y);

		return rounded;
	}

	private Vector2 HexRound(Vector2 rawCoords)
	{
		// Get the s coordinate (converting to cube coordinates)
		float raw_q = rawCoords.X;
		float raw_r = rawCoords.Y;
		float raw_s = -rawCoords.X-rawCoords.Y;

		// Initial rounding
		var q = Math.Round(raw_q);
		var r = Math.Round(raw_r);
		var s = Math.Round(raw_s);

		// Remaining fractions of hex
		var q_diff = Math.Abs(q - raw_q);
		var r_diff = Math.Abs(r - raw_r);
		var s_diff = Math.Abs(s - raw_s);

		if (q_diff > r_diff && q_diff > s_diff)
			q = -r-s;
		else if (r_diff > s_diff)
			r = -q-s;
		else
			s = -q-r;

		return new Vector2((int) q, (int) r);
	}

	// Converts axial coordinates (q, r) to 2d array indices.
	private Vector2 AxialToArray(Vector2 axialcoords) {
		return new Vector2(axialcoords.X + this.arrayOffset, axialcoords.Y);
	}

	
	private Vector2 ArrayToAxial(Vector2 arraycoords) {
		return new Vector2(arraycoords.X - this.arrayOffset, arraycoords.Y);
	}

	private Color GetRandomColor()
	{
		Random r = new Random();
		return new Color(r.Next(255)/255.0f, r.Next(255)/255.0f, r.Next(255)/255.0f);
	}
}
