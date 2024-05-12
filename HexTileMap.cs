using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public enum TerrainType { PLAINS, WATER, DESERT, MOUNTAIN, ICE, SHALLOW_WATER, FOREST, BEACH, CIV_COLOR_BASE }

public partial class Hex
{

	public readonly Vector2I coordinate; // The coordinate of this hex on the map. Should not change.

	// Tile attributes
	public TerrainType terrainType;
	public int food;
	public int production;

	public Civilization ownerCiv;
	public City ownerCity;
	
	public Hex(Vector2I coord)
	{
		coordinate = coord;
		ownerCiv = null;
		ownerCity = null;
	}
}

public partial class HexTileMap : TileMap
{

	// PackedScenes
	PackedScene cityScene;


	// Tile atlases
	TileSetAtlasSource iconAtlas;
	TileSetAtlasSource terrainAtlas;


	// Signals
	[Signal]
	// A signal that sets the camera to the desired position and zoom from in game
	public delegate void SetCameraEventHandler(Vector2 pos, Vector2 zoom); 
	[Signal]
	public delegate void SendCityUIInfoEventHandler(City c); // Sends city info to the UI
	[Signal]
	public delegate void SendTerrainUIInfoEventHandler(TerrainType t, int f, int p); // Sends terrain info to the UI
	[Signal]
	public delegate void ClickOffMapEventHandler(); // Signals that a click off map has occurred

	// Note this signal is in pure C# due to Hex not being a Godot variant type
	public delegate void RightClickOnMapEventHandler(Hex h);
	public event RightClickOnMapEventHandler RightClickOnMap;

	/////////////////////
	// GAME PARAMETERS //
	/////////////////////

	[Export]
	public int width = 106;
	[Export]
	public int height = 66;

	[Export]
	public int NUM_AI_CIVS = 0;


	// MAP DATA
	Dictionary<Vector2I, Hex> mapData;
	Dictionary<TerrainType, Vector2I> terrainTextures; // Maps terrain types to their textures in texture atlas

	// MAP PARAMETERS
	[Export]
	float TERRAIN_FREQUENCY = 0.008f;
	[Export]
	float TERRAIN_LACUNARITY = 2.25f;

	// GAMEPLAY DATA
	List<Civilization> civs;
	Dictionary<Vector2I, City> cities;

	Unit currentSelectedUnit;
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Load packed scenes
		cityScene = ResourceLoader.Load<PackedScene>("City.tscn");

		// Get the tile atlas of base color icons.
		// We will use this to change icons to a certain civilization's color
		// via alternative tiles.
		this.iconAtlas = (TileSetAtlasSource) TileSet.GetSource(2); // Takes in the ID number of the tile atlas as configured in the editor.
		this.terrainAtlas = (TileSetAtlasSource) TileSet.GetSource(0);

		// Terrain gen step
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
			{ TerrainType.ICE, new Vector2I(0, 3) },	
			{ TerrainType.CIV_COLOR_BASE, new Vector2I(0, 3)}	
		};

		// TERRAIN GEN
		GenerateTerrain();
		
		// RESOURCE GEN
		GenerateResources();

		
		// CIVILIZATIONS AND CITIES GEN
		// TODO:
		// - Make sure civs dont overlap (via logic in the gen starts function)
		// - Make sure civs start on a reasonably sized piece of land
		civs = new List<Civilization>();
		cities = new Dictionary<Vector2I, City>();

		// Create player civilization and starting city
		Civilization playerCiv = new Civilization();
		playerCiv.id = 0;
		playerCiv.playerCiv = true;
		playerCiv.SetRandomColor();

		// Create alt tiles for each civ's territory color.
		// We base this off the ice tiles since they are almost white and
		// can so can be modulated to other colors.

		// Note that the alpha modulation (transparency) for these tiles are set in the TileMap layer they are
		// assigned to, so we don't need to do that here.
		int id = terrainAtlas.CreateAlternativeTile(terrainTextures[TerrainType.CIV_COLOR_BASE]); // Part of civ gen
		terrainAtlas.GetTileData(terrainTextures[TerrainType.CIV_COLOR_BASE], id).Modulate = playerCiv.territoryColor; // Part of civ gen
		
		playerCiv.territoryColorAltTileId = id;

		// Create player city
		CreateCity(playerCiv, GenerateCivStartingLocations(1)[0], "Player City");

		// Go through the same process for all AI civs.
		GenerateAICivs();


		// CAMERA ETC SETUP
		// Send signal to camera to center on player city to start.
		EmitSignal(SignalName.SetCamera, ToGlobal(MapToLocal(playerCiv.cities[0].centerCoordinates)), new Vector2(0.5f, 0.5f));

	}

	Vector2I currentSelectedCell = new Vector2I(-1, -1); // Representation of non-selected cell

    public override void _UnhandledInput(InputEvent @event)
    {
		if (@event is InputEventMouseButton mouse) { // Map mouse controls
			Vector2I mapCoords = LocalToMap(ToLocal(GetGlobalMousePosition()));

			if (mapCoords.X >= 0 && mapCoords.X < width && mapCoords.Y >= 0 && mapCoords.Y < height) // If click is in bounds of the map
			{
				Hex h = mapData[mapCoords]; // Get the clicked hex
				if (mouse.ButtonMask == MouseButtonMask.Left)
				{

					// Send signals for UI, etc.
					
					// If the tile is a city
					// TODO: check if player city
					if (cities.ContainsKey(mapCoords))
					{
						EmitSignal(SignalName.SendCityUIInfo, cities[mapCoords]);
					} else { // Tile is not a city
						EmitSignal(SignalName.SendTerrainUIInfo, (int) h.terrainType, h.food, h.production);
					}


					if (mapCoords != currentSelectedCell) { // If the clicked area differs from current selection, unselect current
						SetCell(3, currentSelectedCell, -1);
					}
					
					SetCell(3, mapCoords, 1, new Vector2I(0, 1));

					currentSelectedCell = mapCoords; // Update current

					// GD.Print("Coordinates: " + mapCoords);
					// GD.Print("Terrain: " + mapData[mapCoords].terrainType);
					// GD.Print("Food: " + mapData[mapCoords].food);
					// GD.Print("Production: " + mapData[mapCoords].production);
					// GD.Print("Neighbors: " +  GetSurroundingCells(mapCoords));
				}

				if (mouse.ButtonMask == MouseButtonMask.Right)
				{
					RightClickOnMap?.Invoke(h); // Emit right click on map signal (note this is raw C# and not the Godot system)
					// The '?' notation checks that there are subscribers to the event to avoid null references.
				}

			} else { // Click off map occurred
				EmitSignal(SignalName.ClickOffMap);
				DeselectCurrentCell();
			}
		} 
    }

	// Deselects the current selected cell visually
	public void DeselectCurrentCell()
	{
		SetCell(3, currentSelectedCell, -1);
	}

	// Overload for compatibility with unit click event
	public void DeselectCurrentCell(Unit u = null)
	{
		SetCell(3, currentSelectedCell, -1);
	}


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}

	// Generates valid starting locations for a given number of civilizations to be placed in
	// at the start of the game, avoiding overlap between them.
	// Returns the starting locations as a list of Vector2I hex coordinates.

	// POTENTIAL ISSUES:
	// - Infinite loop if no valid location is found
	public List<Vector2I> GenerateCivStartingLocations(int numLocations)
	{
		List<Vector2I> locations = new List<Vector2I>();
		List<Vector2I> plainsTiles = new List<Vector2I>();

		// Before we begin, we know that we want civs to spawn only on PLAINS terrain,
		// so to narrow our search space we first iterate through the map and gather the coordinates
		// that are PLAINS terrain.
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y ++)
			{
				if (mapData[new Vector2I(x, y)].terrainType == TerrainType.PLAINS)
					plainsTiles.Add(new Vector2I(x, y));
			}
		}

		Random r = new Random();
		for (int i = 0; i < numLocations; i++)
		{
			// Generate a coordinate
			Vector2I coord = new Vector2I();  // Pick a random plains tile.

			bool valid = false;
			int counter = 0; // prevent infinite loop. For now, if after 100 attempts no suitable locations are found, accept unsuitable location.

			while (!valid && counter < 100)
			{
				coord = plainsTiles[r.Next(plainsTiles.Count)];
				valid = IsValidLocation(coord, locations);
				counter++;
			}
			
			locations.Add(coord);
		}

		return locations;
	}

	// Checks whether a given coordinate is valid.
	// locations should be a list of already generated city coordinates
	private bool IsValidLocation(Vector2I coord, List<Vector2I> locations)
	{
		// Check that coordinate is not too close to map edge,
		// as this can cause bugs due to territory extending beyond the map edge.
		if (coord.X < 3 || coord.X > width - 3 ||
			coord.Y < 3 || coord.Y > height - 3)
		{
			return false;
		}

		foreach (Vector2I l in locations) // Check cities aren't too close
		{
			if (Math.Abs(coord.X - l.X) < 3 || Math.Abs(coord.Y - l.Y) < 3)
				return false;  // The city is too close to another
		}		

		return true;
	}

	public void CreateCity(Civilization civ, Vector2I coords, string name)
	{
		City city = cityScene.Instantiate() as City;
		city.map = this; // Give city a reference to the map
		civ.cities.Add(city); // Register city with civilization object
		city.civ = civ;
		// Attach to scene tree
		AddChild(city);

		city.SetIconColor(civ.territoryColor); 

		city.SetName(name);

		city.centerCoordinates = coords;
		city.AddTerritory(new List<Hex>{mapData[coords]}); // Add city center coordinate
		city.AddTerritory(GetSurroundingHexes(coords)); // Add starting surrounding tiles

		// Convert map coords to local space coordinates to place city node.
		city.Position = MapToLocal(coords);

		UpdateCivTerritoryMap(civ);

		cities[coords] = city; // Add to cities lookup table for map
	}

	// Like GetSurroundingCells(), but returns the Hex objects for those cells
	// instead of just coordinates.
	// Will throw out coordinates that are not in map bounds
	public List<Hex> GetSurroundingHexes(Vector2I coords)
	{
		List<Hex> result = new List<Hex>();
		foreach (Vector2I coord in GetSurroundingCells(coords))
		{
			if (HexInBounds(coord))
				result.Add(mapData[coord]);
		}

		return result;
	}

	public void GenerateAICivs()
	{
		List<Vector2I> civStarts = GenerateCivStartingLocations(NUM_AI_CIVS);
		
		for (int i = 0; i < civStarts.Count; i++)
		{
			Civilization currentCiv = new Civilization();
			currentCiv.id = i + 1; // id 0 reserved for player
			currentCiv.playerCiv = false;
			currentCiv.SetRandomColor();

			civs.Add(currentCiv);

			// Setup alt tile colors
			int id = terrainAtlas.CreateAlternativeTile(terrainTextures[TerrainType.CIV_COLOR_BASE]); // Part of civ gen
			terrainAtlas.GetTileData(terrainTextures[TerrainType.CIV_COLOR_BASE], id).Modulate = currentCiv.territoryColor; // Part of civ gen
		
			currentCiv.territoryColorAltTileId = id;

			CreateCity(currentCiv, civStarts[i], "City " + civStarts[i].X);
		}
	}

	// Iterates through all civs and cities,
	// refreshing representation of territory
	public void UpdateAllCivsTerritoryMap()
	{
		foreach (Civilization civ in civs)
		{
			UpdateCivTerritoryMap(civ);
		}
	}

	// Updates the territory representation on the map for a single civilization
	public void UpdateCivTerritoryMap(Civilization civ)
	{
		foreach (City c in civ.cities)
		{
			foreach (Hex h in c.territory)
			{
				SetCell(2, h.coordinate, 0, terrainTextures[TerrainType.CIV_COLOR_BASE], civ.territoryColorAltTileId);
			}
		}
	}

	///////////////////////////////////
	// TILE RESOURCES AND ATTRIBUTES //
	///////////////////////////////////
	public void GenerateResources()
	{
		Random r = new Random();

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
	}

	////////////////////////
	// TERRAIN GENERATION //
	////////////////////////
	public void GenerateTerrain()
	{

		// For storing noise values during intermediate stages of terrain generation, allowing for adjustments
		// and setting terrain textures based on ratios rather than raw noise values
		float[,] noiseMap = new float[width, height];
		float[,] forestMap = new float[width, height];
		float[,] desertMap = new float[width, height];
		float[,] mountainMap = new float[width, height];

		Random r = new Random();
		int seed = r.Next(100000);

		// BASE TERRAIN (Water, Beach, Plains)
		FastNoiseLite noise = new FastNoiseLite();

		float noiseMax = 0f; // Keep track of range of noise

		noise.Seed = seed;
		noise.Frequency = TERRAIN_FREQUENCY;
		noise.FractalType = FastNoiseLite.FractalTypeEnum.Fbm;
		noise.FractalOctaves = 4;
		noise.FractalLacunarity = TERRAIN_LACUNARITY;


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
				Hex h = new Hex(new Vector2I(x, y));
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

	}

	// Returns a the Hex object associated with a given map coordinate.
	public Hex GetHex(Vector2I coords)
	{
		return mapData[coords];
	}

	// Returns whether a given coordinate is in bounds of the map.
	public bool HexInBounds(Vector2I coords)
	{
		if (coords.X < 0 || coords.X >= width ||
			coords.Y < 0 || coords.Y >= height)
			return false;

		return true;
	}
}
