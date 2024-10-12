using Godot;

[GlobalClass]
public partial class CustomMainLoop : SceneTree
{
    //private double _timeElapsed = 0;
    
    private static CustomMainLoop instance;
    private LevelManager level;
    private SaveManager save;
    
    public string path = ProjectSettings.GlobalizePath("user://");
    
    public string levelPath_Level1 = "res://Scenes/level1.tscn";
    public string levelPath_Credits = "res://Scenes/Credits.tscn";
	
    public CustomMainLoop()
    {
        instance = this;
        level = new LevelManager();
        save = new SaveManager();
    }

    public static CustomMainLoop Get()
    {
        return instance;
    }

    public LevelManager GetLevelManager()
    {
        if (level.GetParent() == null)
        {
            base.Root.AddChild(level);
        }
        return level;
    }

    public SaveManager GetSaveManager()
    {
        // Need to make sure that the SaveManager that we use is in the tree
        // Otherwise we cant use functions like getTree()
        if (save.GetParent() == null)
        {
            base.Root.AddChild(save);
        }
        
        return save;
    }

    public override void _Initialize()
    {
        //base._Initialize();
        GD.Print("Initialized:");

        // Need to wait until the CustomMainLoop is fully initialized
        // And then we load the game
        CallDeferred(nameof(DeferredLoadGame));
    }

    private void DeferredLoadGame()
    {
        // Now the CMainLoop is initialized
        //GD.Print("Initialization finished, now loading the game");
        GetSaveManager().LoadGame(path);
    }
    
    public override bool _Process(double delta)
    {
        base._Process(delta);

        // Return true to end the main loop.
        
        // The game won't call _Finalize() if I just close it,
        // so I need to do it manually
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            GD.Print("UICancel triggered, start saving game and then end game");
            _Finalize();
            Quit();
        }

        // When you press "Enter", game will load Credits level
        if (Input.IsActionJustPressed("ui_accept"))
        {
            GetLevelManager().LoadLevel(levelPath_Credits);
        }
        
        return false;
        //return Input.GetMouseButtonMask() != 0 || Input.IsKeyPressed(Key.Escape);
    }

    public override void _Finalize()
    {
        //GD.Print("Finalized:");
        // Save game by SaveManager
        GetSaveManager().SaveGame(path);
    }
}