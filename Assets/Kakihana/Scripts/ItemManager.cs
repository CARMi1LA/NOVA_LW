using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public enum ItemType
    {
        Score = 0,
        Hp,
        Energy
    }
    public ItemType itemType;

    public int itemScore;
    public int itemHp;
    public int itemEnergy;
    // Start is called before the first frame update
    void Start()
    {
        
    }
}
