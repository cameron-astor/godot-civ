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
	Area2D collider;

	// Gameplay variables
	public string unitName = "DEFAULT";
	public UnitType unitType;
	public Civilization civ;
	public int maxHp;
	public int hp;
	public int maxMovePoints;
	public int movePoints;
	public int productionRequired;
	public Vector2I coords = new Vector2I();


	public static Dictionary<Type, PackedScene> unitSceneResources;


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

	public static void LoadUnitScenes()
	{
		unitSceneResources = new Dictionary<Type, PackedScene> {
			{ typeof(Settler), ResourceLoader.Load<PackedScene>("res://Settler.tscn") },
			{ typeof(Warrior), ResourceLoader.Load<PackedScene>("res://Warrior.tscn") }
		};
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		collider = GetNode<Area2D>("Sprite2D/Area2D");
		UIManager manager = GetNode<UIManager>("/root/Game/CanvasLayer/UiManager");

		// Connect signal to UIManager
		this.UnitClicked += manager.SetUnitUI;
		this.UnitClicked += GetNode<HexTileMap>("/root/Game/HexTileMap").DeselectCurrentCell;
		
	}

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouse && mouse.ButtonMask == MouseButtonMask.Left)
		{
			var spaceState = GetWorld2D().DirectSpaceState;
			var point = new PhysicsPointQueryParameters2D();
			point.CollideWithAreas = true;
			point.Position = GetGlobalMousePosition();
			var result = spaceState.IntersectPoint(point);
			if (result.Count > 0 && (Area2D) result[0]["collider"] == collider) // There is a click on this unit
			{
				EmitSignal(SignalName.UnitClicked, this);
				GetViewport().SetInputAsHandled(); // Consume input
			}			
		}
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		// if (Input.IsActionJustPressed("left_click")) // Check for mouse clicks on unit
		// {
		// 	var spaceState = GetWorld2D().DirectSpaceState;
		// 	var point = new PhysicsPointQueryParameters2D();
		// 	point.CollideWithAreas = true;
		// 	point.Position = GetGlobalMousePosition();
		// 	var result = spaceState.IntersectPoint(point);
		// 	if (result.Count > 0 && (Area2D) result[0]["collider"] == collider) // There is a click on this unit
		// 	{
		// 		EmitSignal(SignalName.UnitClicked, this);
		// 	}

		// }
	}
}
