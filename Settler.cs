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
                attackVal = 0;

                movePoints = 2;
                maxMovePoints = 2;

                productionRequired = 100; // test
        }


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
                base._Ready(); // Unit shared setup code
                impassible = Unit.GroundUnitsDefaultImpassible();
	}

        public void FoundCity()
        {
                if ( map.GetHex(this.coords).ownerCity is null ) // Make sure the tile is not currently owned
                {
                        bool valid = true;
                        foreach ( Hex h in map.GetSurroundingHexes(this.coords) ) // Make sure surrounding tiles are not currently owned
                        {
                                valid = h.ownerCity is null;

                                foreach (Civilization civ in map.civs) // Ensure no other civ has this tile in their border tile pool already
                                {
                                        foreach (City city in civ.cities)
                                        {
                                                if (city.borderTilePool.Contains(h))
                                                        valid = false;
                                        }
                                }
                        }

                        if ( valid )
                        {
                                map.CreateCity(this.civ, this.coords, $"Settled City {coords.X}");
                                this.DestroyUnit();
                        }
                }
        }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
                base._Process(delta);
	}

}