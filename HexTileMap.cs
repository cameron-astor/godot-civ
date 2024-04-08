using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public enum TerrainType { PLAINS, WATER, DESERT, MOUNTAIN, ICE, SHALLOW_WATER, FOREST, BEACH }

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
	public int width = 106;
	[Export]
	public int height = 66;

	Dictionary<Vector2I, Hex> mapData;
	Dictionary<TerrainType, Vector2I> terrainTextures; // Maps terrain types to their textures in texture atlas
	List<(float Min, float Max, TerrainType Type)> terrainGenValues;

	[Export]
	FastNoiseLite noise;

	// For storing noise values during intermediate stages of terrain generation, allowing for adjustments
	// and setting terrain textures based on ratios rather than raw noise values
	float[,] noiseMap; 

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		mapData = new Dictionary<Vector2I, Hex>();
		terrainTextures = new Dictionary<TerrainType, Vector2I>
		{
		    { TerrainType.PLAINS, new Vector2I(0, 0) }, // Texture atlas coords
			{ TerrainType.WATER, new Vector2I(1, 0) },
			{ TerrainType.DESERT, new Vector2I(0, 1) },
			{ TerrainType.MOUNTAIN, new Vector2I(1, 1) },
			{ TerrainType.SHALLOW_WATER, new Vector2I(1, 2) },
			{ TerrainType.BEACH, new Vector2I(0, 2) },
			{ TerrainType.FOREST, new Vector2I(1, 3) },
			{ TerrainType.ICE, new Vector2I(0, 3) }		
		};

		Random r = new Random();

		// Perlin noise terrain gen
		noiseMap = new float[width, height];

		noise.Seed = r.Next(10000);
		noise.FractalOctaves = 4;
		float noiseMax = 0f; // Keep track of range of noise

		// First pass: basic terrain and water
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				noiseMap[x, y] = Math.Abs(noise.GetNoise2D(x, y));
				if (noiseMap[x, y] > noiseMax) noiseMax = noiseMap[x, y];
			}
		}

		// Calculate base terrain tile ratios.
		// Base terrain includes water, shallow water, beach, and plains.
		// This will form the basic structures of the continents.

		List<(float Min, float Max, TerrainType Type)> terrainGenValues = new List<(float Min, float Max, TerrainType Type)>
		{
			(0, noiseMax/10 * 2, TerrainType.WATER),
			(noiseMax/10 * 2, noiseMax/10 * 4, TerrainType.SHALLOW_WATER),
			(noiseMax/10 * 4, noiseMax/10 * 4.5f, TerrainType.BEACH),
			(noiseMax/10 * 4.5f, noiseMax + 0.05f, TerrainType.PLAINS)
		};

		// Second pass: deserts, forests, and ice
		// noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Cellular;
		// noise.Seed = r.Next(10000);
	
		// for (int x = 0; x < width; x++)
		// {
		// 	for (int y = 0; y < height; y++)
		// 	{
		// 		noiseMap[x, y] = Math.Abs(noise.GetNoise2D(x, y));
		// 		if (noiseMap[x, y] > noiseMax) noiseMax = noiseMap[x, y];
		// 	}
		// }

		// Third pass: mountains

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				// Create new hex
				Hex h = new Hex();
				float noiseValue = noiseMap[x, y];

				// Get basic terrain type from Perlin noise
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
			Vector2I mapCoords = LocalToMap(ToLocal(GetGlobalMousePosition()));
			GD.Print(mapCoords);
			GD.Print(mapData[mapCoords].terrainType);
		}
	}
}
