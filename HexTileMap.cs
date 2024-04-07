using Godot;
using System;
using System.Collections.Generic;

public enum TerrainType { PLAINS, WATER, DESERT, MOUNTAIN }

public class Hex
{

	public TerrainType terrainType;
	
	public Hex(TerrainType t)
	{

		this.terrainType = t;
	}
}

public partial class HexTileMap : TileMap
{

	[Export]
	int width = 106;
	[Export]
	int height = 66;

	Dictionary<Vector2I, Hex> mapData;
	Dictionary<TerrainType, Vector2I> terrainTextures; // Maps terrain types to their textures in texture atlas


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		mapData = new Dictionary<Vector2I, Hex>();
		terrainTextures = new Dictionary<TerrainType, Vector2I>
		{
		    { TerrainType.PLAINS, new Vector2I(0, 0) }, // Texture atlas coords
			{ TerrainType.WATER, new Vector2I(1, 0) },
			{ TerrainType.DESERT, new Vector2I(0, 1) },
			{ TerrainType.MOUNTAIN, new Vector2I(1, 1) }	
		};

		Random r = new Random();

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				// Create new hex
				Hex h = new Hex((TerrainType) r.Next(4));

				mapData[new Vector2I(x, y)] = h;

				SetCell(0, new Vector2I(x, y), 0, terrainTextures[(h.terrainType)]);
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("left_click")) {
			Vector2I mapCoords = LocalToMap(GetGlobalMousePosition());
			GD.Print(mapCoords);
			GD.Print(mapData[mapCoords].terrainType);
		}
	}
}
