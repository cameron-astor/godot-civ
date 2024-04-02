using Godot;
using System;
using System.Linq;

public partial class MapChunk : Node2D
{

	public Map mapNode;
	public Tile[,] map;
	public Vector2 x_range; // The ranges are based on the 2D array representation, not axial coords.
	public Vector2 y_range;

	public Color chunkColor; // DEBUG

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		mapNode = GetParent<Map>();
		map = mapNode.map;
		Random r = new Random();
		chunkColor = new Color(r.Next(255)/255.0f, r.Next(255)/255.0f, r.Next(255)/255.0f);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


    public override void _Draw()
    {
        if (mapNode.MAP_GENERATED)
		{
			for (int x = (int) x_range.X; x < x_range.Y; x++) 
			{
				for (int y = (int) y_range.X; y < y_range.Y; y++)
				{					
					if (this.map[x, y] != null)
					{
						// Apply transform to vertices to get them to the right pixel coord
						Vector2[] hexVertices = mapNode.baseHexVertices.Select(v => v + this.map[x, y].Position).ToArray();
						DrawColoredPolygon(hexVertices, chunkColor);
					}
				}
			}
		}
    }

	public void SetXRange(int min, int max)
	{
		this.x_range = new Vector2(min, max);
	}

	public void SetYRange(int min, int max)
	{
		this.y_range = new Vector2(min, max);
	}


}
