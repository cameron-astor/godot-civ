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
	public List<Vector2I> territory; // The territory this city controls on the map (hex coordinates)


	// Gameplay constants
	public int POPULATION_THRESHOLD_INCREASE = 20; // amount to increase the threshold each time population grows.

	// City attributes
	public string name; // Name of city to be displayed on the label

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
		territory = new List<Vector2I>();

		population = 1; // Default starting population
		populationGrowthThreshold = 20; // Default starting growth threshold
		populationGrowthTracker = 0; // Start growth tracker at 0

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public void AddTerritory(List<Vector2I> territoryToAdd)
	{
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
		foreach (Vector2I coord in territory)
		{
			Hex h = map.GetHex(coord);
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
		}
	}

	public void GrowTerritory()
	{
		
	}

}
