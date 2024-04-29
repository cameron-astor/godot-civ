using Godot;
using System;

public partial class Warrior : Unit
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready(); // Unit shared setup code

        unitType = UnitType.WARRIOR;
        unitName = "Warrior";

        maxHp = 3;
        hp = 3;
        movePoints = 1;
        maxMovePoints = 1;

        GD.Print("Warrior instantiated!");
        GD.Print(ui_images[unitType]);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
		
	}
}
