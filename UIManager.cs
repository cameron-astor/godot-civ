using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// The UI Manager is the top level node for the user interface.
/// It handles tasks that affect the entire UI, such as hiding and revealing windows.
/// It takes in all UI related signals from gameplay objects, abstracting over the particular
/// implementation and components of the UI.
/// </summary>
public partial class UIManager : Node
{

	// Packed scenes
	PackedScene cityUiScene;
	PackedScene terrainUiScene;
	PackedScene unitUiScene;

	TerrainTileUI terrainUi;
	CityUI cityUI;
	UnitUI unitUi;
	GeneralUI generalUi;

	[Signal]
	public delegate void EndTurnEventHandler();


	// All UIs
	// List<Control> allUIs = new List<Control>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		cityUiScene = ResourceLoader.Load<PackedScene>("CityUI.tscn");
		terrainUiScene = ResourceLoader.Load<PackedScene>("TerrainTileUI.tscn");
		unitUiScene = ResourceLoader.Load<PackedScene>("UnitUI.tscn");

		// Get UI panels
		terrainUi = (TerrainTileUI) GetNode<Panel>("TerrainTileUi");
		cityUI = (CityUI) GetNode<Control>("CityUI");
		unitUi = (UnitUI) GetNode<Panel>("UnitUi");
		generalUi = (GeneralUI) GetNode<Panel>("GeneralUi");

		// allUIs.AddRange(new List<Control>{terrainUi, cityUI, unitUi, generalUi});

		// Attach EndTurn signal to end turn button
		Button endTurnButton = generalUi.GetNode<Button>("EndTurnButton");
		endTurnButton.Pressed += SignalEndTurn;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SignalEndTurn()
	{
		EmitSignal(SignalName.EndTurn);
		generalUi.IncrementTurnCounter();
		RefreshUI();
	}

	// Hides all UI windows that are not permanently fixed to the screen.
	// Used when mouse clicks out of bounds, for instance.
	public void HideAllPopups()
	{
		terrainUi.Visible = false;
		unitUi.Visible = false;
		cityUI.Visible = false;
	}

	public void SetTerrainUI(Hex h)
	{
		HideAllPopups();

		if (terrainUi is not null)
		{
			terrainUi.QueueFree();

			terrainUi = (TerrainTileUI) terrainUiScene.Instantiate();
			AddChild(terrainUi);
			terrainUi.SetHex(h);
			terrainUi.Visible = true;
		}
	}

	public void SetCityUI(City c)
	{
		HideAllPopups();

		cityUI.QueueFree(); // destroy current city ui

		cityUI = (CityUI) cityUiScene.Instantiate(); // Create new city ui
		AddChild(cityUI);
		cityUI.SetCityUI(c);
		cityUI.Visible = true;
	}

	public void SetUnitUI(Unit u)
	{
		HideAllPopups();

		unitUi.QueueFree();

		unitUi = (UnitUI) unitUiScene.Instantiate();
		AddChild(unitUi);
		unitUi.SetUnit(u);
		unitUi.Refresh();
		unitUi.Visible = true;
	}

	// Refreshes the current visible UIs to be reflective of current data.
	public void RefreshUI()
	{
		if (cityUI.Visible)
			cityUI.Refresh();
		if (terrainUi.Visible)
			terrainUi.Refresh();
		if (unitUi.Visible)
			unitUi.Refresh();
	}

	// UI should be recreated on a new turn to reflect updated data
	public void ProcessTurn()
	{

	}
}
