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
	public Color iconColor;
	public string name;
	public bool playerCiv;

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

        // Process cities
        foreach (City c in cities)
        {
            c.ProcessTurn();
        }
        
        if (!playerCiv) // This is an AI civ
        {
            // AI logic...
        }

    }

}