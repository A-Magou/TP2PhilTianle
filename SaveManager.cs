using Godot;
using System;
using System.IO;

using Dictionary = Godot.Collections.Dictionary;
using Array = Godot.Collections.Array;
using FileAccess = Godot.FileAccess;

public partial class SaveManager : Node
{
	public override void _Ready()
	{ 
		//GD.Print("SaveManager is ready");
	}
	
	public void SaveGame(string path)
	{
		/*
		GD.Print("SAVING GAME");
		GD.Print("Try to see if SceneTree is null");
		*/
		/*
		if (GetTree() == null)
		{
			GD.Print("SceneTree is null");
		}
		else
		{
			GD.Print("SceneTree is not null");
		}
		*/
		// Get all nodes that need to be saved
		var saveNodes = GetTree().GetNodesInGroup("Persist");
		
		// For each node, call function Save(), convert nodeDate to string and save it into a json file
		foreach (Node saveNode in saveNodes)
		{
			// Check the node is an instanced scene so it can be instanced again during load.
			if (string.IsNullOrEmpty(saveNode.SceneFilePath))
			{
				GD.Print($"persistent node '{saveNode.Name}' is not an instanced scene, skipped");
				continue;
			}
			// Check the node has a save function.
			if (!saveNode.HasMethod("Save"))
			{
				GD.Print($"persistent node '{saveNode.Name}' is missing a Save() function, skipped");
				continue;
			}

			// Call the node's save function.
			var nodeData = saveNode.Call("Save");
			// Make nodeData a string
			string jsonString = Json.Stringify(nodeData);
			// Save this string to file
			SaveTextToFile(path, "SaveGame1.json", jsonString);
		}
	}

	public void LoadGame(string path)
	{
		string fileName = "SaveGame1.json";
		string loadPath = path + fileName;

		/*
		var tree = GetTree();
		if (tree == null)
		{
			GD.Print("Error: SceneTree is null.");
		}
		else
		{
			GD.Print("SceneTree is initialized.");
		}
		*/
		if (!FileAccess.FileExists(loadPath))
		{
			GD.Print("LoadPath not found, maybe first time loading the game");
			return; // Error! We don't have a save to load.
		}
		
		var saveNodes = GetTree().GetNodesInGroup("Persist");
		foreach (Node saveNode in saveNodes)
		{
			saveNode.QueueFree();
		}
		
		using var saveFile = FileAccess.Open(loadPath, FileAccess.ModeFlags.Read);

		while (saveFile.GetPosition() < saveFile.GetLength())
		{
			var jsonString = saveFile.GetLine();

			// Creates the helper class to interact with JSON.
			var json = new Json();
			var parseResult = json.Parse(jsonString);
			if (parseResult != Error.Ok)
			{
				GD.Print($"JSON Parse Error: {json.GetErrorMessage()} in {jsonString} at line {json.GetErrorLine()}");
				continue;
			}
			// Get the data from the JSON object.
			var nodeData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);
			
			// Firstly, we need to create the object and add it to the tree and set its position.
			var newObjectScene = GD.Load<PackedScene>(nodeData["Filename"].ToString());
			var newObject = newObjectScene.Instantiate<Node>();
			GetNode(nodeData["Parent"].ToString()).AddChild(newObject);
			newObject.Set(Node2D.PropertyName.Position, new Vector2((float)nodeData["PosX"], (float)nodeData["PosY"]));
			
			// Now we set the remaining variables.
			foreach (var (key, value) in nodeData)
			{
				if (key == "Filename" || key == "Parent" || key == "PosX" || key == "PosY")
				{
					continue;
				}
				newObject.Set(key, value);
			}
		}
	}
	
	private void SaveTextToFile(string path, string fileName, string data)
	{
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		
		path = Path.Join(path, fileName);
		
		GD.Print(path);

		try
		{
			File.WriteAllText(path, data);
		}
		catch (System.Exception e)
		{
			GD.Print(e);
		}
	}
	
}

