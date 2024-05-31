using System.Linq;

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
                if ( map.GetHex(this.coords).ownerCity is null && !City.invalidTiles.ContainsKey(map.GetHex(this.coords))) // Make sure the tile is not currently owned
                {
                        bool valid = true;
                        foreach ( Hex h in map.GetSurroundingHexes(this.coords) ) // Make sure surrounding tiles are not currently owned
                        {
                                valid = h.ownerCity is null && !City.invalidTiles.ContainsKey(h);
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