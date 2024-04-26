using Godot;
using System;
using System.Collections.Generic;

public partial class TerrainTileUI : Panel
{

	// UI Components
	Label terrainLabel;
	Label foodLabel;
	Label productionLabel;

	// Mappings for terrain type names
	Dictionary<TerrainType, string> terrainTypeStrings = new Dictionary<TerrainType, string>
	{
		{ TerrainType.PLAINS, "Plains" },
		{ TerrainType.BEACH,  "Beach" },
		{ TerrainType.DESERT, "Desert" },
		{ TerrainType.MOUNTAIN, "Mountain" },
		{ TerrainType.ICE, "Ice" },
		{ TerrainType.WATER, "Water" },
		{ TerrainType.SHALLOW_WATER, "Shallow Water" },
		{ TerrainType.FOREST, "Forest" },
	};

	// Mappings for terrain images
	Dictionary<TerrainType, Texture2D> terrainTypeImages;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		terrainLabel = GetNode<Label>("TerrainLabel");
		foodLabel = GetNode<Label>("FoodLabel");
		productionLabel = GetNode<Label>("ProductionLabel");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetTerrainUI(TerrainType ttype, int food, int prod)
	{
		terrainLabel.Text = "Terrain: " + terrainTypeStrings[ttype];
		foodLabel.Text = "Food: " + food;
		productionLabel.Text = "Production: " + prod;
	}

	
}
