using System.Collections.Generic;
using Godot;
using static Resources;

public partial class SaveTemplate : Resource
{
    public Dictionary<MaterialType?, int> userIngredients = new Dictionary<MaterialType?, int>();
    public Dictionary<MaterialType?, int> userMaterials = new Dictionary<MaterialType?, int>();
    public Dictionary<string, bool> userDishes = new Dictionary<string, bool>();
    public Dictionary<string, bool> userCreatures = new Dictionary<string, bool>();
    public Dictionary<string, bool> userCrafts = new Dictionary<string, bool>();
    public List<DishType> userInventory = new List<DishType>();
    public List<string> userEquipment = new List<string>();

}