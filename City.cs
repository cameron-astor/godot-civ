using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// Represents a single city under the control
// of a particular civilization. Cities have their own territories
// for the purposes of calculating resources etc.
// Once established, cities never change their coordinates.
// Cities are associated with a particular civilization at any
// given time.
public partial class City : Node2D
{

	public HexTileMap map; // Reference to the main map

	public Vector2I centerCoordinates; // Coordinates in grid hexes of the city center
	public Civilization civ; // The civ this city belongs to
	public List<Hex> territory; // The territory this city controls on the map (hex coordinates)

	public List<Hex> borderTilePool; // Potential tiles for expansion. Cached for efficiency.

	

	// Gameplay constants
	public int POPULATION_THRESHOLD_INCREASE = 0; // amount to increase the threshold each time population grows.

	// City attributes
	public string name; // Name of city to be displayed on the label


	// Units
	public List<Unit> unitBuildQueue; // Queue of units to be built
	public Unit currentUnitBeingBuilt;
	public int unitBuildTracker;


	// Population
	public int population; // City population
	public int populationGrowthThreshold;
	public int populationGrowthTracker;

	// Resources
	public int totalFood;
	public int totalProduction;

	// Label
	Label label;

	// Sprite
	Sprite2D sprite;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Graphics
		label = GetNode<Label>("Label");
		sprite = GetNode<Sprite2D>("Sprite2D");

		label.Text = name;

		// Gameplay data
		territory = new List<Hex>();
		borderTilePool = new List<Hex>();
		unitBuildQueue = new List<Unit>();

		population = 1; // Default starting population
		populationGrowthThreshold = 20; // Default starting growth threshold
		populationGrowthTracker = 0; // Start growth tracker at 0

		unitBuildTracker = 0;

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		ProcessTurn();
	}

	public void AddTerritory(List<Hex> territoryToAdd)
	{
		foreach (Hex h in territoryToAdd) // Set ownership of hex
		{
			h.ownerCity = this;
			h.ownerCiv = this.civ;

			// Add new border hexes to border tile pool
			AddValidNeighborsToBorderPool(h);

		}

		territory.AddRange(territoryToAdd);
		CalculateTerritoryResourceTotals();
	}

	public void SetName(string newName)
	{
		name = newName;
		label.Text = newName;
	}

	public void SetIconColor(Color c)
	{
		sprite.Modulate = c;
	}

	// Recalculates the total food and production values 
	// in the city's territory.
	public void CalculateTerritoryResourceTotals()
	{
		totalFood = 0;
		totalProduction = 0;
		foreach (Hex h in territory)
		{
			totalFood += h.food;
			totalProduction += h.production;
		}
	}

	// Performs necessary actions for the city's turn.
	// Includes population growth, building/spawning units, etc.
	public void ProcessTurn()
	{
		// Population and food
		populationGrowthTracker += totalFood;
		if (populationGrowthTracker > populationGrowthThreshold) // If the growth threshold is crossed, increase population
		{
			population++;
			populationGrowthTracker = 0;
			populationGrowthThreshold += POPULATION_THRESHOLD_INCREASE; // Increase threshold

			// Grow territory
			AddRandomNewTile();
			map.UpdateCivTerritoryMap(civ);

			// Work on building units
			ProcessUnitBuildQueue();
		}
	}

	// Randomly adds a new tile to the city's territory.
	// References the map to make sure the tile is in bounds,
	// not on impassible terrain, etc.
	// This is for population growth.
	public void AddRandomNewTile()
	{
		if (borderTilePool.Count > 0)
		{
			Random r = new Random();
			int index = r.Next(borderTilePool.Count);
			this.AddTerritory( new List<Hex>{borderTilePool[index]} );
			borderTilePool.RemoveAt(index);
		} else {
			// GD.Print("No possible tiles to add. " + this.name);
		}
	}

	public bool IsValidNeighborTile(Hex n)
	{
		if ( n.terrainType == TerrainType.WATER || 
			n.terrainType == TerrainType.ICE || 
			n.terrainType == TerrainType.SHALLOW_WATER || 
			n.terrainType == TerrainType.MOUNTAIN ) // Check that hex is valid terrain
		{
			return false;
		}

		if ( n.ownerCiv != null && n.ownerCity != null) // Check that hex is not already owned
			return false;

		if ( !map.HexInBounds(n.coordinate) ) // Check that hex is in map bounds
			return false;

		return true;
	}

	public void AddValidNeighborsToBorderPool(Hex h)
	{
		// Acquire neighbors
		List<Hex> neighbors = map.GetSurroundingHexes(h.coordinate);

		// Now that we have all of the neighbors, of all tiles, we need to filter out those that we don't
		// want in the new tile pool.
		// This includes: cells which belong to this civ already or to another civ, and tiles with impassible terrain.	

		foreach (Hex n in neighbors)
		{
			if (IsValidNeighborTile(n)) { borderTilePool.Add(n); } // If after all the checks the hex is still valid, add to pool.
		}		
	}

	// During turn processing, units in the build queue will be processed.
	//
	// If after adding all city production to the current build progress,
	// the unit's build cost is not met or exceeded, performs the addition and does nothing else.
	//
	// If after adding production, the unit's build cost IS met or exceeded, spawns the unit, 
	// queues up the next unit (if there is one), and adds the remainder of production not spent
	// on the first unit to the build progress of the next unit (if there is one).
	//
	// If there are no units in the build queue, does nothing.
	public void ProcessUnitBuildQueue()
	{
		if (unitBuildQueue.Count > 0) // If there are units to process, do nothing.
		{
			// A new unit is added to the queue
			if (currentUnitBeingBuilt == null) 
			{
				currentUnitBeingBuilt = unitBuildQueue[0]; 
			}

			unitBuildTracker += totalProduction; // Add city production to unit build tracker

			if (unitBuildTracker >= currentUnitBeingBuilt.productionRequired) // if the unit finishes building
			{
				// Spawn unit


				// Adjust queue
				unitBuildQueue.RemoveAt(0); // remove the current unit
				currentUnitBeingBuilt = null; // reset current unit being built

			} // If the unit is not finished yet, nothing else needs to be done.
		}
	}

	public void SpawnUnit(Unit u)
	{
		
	}

	// TODO: Replace with UnitType. UI should not need to know anything about units.
	// Maybe store another static mapping in the Unit class for this.
	public void AddUnitToBuildQueue(Unit u)
	{
		unitBuildQueue.Add(u);
	}

}
