using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string itemName;
    public int quantity;
    // Có thể thêm các thuộc tính khác như: icon, mô tả, loại vật phẩm...
    public ItemData(string name, int qty)
    {
        itemName = name;
        quantity = qty;
    }
}
