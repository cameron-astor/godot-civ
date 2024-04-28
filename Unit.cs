using Godot;
using System;
using System.Collections.Generic;

public enum UnitType { SETTLER, WARRIOR }

public partial class Unit : Node2D
{

	[Signal]
	public delegate void UnitClickedEventHandler(Unit u);

	// Mapping of unit type to icon resource location.
	// Actual loading of texture will take place in unit subclasses.
	public static Dictionary<UnitType, Texture2D> icons;

	// Likewise for UI images
	public static Dictionary<UnitType, Texture2D> ui_images;
	private static bool texturesLoaded = false;

	// Unit collision area
	CollisionShape2D collider;

	// Gameplay variables
	public string unitName = "DEFAULT";
	public UnitType unitType;
	public Civilization civ;
	public int maxHp;
	public int hp;
	public int maxMovePoints;
	public int movePoints;


	// Loads unit textures for all derived classes to use.
	public static void LoadTextures()
	{
		if (!texturesLoaded)
		{
			icons = new Dictionary<UnitType, Texture2D>
			{
				{ UnitType.SETTLER, (Texture2D) ResourceLoader.Load("res://textures/settler.png") },
				{ UnitType.WARRIOR, (Texture2D) ResourceLoader.Load( "res://textures/warrior.png") }
			};	

			ui_images = new Dictionary<UnitType, Texture2D>
			{
				{ UnitType.SETTLER, (Texture2D) ResourceLoader.Load("res://textures/settler_image.png") },
				{ UnitType.WARRIOR, (Texture2D) ResourceLoader.Load( "res://textures/warrior_image.jpg") }		
			};
		}
		texturesLoaded = true;
	}


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		collider = GetNode<CollisionShape2D>("Sprite2D/Area2D/CollisionShape2D");
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
