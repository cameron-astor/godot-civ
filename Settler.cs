using Godot;
using System;
using System.Collections.Generic;

public partial class Settler : Unit
{

        public Settler()
        {
                unitType = UnitType.SETTLER;
                unitName = "Settler";

                maxHp = 1;
                hp = 1;
                movePoints = 2;
                maxMovePoints = 2;
                productionRequired = 100; // test
        }


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
                base._Ready(); // Unit shared setup code

                impassible = Unit.GroundUnitsDefaultImpassible();
                // GD.Print("Settler entered scene tree!");
                // GD.Print(ui_images[unitType]);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
                base._Process(delta);
	}

}