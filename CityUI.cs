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

	// City data
	City city;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		cityName = GetNode<Label>("Panel/CityName");
		population = GetNode<Label>("Panel/Population");
		food = GetNode<Label>("Panel/Food");
		production = GetNode<Label>("Panel/Production");
	}

	public void SetCityUI(City city)
	{
		this.city = city;

		cityName.Text = this.city.name;
		population.Text = "Population: " + this.city.population;
		food.Text = "Food: " + this.city.totalFood;
		production.Text = "Production: " + this.city.totalProduction;

		PopulateUnitQueueUI(this.city);

		ConnectUnitBuildSignals(this.city); // This is why we should destroy and remake the UIs every time!!
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

		// Attach signals to city unit queue
		// Would be something to generalize with more unit types
		settlerButton.OnPressed += city.AddUnitToBuildQueue;
		settlerButton.OnPressed += this.Refresh;
		warriorButton.OnPressed += city.AddUnitToBuildQueue;
		warriorButton.OnPressed += this.Refresh;

	}

	public void PopulateUnitQueueUI(City city)
	{

		VBoxContainer queue = GetNode<VBoxContainer>("Panel/QueueContainer/VBoxContainer");

		// Clear existing queue ui
		foreach (Node n in queue.GetChildren())
		{
			queue.RemoveChild(n);
			n.QueueFree();
		}

		for (int i = 0; i < city.unitBuildQueue.Count; i++)
		{
			Unit u = city.unitBuildQueue[i];

			if (i == 0) // Unit is first in queue, currently being built
			{
				queue.AddChild(new Label() {
					Text = $"{u.unitName} {city.unitBuildTracker}/{u.productionRequired}"
				});				
			} else {
				queue.AddChild(new Label() {
					Text = $"{u.unitName} 0/{u.productionRequired}"
				});	
			}
		}
		
	}

	public void SetVisibile(bool visible)
	{
		Visible = visible;
	}

	public void Refresh()
	{
		cityName.Text = this.city.name;
		population.Text = "Population: " + this.city.population;
		food.Text = "Food: " + this.city.totalFood;
		production.Text = "Production: " + this.city.totalProduction;

		PopulateUnitQueueUI(this.city);
	}

	// For signal compatibility
	public void Refresh(Unit u)
	{
		Refresh();
	}

}
