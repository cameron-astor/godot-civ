using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public enum TerrainType { PLAINS, WATER, DESERT, MOUNTAIN }

public class Hex
{

	public TerrainType terrainType;
	
	public Hex()
	{

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
	List<(float Min, float Max, TerrainType Type)> terrainGenValues;

	[Export]
	FastNoiseLite noise;

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

		List<(float Min, float Max, TerrainType Type)> terrainGenValues = new List<(float Min, float Max, TerrainType Type)>
		{
			(0, 0.2f, TerrainType.WATER),
			(0.2f, 0.9f, TerrainType.PLAINS),
			(0.9f, 1.0f, TerrainType.MOUNTAIN)
		};

		Random r = new Random();

		// Perlin noise terrain gen
		noise.Seed = r.Next(10000);
		noise.FractalOctaves = 4;

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				// Create new hex
				Hex h = new Hex();
				float noiseValue = Math.Abs(noise.GetNoise2D(x, y));

				// Get terrain type from Perlin noise
				h.terrainType = terrainGenValues.First(range => noiseValue >= range.Min 
																&& noiseValue < range.Max).Type;


				mapData[new Vector2I(x, y)] = h;

				SetCell(0, new Vector2I(x, y), 0, terrainTextures[h.terrainType]);
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
