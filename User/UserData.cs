using Godot;
using System.Collections.Generic;
using static Resources;

public partial class UserData : Node
{
    private UserData(){GenerateUserData();}
    private static UserData _instance;
    public static UserData GetInstance()
    {
        if (_instance == null)
        {
            _instance = new UserData();
        }
        return _instance;
    }

    public Dictionary<MaterialType?, int> userIngredients = new Dictionary<MaterialType?, int>();
    public void SaveGame()
    {

    }

    public void LoadGame()
    {

    }

    public void GenerateUserData()
    {
        foreach(MaterialType? materialType in FoodMaterialType)
        {
            userIngredients.Add(materialType, 1);
        }
    }
}
