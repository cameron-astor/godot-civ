using Godot;
using System;
using System.Collections.Generic;

public partial class Settler : Unit
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        base._Ready(); // Unit shared setup code

        unitType = UnitType.SETTLER;
        unitName = "Settler";

        maxHp = 1;
        hp = 1;
        movePoints = 2;
        maxMovePoints = 2;

        GD.Print("Settler instantiated!");
        GD.Print(ui_images[unitType]);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        
	}

}