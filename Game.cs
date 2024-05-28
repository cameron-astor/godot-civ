using Godot;
using System;

public partial class Game : Node
{

    public override void _EnterTree()
    {
		// Load unit textures and scenes
		Unit.LoadTextures();
		Unit.LoadUnitScenes();

		// Load UI textures
		TerrainTileUI.LoadTerrainImages();
    }

}
