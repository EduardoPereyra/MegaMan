using UnityEngine;

public class AssetPalette: MonoBehaviour
{
    public GameObject[] enemyPrefabs = new GameObject[4];

    public enum EnemyList
    {
        BigEye,
        KillerBomb,
        Mambu,
        Pepe
    }
    public EnemyList enemyList;


    public GameObject[] itemsPrefabs = new GameObject[9];
    public enum ItemList
    {
        BonusBall,
        ExtraLife,
        LifeEnergyBig,
        LifeEnergySmall,
        WeaponEnergyBig,
        WeaponEnergySmall,
        MagnetBeam,
        WeaponPart,
        Yashichi
    }

    public ItemList itemList;
}
