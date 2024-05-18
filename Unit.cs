using Godot;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

public enum UnitType { SETTLER, WARRIOR }

public partial class Unit : Node2D
{

	[Signal]
	public delegate void UnitClickedEventHandler(Unit u);
	public delegate void SelectedUnitDestroyedEventHandler();
    public event SelectedUnitDestroyedEventHandler SelectedUnitDestroyed;

	// Mapping of unit type to icon resource location.
	// Actual loading of texture will take place in unit subclasses.
	public static Dictionary<UnitType, Texture2D> icons;

	// Likewise for UI images
	public static Dictionary<UnitType, Texture2D> ui_images;
	private static bool texturesLoaded = false;

	// Unit collision area
	Area2D collider;

	// Map reference
	public HexTileMap map;
	public bool selected = false;

	// Unit properties
	public HashSet<TerrainType> impassible = new HashSet<TerrainType>(); // Terrain that is considered impassible by this unit.
																		 // Default empty, so nothing impassible.
	
	// Default impassible terrain for ground units
	public static HashSet<TerrainType> GroundUnitsDefaultImpassible()
	{
		return new HashSet<TerrainType>{
			TerrainType.ICE,
			TerrainType.MOUNTAIN,
			TerrainType.WATER,
			TerrainType.SHALLOW_WATER
		};
	}
	
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


	// Movement
	List<Hex> validMovementHexes = new List<Hex>();

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

	// Sets the unit's ownership to a certain faction and colors the unit accordingly
	public void SetCiv(Civilization civ)
	{
		this.civ = civ;
		
		// Set color
		GetNode<Sprite2D>("Sprite2D").Modulate = civ.territoryColor;
	}

	public void SetSelected()
	{
		selected = true;

		// Calculate movement ranges ahead of time
		validMovementHexes = CalculateValidAdjacentMovementHexes();

		Sprite2D sprite = GetNode<Sprite2D>("Sprite2D");
		Color c = new Color(sprite.Modulate);
		c.V = c.V - 0.25f;
		sprite.Modulate = c;
	}

	public void SetDeselected()
	{
		selected = false;

		validMovementHexes.Clear(); // Clear movement adjacency data on deselect

		// Set color
		GetNode<Sprite2D>("Sprite2D").Modulate = civ.territoryColor;
	}

	public void MoveToHex(Hex h)
	{
		Position = map.MapToLocal(h.coordinate);
		coords = h.coordinate; // Update unit coords
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Collider and UI setup
		collider = GetNode<Area2D>("Sprite2D/Area2D");
		UIManager manager = GetNode<UIManager>("/root/Game/CanvasLayer/UiManager");
		// Connect signals
		this.UnitClicked += manager.SetUnitUI;
		manager.EndTurn += this.ProcessTurn;
		this.SelectedUnitDestroyed += manager.HideAllPopups;

		// Map setup and map signals
		map = GetNode<HexTileMap>("/root/Game/HexTileMap");
		// Every unit must subscribe to map clicke events to know when to move, etc.
		this.UnitClicked += map.DeselectCurrentCell;
		map.RightClickOnMap += Move; // Unit movement signal
		
	}

	// Calculates the adjacent tiles that are currently valid for 
	// moving this unit into.
	public List<Hex> CalculateValidAdjacentMovementHexes()
	{
		List<Hex> hexes = new List<Hex>();

		hexes.AddRange(map.GetSurroundingHexes(this.coords));
		hexes = hexes.Where(h => !impassible.Contains(h.terrainType)).ToList(); // Filter out impassible terrain

		return hexes;
	}

	public void Move(Hex h)
	{
		if (selected && movePoints > 0) // If unit is not selected, do nothing
		{
			if (validMovementHexes.Contains(h))
			{
				MoveToHex(h);
				validMovementHexes = CalculateValidAdjacentMovementHexes();
				movePoints -= 1;
				EmitSignal(SignalName.UnitClicked, this); // refresh unit UI
			}
		}
	}

	public void ProcessTurn()
	{
		movePoints = maxMovePoints; // Reset movement points every turn
	}

	public void DestroyUnit()
	{	
		// Disconnect signals
		map.RightClickOnMap -= Move;

		if (selected)
		{
			SelectedUnitDestroyed?.Invoke();
		}

		this.QueueFree();
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
				SetSelected();
				GetViewport().SetInputAsHandled(); // Consume input
			}	else {
				SetDeselected(); // Unit not clicked
			}		
		}
    }

}
