using Godot;
using System;

public partial class CityUI : Control
{

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
	}

	public void SetVisibile(bool visible)
	{
		Visible = visible;
	}
}
