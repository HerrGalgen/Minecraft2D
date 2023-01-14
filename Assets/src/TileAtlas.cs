using UnityEngine;

[CreateAssetMenu(fileName ="TileAtlas", menuName ="Tile Atlas")]
public class TileAtlas : ScriptableObject
{
    [Header("Environment")]
    public TileClass   dirt;
    public TileClass   grass;
    public TileClass   stone;
    public TileClass   tallGrass;
    public TileClass   log;
    public TileClass   leaf;
    public TileClass   snow;
    public TileClass   snad;

    [Header("Ores")]
    public TileClass   coal;
    public TileClass   iron;
    public TileClass   gold;
    public TileClass   diamond;
}
