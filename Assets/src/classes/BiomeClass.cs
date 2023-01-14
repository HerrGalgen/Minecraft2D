using UnityEngine;

[System.Serializable]
public class BiomeClass
{
    public string biomeName;
    public Color biomeColor;
    public TileAtlas tileAtlas;

    [Header("Noise Settings")]
    public float    caveFreq            = 0.08f;
    public float    terrainFreq         = 0.04f;
    public Texture2D caveNoiseTexture;
    
    [Header("Noise Settings")]
    public bool     generateCaves       = true;
    public float    heightMultiplier    = 25f;
    public float    surfaceValue        = 0.25f;
    public int      dirtLayerHeight     = 5;

        //Tree:
    [Header("Tree")]
    public int      treeChance          = 10;
    public int      minTreeHeight       = 4;
    public int      maxTreeHeight       = 6;

    [Header("Grass")]
    public int      grassChance         = 3;

    [Header("Ore Settings")]
    public OreClass[] ores;



}
