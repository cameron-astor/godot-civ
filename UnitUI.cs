using Godot;
using System;
using System.Security.AccessControl;

public partial class UnitUI : Panel
{

	TextureRect unitImage;
	Label unitType;
	Label moves;
	Label hp;

	// Unit data
	Unit u;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		unitImage = GetNode<TextureRect>("TextureRect");
		unitType = GetNode<Label>("UnitTypeLabel");
		hp = GetNode<Label>("HealthLabel");
		moves = GetNode<Label>("MovesLabel");
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetUnit(Unit u)
	{
		this.u = u;

		// Special setup for settler
		if (this.u.unitType == UnitType.SETTLER)
		{
			VBoxContainer actionsContainer = GetNode<VBoxContainer>("ActionsContainer");
			Button foundCityButton = new Button();
			foundCityButton.Text = "Found City";
			actionsContainer.AddChild(foundCityButton);

			Settler settler = (Settler) this.u;
			foundCityButton.Pressed += settler.FoundCity;
			// settler.SettlerDestroyed += DisposeSignals;
			// NOTE THIS SIGNAL MUST BE DISCONNECTED ON SETTLER DESPAWN SINCE RIGHT NOW IT DOESNT AND CAUSES BUGS!!!
		}

		Refresh();
	}

	// public void DisposeSignals()
	// {
	// 	if (this.u.unitType == UnitType.SETTLER)
	// 	{
	// 		Settler s = (Settler) this.u;
	// 		Button foundCityButton = GetNode<Button>("ActionsContainer/Button");
	// 		foundCityButton.Pressed -= s.FoundCity;
	// 		s.SettlerDestroyed -= DisposeSignals;
	// 	}
	// }

	public void Refresh()
	{
		unitImage.Texture = Unit.ui_images[u.unitType];
		unitType.Text = "Unit Type: " + u.unitName;
		moves.Text = "Moves: " + u.movePoints + "/" + u.maxMovePoints;
		hp.Text = "HP: " + u.hp + "/" + u.maxHp;
	}

}
