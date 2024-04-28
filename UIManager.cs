using Godot;
using System;

/// <summary>
/// The UI Manager is the top level node for the user interface.
/// It handles tasks that affect the entire UI, such as hiding and revealing windows.
/// It takes in all UI related signals from gameplay objects, abstracting over the particular
/// implementation and components of the UI.
/// </summary>
public partial class UIManager : Node
{
	// City UI TODO
	TerrainTileUI terrainUi;
	CityUI cityUI;
	UnitUI unitUi;
	Panel generalUi;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Get UI panels
		terrainUi = (TerrainTileUI) GetNode<Panel>("TerrainTileUi");
		cityUI = (CityUI) GetNode<Control>("CityUI");
		unitUi = (UnitUI) GetNode<Panel>("UnitUi");
		generalUi = GetNode<Panel>("GeneralUi");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Hides all UI windows that are not permanently fixed to the screen.
	// Used when mouse clicks out of bounds, for instance.
	public void HideAllPopups()
	{
		terrainUi.Visible = false;
		unitUi.Visible = false;
		cityUI.Visible = false;
	}

	public void SetTerrainUI(TerrainType ttype, int food, int prod)
	{
		HideAllPopups(); // clear screen of current popups
		terrainUi.SetTerrainUI(ttype, food, prod);
		terrainUi.Visible = true;
	}

	public void SetCityUI(City c)
	{
		HideAllPopups();
		cityUI.SetCityUI(c);
		cityUI.Visible = true;
	}

	public void SetUnitUI(Unit u)
	{
		HideAllPopups();
		unitUi.UpdateUnitUI(u);
		unitUi.Visible = true;
	}
}
