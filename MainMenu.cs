using Godot;

public partial class MainMenu : Node2D
{
	public void Start()
	{
		GD.Print("Starting game...");

		Game g = (Game) ResourceLoader.Load<PackedScene>("Game.tscn").Instantiate();
		HexTileMap map = g.GetNode<HexTileMap>("HexTileMap");
	
		// Set game attributes
		map.width = (int) this.GetNode<SpinBox>("VBoxContainer/HBoxContainer3/SpinBox").Value;
		map.height = (int) this.GetNode<SpinBox>("VBoxContainer/HBoxContainer3/SpinBox2").Value;
		map.PLAYER_COLOR = this.GetNode<ColorPickerButton>("VBoxContainer/HBoxContainer/ColorPickerButton").Color;
		map.NUM_AI_CIVS = (int) this.GetNode<SpinBox>("VBoxContainer/HBoxContainer2/SpinBox").Value;

		// Delete current scene and set Game scene as root
		GetNode("/root/MainMenu").QueueFree();
		GetTree().Root.AddChild(g);
	}

	public void Quit()
	{
		GetTree().Quit();
	}
}
