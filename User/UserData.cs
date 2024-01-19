using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using static Resources;

[GlobalClass]
public partial class UserData : Node
{
    const string userIngredientsPath = "res://User/Ingredients.save";
    const string userMaterialsPath = "res://User/Materials.save";
    const string userDishesPath = "res://User/Dishes.save";
    const string userCreaturesPath = "res://User/Creatures.save";
    const string userCraftsPath = "res://User/Crafts.save";
    const string userInventoryPath = "res://User/Inventory.save";
    const string userEquipmentPath = "res://User/Equipment.save";

    public Dictionary<MaterialType?, int> userIngredients = new Dictionary<MaterialType?, int>();
    public Dictionary<MaterialType?, int> userMaterials = new Dictionary<MaterialType?, int>();
    public Dictionary<string, bool> userDishes = new Dictionary<string, bool>();
    public Dictionary<string, bool> userCreatures = new Dictionary<string, bool>();
    public Dictionary<string, bool> userCrafts = new Dictionary<string, bool>();
    public List<DishType> userInventory = new List<DishType>();
    public List<DishType> userEquipment = new List<DishType>();

    private UserData()
    {
        GenerateUserData();
    }
    private static UserData _instance;
    public static UserData GetInstance()
    {
        if (_instance == null)
        {
            _instance = new UserData();
        }
        return _instance;
    }

    public void SaveGame()
    {
        
    }

    void SaveMaterials()
    {
        Dictionary<string, int> dictionary = new Dictionary<string, int>();
        foreach(var i in userMaterials.Keys)
        {
            dictionary.Add(i.ToString(), userMaterials[i]);
        }
    }

    public void LoadGame()
    {
        
    }

    public void GenerateUserData()
    {
        foreach(MaterialType? materialType in FoodMaterialType)
        {
            userIngredients.Add(materialType, 9);
        }
        foreach(MaterialType? materialType in CraftMaterialType)
        {
            userMaterials.Add(materialType, 9);
        }
        foreach(DishType dishType in Enum.GetValues(typeof(DishType)))
        {
            userDishes.Add(dishType.ToString(), false);
        }
        foreach(Entities entity in Enum.GetValues(typeof(Entities)))
        {
            userCreatures.Add(entity.ToString(), false);
        }
        foreach(CraftType craft in Enum.GetValues(typeof(CraftType)))
        {
            userCrafts.Add(craft.ToString(), false);
        }
        userInventory.Add(DishType.BerrySoup);
        userInventory.Add(DishType.ChickenBerrySauce);
        userInventory.Add(DishType.Salad);
        userInventory.Add(DishType.BerrySoup);
        userInventory.Add(DishType.Salad);
    }
}
