using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [SerializeField]
    private List<Item> Items = new List<Item>();

    [SerializeField]
    private Transform ItemContent;
    [SerializeField]
    private GameObject InventoryItem;

    public void Awake()
    {
        Instance = this;
    }

    public void Add(Item item)
    {
        Items.Add(item);
        RefreshInventoryUI();
    }

    public void Remove(Item item)
    {
        int index = Items.IndexOf(item);
        if (index != -1)
        {
            Items.RemoveAt(index);
            Destroy(ItemContent.GetChild(index).gameObject); // Destroy the UI element

            RefreshInventoryUI();
        }
    }

    public void RefreshInventoryUI()
    {

        if (ItemContent == null || InventoryItem == null)
        {
            Debug.LogError("UI components not initialized");
            return; // Exit the method if UI components are not initialized
        }

        // Clear existing UI items
        foreach (Transform child in ItemContent)
        {
            Destroy(child.gameObject);
        }

        // Re-create UI items from the Items list
        foreach (var item in Items)
        {
            GameObject obj = Instantiate(InventoryItem, ItemContent);
            var itemName = obj.transform.Find("ItemName").GetComponent<Text>();
            var itemIcon = obj.transform.Find("ItemIcon").GetComponent<Image>();
            var removeButton = obj.transform.Find("RemoveButton").GetComponent<Button>();

            itemName.text = item.GetName;
            itemIcon.sprite = item.GetIcon;

            // Setup the button to remove the item
            removeButton.onClick.AddListener(() => Remove(item));
        }
    }

    public bool Contains(Item item)
    {
        return Items.Contains(item);
    }
}
