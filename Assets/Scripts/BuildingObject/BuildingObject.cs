using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingObject : MonoBehaviour
{
    public AudioClip sound;
    public Sprite texture;

    public int durability;

    public bool isUnbreakable;
    public bool isPlaceableOn;
    public bool isInteractable;

    public bool isMultiBlock;
    public List<Sprite> multiTextures;

    public List<Item> dropItems;

    public virtual void BreakObject()
    {

    }

    public virtual void DropItem()
    {

    }

    public virtual void Interact()
    {

    }

    public virtual void SpawnItems()
    {

    }
}
