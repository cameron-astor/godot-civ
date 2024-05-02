using Godot;
using System;

public partial class CityUI : Control
{

	[Signal]
	public delegate void QueueUnitEventHandler(Unit u);

	Label cityName;
	Label population;
	Label food;
	Label production;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		cityName = GetNode<Label>("Panel/CityName");
		population = GetNode<Label>("Panel/Population");
		food = GetNode<Label>("Panel/Food");
		production = GetNode<Label>("Panel/Production");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetCityUI(City city)
	{
		cityName.Text = city.name;
		population.Text = "Population: " + city.population;
		food.Text = "Food: " + city.totalFood;
		production.Text = "Production: " + city.totalProduction;

		ConnectUnitBuildSignals(city);
	}

	// Connects button press signals in city UI to the relevant city
	public void ConnectUnitBuildSignals(City city)
	{
		// For now, we will connect these manually in a non-generic way.
		// With more unit types and complexity, should be made generic.

		// Get the buttons
		VBoxContainer buttons = GetNode<VBoxContainer>("Panel/UnitBuildButtons/VBoxContainer");

		// Assign units to buttons
		UnitBuildButton settlerButton = buttons.GetNode<UnitBuildButton>("SettlerButton");
		settlerButton.u = new Settler();

		UnitBuildButton warriorButton = buttons.GetNode<UnitBuildButton>("WarriorButton");
		warriorButton.u = new Warrior();

	}

	public void SetVisibile(bool visible)
	{
		Visible = visible;
	}

}
