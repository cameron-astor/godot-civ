using Godot;
using System;
using System.Collections.Generic;

public partial class TerrainTileUI : Panel
{

	// SHARED RESOURCES

	// Mappings for terrain type names
	public static Dictionary<TerrainType, string> terrainTypeStrings = new Dictionary<TerrainType, string>
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

	public static Dictionary<TerrainType, Texture2D> terrainTypeImages = new Dictionary<TerrainType, Texture2D>();

	public static void LoadTerrainImages()
	{
		// Set up terrain images
		Texture2D plains = (Texture2D) ResourceLoader.Load("res://textures/plains.jpg");
		Texture2D beach = (Texture2D) ResourceLoader.Load("res://textures/beach.jpg");
		Texture2D desert = (Texture2D) ResourceLoader.Load("res://textures/desert.jpg");
		Texture2D mountain = (Texture2D) ResourceLoader.Load("res://textures/mountain.jpg");
		Texture2D ice = (Texture2D) ResourceLoader.Load("res://textures/ice.jpg");
		Texture2D ocean = (Texture2D) ResourceLoader.Load("res://textures/ocean.jpg");
		Texture2D shallow = (Texture2D) ResourceLoader.Load("res://textures/shallow.jpg");
		Texture2D forest = (Texture2D) ResourceLoader.Load("res://textures/forest.jpg");

		terrainTypeImages = new Dictionary<TerrainType, Texture2D> // Terrain to image dict
		{
			{ TerrainType.PLAINS, plains },
			{ TerrainType.BEACH,  beach },
			{ TerrainType.DESERT, desert },
			{ TerrainType.MOUNTAIN, mountain },
			{ TerrainType.ICE, ice },
			{ TerrainType.WATER, ocean },
			{ TerrainType.SHALLOW_WATER, shallow },
			{ TerrainType.FOREST, forest },
		};
	}


	// Data hex
	Hex h = null;

	// UI Components
	Label terrainLabel;
	Label foodLabel;
	Label productionLabel;
	TextureRect terrainImage;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		terrainLabel = GetNode<Label>("TerrainLabel");
		foodLabel = GetNode<Label>("FoodLabel");
		productionLabel = GetNode<Label>("ProductionLabel");
		terrainImage = GetNode<TextureRect>("TerrainImage");
	}

	public void SetHex(Hex h)
	{
		this.h = h;
		Refresh();
	}

	public void Refresh()
	{
		terrainImage.Texture = terrainTypeImages[h.terrainType];
		terrainLabel.Text = "Terrain: " + terrainTypeStrings[h.terrainType];
		foodLabel.Text = "Food: " + h.food;
		productionLabel.Text = "Production: " + h.production;		
	}
	
}
