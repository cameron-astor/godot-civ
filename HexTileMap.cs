using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public enum TerrainType { PLAINS, WATER, DESERT, MOUNTAIN, ICE, SHALLOW_WATER, FOREST, BEACH }

public class Hex
{

	// Tile attributes
	public TerrainType terrainType;
	public int food;
	public int production;

	
	public Hex()
	{

	}
}

// Represents a single civilization present
// in the game. Civilizations have
// cities, and units.
// The territory of a civilization is identical to 
// the combined territories of all its cities.
public class Civilization 
{
	public List<City> cities;
	public Color territoryColor;
	public Color iconColor;
	public string name;
	public bool playerCiv;

	public Civilization()
	{
		cities = new List<City>();
	}

}

// Represents a single city under the control
// of a particular civilization. Cities have their own territories
// for the purposes of calculating resources etc.
// Once established, cities never change their coordinates.
// Cities are associated with a particular civilization at any
// given time.
public class City
{
	public Vector2I Centercoordinates;
	public Civilization civ;
	public List<Vector2I> territory;

	public City(Vector2I coordinates, Civilization civ)
	{
		this.Centercoordinates = coordinates;
		this.civ = civ;

		territory = new List<Vector2I>();
	}

}

public partial class HexTileMap : TileMap
{

	// Tile atlases
	TileSetAtlasSource iconAtlas;
	TileSetAtlasSource terrainAtlas;

	[Export]
	public int width = 106;
	[Export]
	public int height = 66;

	Dictionary<Vector2I, Hex> mapData;
	Dictionary<TerrainType, Vector2I> terrainTextures; // Maps terrain types to their textures in texture atlas

	[Export] // Debug only. To be removed and set up with code.
	FastNoiseLite noise;

	// For storing noise values during intermediate stages of terrain generation, allowing for adjustments
	// and setting terrain textures based on ratios rather than raw noise values
	float[,] noiseMap; 
	float[,] forestMap;
	float[,] desertMap;
	float[,] mountainMap;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Get the tile atlas of base color icons.
		// We will use this to change icons to a certain civilization's color
		// via alternative tiles.
		this.iconAtlas = (TileSetAtlasSource) TileSet.GetSource(2); // Takes in the ID number of the tile atlas as configured in the editor.
		this.terrainAtlas = (TileSetAtlasSource) TileSet.GetSource(0);

		////////////////////////
		// TERRAIN GENERATION //
		////////////////////////

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
		int seed = r.Next(100000);

		// Perlin noise terrain gen
		noiseMap = new float[width, height];
		forestMap = new float[width, height];
		desertMap = new float[width, height];
		mountainMap = new float[width, height];

		noise.Seed = seed;
		noise.FractalOctaves = 4;
		float noiseMax = 0f; // Keep track of range of noise

		// Configure different types of noise for different terrain
		// FOREST
		// For forests, we want smaller sploches like Voronoi/Cellular
		FastNoiseLite forestNoise = new FastNoiseLite();

		float forestNoiseMax = 0f;

		forestNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Cellular;
		forestNoise.Seed = seed;
		forestNoise.Frequency = 0.04f;
		forestNoise.FractalType = FastNoiseLite.FractalTypeEnum.Fbm;
		forestNoise.FractalLacunarity = 2f;

		// DESERT
		// For deserts, we want wide amorphous regions
		FastNoiseLite desertNoise = new FastNoiseLite();

		float desertNoiseMax = 0f;

		desertNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
		desertNoise.Seed = seed;
		desertNoise.Frequency = 0.015f;
		desertNoise.FractalType = FastNoiseLite.FractalTypeEnum.Fbm;
		desertNoise.FractalLacunarity = 2f;

		// MOUNTAIN
		// For mountains, we want a ridge-like fractal.
		FastNoiseLite mountainNoise = new FastNoiseLite();

		float mountainNoiseMax = 0f;

		mountainNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
		mountainNoise.Seed = seed;
		mountainNoise.Frequency = 0.02f;
		mountainNoise.FractalType = FastNoiseLite.FractalTypeEnum.Ridged;
		mountainNoise.FractalLacunarity = 2f;

		// Generate noise for ALL terrain types at once (we will apply them selectively based on each other later)
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				// Basic
				noiseMap[x, y] = Math.Abs(noise.GetNoise2D(x, y));
				if (noiseMap[x, y] > noiseMax) noiseMax = noiseMap[x, y];

				// Desert
				desertMap[x, y] = Math.Abs(desertNoise.GetNoise2D(x, y));
				if (desertMap[x, y] > desertNoiseMax) desertNoiseMax = desertMap[x, y];

				// Forest
				forestMap[x, y] = Math.Abs(forestNoise.GetNoise2D(x, y));
				if (forestMap[x, y] > forestNoiseMax) forestNoiseMax = forestMap[x, y];

				mountainMap[x, y] = mountainNoise.GetNoise2D(x, y);
				if (mountainMap[x, y] > mountainNoiseMax) mountainNoiseMax = mountainMap[x, y];
			}
		}


		// Calculate base terrain tile ratios.
		// Base terrain includes water, shallow water, beach, and plains.
		// This will form the basic structures of the continents.
		List<(float Min, float Max, TerrainType Type)> terrainGenValues = new List<(float Min, float Max, TerrainType Type)>
		{
			(0, noiseMax/10 * 2.5f, TerrainType.WATER),
			(noiseMax/10 * 2.5f, noiseMax/10 * 4, TerrainType.SHALLOW_WATER),
			(noiseMax/10 * 4, noiseMax/10 * 4.5f, TerrainType.BEACH),
			(noiseMax/10 * 4.5f, noiseMax + 0.05f, TerrainType.PLAINS)
		};

		// Forest gen values
		Vector2 forestGenValues = new Vector2(forestNoiseMax/10 * 7, forestNoiseMax + 0.05f);

		// Desert gen values
		Vector2 desertGenValues = new Vector2(desertNoiseMax/10 * 6, desertNoiseMax + 0.05f);

		// Mountain gen values
		Vector2 mountainGenValues = new Vector2(mountainNoiseMax/10 * 5.5f, mountainNoiseMax + 0.05f);

		// First pass: Basic terrain and water
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
				SetCell(1, new Vector2I(x, y), 1, new Vector2I(0, 0));
			}
		}

		// Second pass: Desert
		// Note that since tiles are already generated at this point, we dont need to instantiate them.
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				Hex h = mapData[new Vector2I(x, y)];
				float noiseValue = desertMap[x, y];
				if (desertMap[x, y] >= desertGenValues[0] && 
					desertMap[x, y] <= desertGenValues[1] &&
					h.terrainType == TerrainType.PLAINS) 
				{
					h.terrainType = TerrainType.DESERT;
					SetCell(0, new Vector2I(x, y), 0, terrainTextures[h.terrainType]);
				}
			}
		}

		// Third pass: Forest
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				Hex h = mapData[new Vector2I(x, y)];
				float noiseValue = forestMap[x, y];
				if (forestMap[x, y] >= forestGenValues[0] && 
					forestMap[x, y] <= forestGenValues[1] &&
					h.terrainType == TerrainType.PLAINS) 
				{
					h.terrainType = TerrainType.FOREST;
					SetCell(0, new Vector2I(x, y), 0, terrainTextures[h.terrainType]);
				}
			}
		}

		// Fourth pass: mountains
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				Hex h = mapData[new Vector2I(x, y)];
				float noiseValue = mountainMap[x, y];
				if (mountainMap[x, y] >= mountainGenValues[0] && 
					mountainMap[x, y] <= mountainGenValues[1] &&
					h.terrainType != TerrainType.WATER &&
					h.terrainType != TerrainType.SHALLOW_WATER) 
				{
					h.terrainType = TerrainType.MOUNTAIN;
					SetCell(0, new Vector2I(x, y), 0, terrainTextures[h.terrainType]);
				}
			}
		}		

		// Ice cap gen
		// Traverse the map on the x-axis.
		// For each x, place ice tiles a random number of tiles deep
		// from the top and bottom of the map
		int maxIce = 5;
		for (int x = 0; x < width; x++)
		{
			// North pole
			for (int y = 0; y < r.Next(maxIce) + 1; y++)
			{
				Hex h = mapData[new Vector2I(x, y)];
				h.terrainType = TerrainType.ICE;
				SetCell(0, new Vector2I(x, y), 0, terrainTextures[h.terrainType]);
			}

			// South pole
			for (int y = height - 1; y > (height - 1) - r.Next(maxIce) - 1; y--)
			{
				Hex h = mapData[new Vector2I(x, y)];
				h.terrainType = TerrainType.ICE;
				SetCell(0, new Vector2I(x, y), 0, terrainTextures[h.terrainType]);				
			}
		}



		///////////////////////////////////
		// TILE RESOURCES AND ATTRIBUTES //
		///////////////////////////////////
		
		// Populate tiles with food and production
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				Hex h = mapData[new Vector2I(x, y)]; // Get hex at the coordinate
				
				// Resource spawning logic
				switch (h.terrainType)
				{
					case TerrainType.PLAINS:
						h.food = r.Next(2, 6);
						h.production = r.Next(0, 3);
						break;
					case TerrainType.FOREST:
						h.food = r.Next(1, 4);
						h.production = r.Next(2, 6);
						break;
					case TerrainType.DESERT:
						h.food = r.Next(0, 2);
						h.production = r.Next(0, 2);
						break;
					case TerrainType.BEACH:
						h.food = r.Next(0, 4);
						h.production = r.Next(0, 2);
						break;
					default:
						h.food = 0;
						h.production = 0;
						break;
				}
			}
		}


		//////////////////////////////
		// CIVILIZATIONS AND CITIES //
		//////////////////////////////
		
		// Create player civilization and starting city

	}

	Vector2I currentSelectedCell = new Vector2I(-1, -1); // Representation of non-selected cell

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("left_click")) { // Map controls
			Vector2I mapCoords = LocalToMap(ToLocal(GetGlobalMousePosition()));
			
			if (mapCoords.X >= 0 && mapCoords.X < width && mapCoords.Y >= 0 && mapCoords.Y < height)
			{
				if (mapCoords != currentSelectedCell) { // If the clicked area differs from current selection, unselect current
					SetCell(2, currentSelectedCell, -1);
				}
				
				SetCell(2, mapCoords, 1, new Vector2I(0, 1));

				currentSelectedCell = mapCoords; // Update current

				GD.Print("Coordinates: " + mapCoords);
				GD.Print("Terrain: " + mapData[mapCoords].terrainType);
				GD.Print("Food: " + mapData[mapCoords].food);
				GD.Print("Production: " + mapData[mapCoords].production);

				GD.Print("Neighbors: " +  GetSurroundingCells(mapCoords));
				GD.Print(GetNeighborCell(mapCoords, TileSet.CellNeighbor.TopRightSide)); // Test of neighbor cell
			} else {
				SetCell(2, currentSelectedCell, -1);
			}


		}
	}
}
