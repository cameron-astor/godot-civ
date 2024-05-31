using Godot;
using System;
using System.Collections.Generic;

public partial class HighlightLayer : TileMapLayer
{

	int w;
	int h;

	List<Hex> currentlyHighlighted;

	City current;

	public void SetupHighlightLayer(int w, int h)
	{
		this.w = w;
		this.h = h;
		currentlyHighlighted = new List<Hex>();
		
		for (int x = 0; x < w; x ++)
		{
			for (int y = 0; y < h; y++)
			{
				SetCell(new Vector2I(x, y), 0, new Vector2I(0, 3));
			}
		}
		Visible = false;
	}

	public void SetHighlightLayerForCity(City c)
	{

		ResetHighlightLayer();
		current = c;

		foreach (Hex h in c.GetTerritory())
		{
			currentlyHighlighted.Add(h);
			SetCell(h.coordinate, -1);
		}

		foreach (Hex h in c.GetBorderTilePool())
		{
			currentlyHighlighted.Add(h);
			SetCell(h.coordinate, -1);
		}
		Visible = true;
	}

	public void ResetHighlightLayer()
	{
		foreach (Hex h in currentlyHighlighted)
		{
			SetCell(h.coordinate, 0, new Vector2I(0, 3));
		}	
		current = null;
		Visible = false;
	}

	public void RefreshLayer()
	{
		if (current != null)
		{
			foreach (Hex h in current.GetTerritory())
			{
				currentlyHighlighted.Add(h);
				SetCell(h.coordinate, -1);
			}

			foreach (Hex h in current.GetBorderTilePool())
			{
				currentlyHighlighted.Add(h);
				SetCell(h.coordinate, -1);
			}
		Visible = true;			
		}
		else {
			Visible = false;
		}
	}

}
