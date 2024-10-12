using Godot;
using System;
using System.Collections.Generic;

public partial class LevelManager : Node
{
    
    public static LevelManager instance;

    public Node simultaneousScene;
    public override void _Ready()
    {
        instance = this;
        //GD.Print("LevelManager ready");
    }

    public void LoadLevel(string levelPath)
    {
        GetTree().ChangeSceneToFile(levelPath);
    }

}
