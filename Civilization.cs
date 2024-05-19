using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Represents a single civilization present
/// in the game. Civilizations have
/// cities, and units.
/// The territory of a civilization is identical to 
/// the combined territories of all its cities.
/// </summary>
public class Civilization 
{

	public int id;
	public List<City> cities;
	public List<Unit> units;
	public Color territoryColor;
	public int territoryColorAltTileId;
	public string name;
	public bool playerCiv;

	// AI rng
	Random r = new Random();

	public Civilization()
	{
		cities = new List<City>();
		units = new List<Unit>();
	}

	public void SetRandomColor()
	{
		Random r = new Random();
		territoryColor = new Color(r.Next(255)/255.0f, r.Next(255)/255.0f, r.Next(255)/255.0f);
	}

    // Processes a civ's turn.
    // This includes things like applying growth from food to cities.
    // If this is an AI civ, it includes AI logic.
    public void ProcessTurn()
    {

        // Process cities growth and production queue
        foreach (City c in cities)
        {
            c.ProcessTurn();
        }
        
        if (!playerCiv) // This is an AI civ
        {
            // AI logic...

			// For each city, queue units
			foreach (City c in cities)
			{
				int rand = r.Next(30);
				// Chance to queue warrior: 1/15
				if (rand > 27)
				{
					c.AddUnitToBuildQueue(new Warrior());
				}

				// Chance to queue settler: 1/30
				if (rand > 28)
				{
					c.AddUnitToBuildQueue(new Settler());
				}
			}

			// For each unit, randomly move around and do an action
			List<Settler> citiesToFound = new List<Settler>();
			foreach (Unit u in units)
			{
				Random r = new Random();

				// Randomly move
				u.RandomMove();

				// Chance to attempt settle: 1/10
				if (u is Settler && r.Next(10) > 8)
				{
					Settler s = (Settler) u;
					citiesToFound.Add(s);
				}
			}

			foreach (Settler s in citiesToFound)
			{
				s.FoundCity();
			}

        }

    }

}