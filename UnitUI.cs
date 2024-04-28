using Godot;
using System;

public partial class UnitUI : Panel
{

	TextureRect unitImage;
	Label unitType;
	Label moves;
	Label hp;

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

	public void UpdateUnitUI(Unit u)
	{
		unitImage.Texture = Unit.ui_images[u.unitType];
		unitType.Text = "Unit Type: " + u.unitName;
		moves.Text = "Moves: " + u.maxMovePoints + "/" + u.movePoints;
		hp.Text = "HP: " + u.maxHp + "/" + u.hp;
	}
}
