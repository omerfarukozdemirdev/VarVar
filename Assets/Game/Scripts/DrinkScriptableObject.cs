using UnityEngine;

[CreateAssetMenu(fileName = "Drink", menuName = "Soner/Drink", order = 3)]
public class DrinkScriptableObject : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    public int Price;
}
