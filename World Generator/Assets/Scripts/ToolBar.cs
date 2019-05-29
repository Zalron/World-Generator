using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace WorldGenerator
{
    public class ToolBar : MonoBehaviour
    {
        World world;
        public Player player;
        public RectTransform highlight;
        public ItemSlot[] itemSlots;

        int slotIndex;

        public void Start()
        {
            world = GameObject.Find("World").GetComponent<World>();
            foreach (ItemSlot slot in itemSlots)
            {
                slot.icon.sprite = world.blockType[slot.itemID].icon;
                slot.icon.gameObject.SetActive(true);
            }
            player.selectedBlockIndex = itemSlots[slotIndex].itemID;
        }
        public void Update()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                if (scroll > 0)
                {
                    slotIndex--;
                }
                else
                {
                    slotIndex++;
                }
                if (slotIndex > itemSlots.Length - 1)
                {
                    slotIndex = 0;
                }
                if (slotIndex < 0)
                {
                    slotIndex = itemSlots.Length - 1;
                }
                highlight.position = itemSlots[slotIndex].icon.transform.position;
                player.selectedBlockIndex = itemSlots[slotIndex].itemID;
            }
        }
        
    }
    [System.Serializable]
    public class ItemSlot
    {
        public byte itemID;
        public Image icon;
    }
}
