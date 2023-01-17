using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class TerrainGeneration : MonoBehaviour
{

    /*****************************************************************/

    public PlayerController player;
    public new CamController camera;
    
    //Terrain:
    [Header("Tile Atlas")]
    public TileAtlas tileAtlas;
    public float     seed;

    public BiomeClass[] biomes;


    [Header("Biomes")]
    public float biomeFreq;
    public Gradient biomeGradient;
    public Texture2D biomeMap;

    [Header("Noise Settings")]
    public int      chunkSize           = 20;
    public bool     generateCaves       = true;
    public int      worldSize           = 100;
    public int      heightAddition      = 25;


    [Header("Noise Settings")]
    public Texture2D caveNoiseTexture;

    [Header("Ore Settings")]
    public OreClass[] ores;

    //Stuff
    private GameObject[] _worldChunks;
    private List<Vector2> _worldTiles = new List<Vector2>();
    private List<GameObject> _worldTileObjects = new List<GameObject>();
    private List<TileClass> _worldTileClasses = new List<TileClass>();
    
    private BiomeClass _curBiome;

    /*****************************************************************/

    private void OnValidate()
    {
        DrawCavesAndOres();
        DrawTextures();
    }

    /*****************************************************************/

    private void Start()
    {
        for (var i = 0; i < ores.Length; i++)
        {
            ores[i].spreadTexture = new Texture2D(worldSize, worldSize);
        }
        seed = Random.Range(-10000, 10000);
        
        //Draw Texture:
        DrawTextures();
        DrawCavesAndOres();
        CreateChunks();
        GenerateTerrain();
        
        camera.Spawn(new Vector3(player.spawnPos.x, player.spawnPos.y, camera.transform.position.z), worldSize);
        player.Spawn(worldSize);

        RefreshChuncks();
    }
    
    /*****************************************************************/

    private void Update()
    {
        RefreshChuncks();
    }

    /*****************************************************************/
    private void DrawCavesAndOres()
    {
        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        for (var x = 0; x < worldSize; x++)     // Each biome has different cave sizes
            for (var y = 0; y < worldSize; y++)
            {
                _curBiome = GetCurrentBiome(x,y);
                var v = Mathf.PerlinNoise((x +seed) * _curBiome.caveFreq, (y+seed) * _curBiome.caveFreq);
                if (v > _curBiome.surfaceValue)
                    caveNoiseTexture.SetPixel(x, y, Color.white);
                else 
                    caveNoiseTexture.SetPixel(x, y, Color.black);

            }
        
        caveNoiseTexture.Apply();
        for (var i = 0; i < _curBiome.ores.Length; i++)
            for (var x = 0; x < worldSize; x++)    
                for (var y = 0; y < worldSize; y++)
                {
                    _curBiome = GetCurrentBiome(x,y);
            
                     // each Or in each biome has different values
                    
                    ores[i].spreadTexture.SetPixel(x, y, Color.black);
                    if (_curBiome.ores.Length >= i+1)
                    {
                        var v = Mathf.PerlinNoise((x +seed) * _curBiome.ores[i].rarity, (y+seed) * _curBiome.ores[i].rarity);
                        if (v > _curBiome.ores[i].size)
                            ores[i].spreadTexture.SetPixel(x, y, Color.white);
        
                        ores[i].spreadTexture.Apply();
                    }
                }
    }

    /*****************************************************************/
    
    private void DrawTextures()
    {
        biomeMap = new Texture2D(worldSize, worldSize);
        DrawBiomeTexture();

        for (var i = 0; i < biomes.Length; i++)
        {
            biomes[i].caveNoiseTexture = new Texture2D(worldSize, worldSize);

            for (var o = 0; o < biomes[i].ores.Length; o++) // ore noise texture
                biomes[i].ores[o].spreadTexture = new Texture2D(worldSize, worldSize);

            GenerateNoiseTexture(biomes[i].caveFreq, biomes[i].surfaceValue, biomes[i].caveNoiseTexture);

            for (var n = 0; n < biomes[i].ores.Length; n++) // ore noise texture
                GenerateNoiseTexture(biomes[i].ores[n].rarity, biomes[i].ores[n].size, biomes[i].ores[n].spreadTexture);
        }
    }

    /*****************************************************************/

    private void DrawBiomeTexture()
    {
        for (var x = 0; x < biomeMap.width; x++)
            for (var y = 0; y < biomeMap.height; y++)
            {
                var v = Mathf.PerlinNoise((x +seed) * biomeFreq, (y+seed) * biomeFreq);
                biomeMap.SetPixel(x, y, biomeGradient.Evaluate(v));
            }

        biomeMap.Apply();
    }

    /*****************************************************************/
    private void CreateChunks()
    {
        var numberOfChunks = worldSize / chunkSize;
        _worldChunks = new GameObject[numberOfChunks];
        for (var i = 0; i < numberOfChunks; i++)
        {
            var newChunk = new GameObject();
            newChunk.name = "chunk_" + i.ToString();
            newChunk.transform.parent = this.transform;
            _worldChunks[i] = newChunk;
        }
    }

    /*****************************************************************/

    private BiomeClass GetCurrentBiome(int x, int y)
    {
        //change current biome

        var v = Mathf.PerlinNoise((x +seed) * biomeFreq, (y+seed) * biomeFreq);

        //search through biomes
        for (int i = 0; i < biomes.Length; i++)
        {
            if (biomes[i].biomeColor == biomeMap.GetPixel(x,y))
            {
                return biomes[i];
            }
        }

        return _curBiome;
    }

    /*****************************************************************/

    private void GenerateTerrain()
    {
        
        TileClass tileClass;

        for (int x = 0; x < worldSize; x++)
        {
            float height;
            
            for (var y = 0; y < worldSize; y++)
            {
                _curBiome = GetCurrentBiome(x, y);
                
                height = Mathf.PerlinNoise((x + seed) * _curBiome.terrainFreq, seed * _curBiome.terrainFreq) * _curBiome.heightMultiplier + heightAddition;

                if (x == worldSize / 2)
                {
                    player.spawnPos = new Vector2(x, height + 1);
                }
                
                if (y >= height) break;

                if (y < height - _curBiome.dirtLayerHeight) {
                    
                    tileClass = _curBiome.tileAtlas.stone;

                    if (ores[0].spreadTexture.GetPixel(x,y).r > 0.5f && height - y > ores[0].maxSpawnHeight)
                        tileClass = tileAtlas.coal;

                    if (ores[1].spreadTexture.GetPixel(x,y).r > 0.5f && height - y > ores[1].maxSpawnHeight)
                        tileClass = tileAtlas.iron;

                    if (ores[2].spreadTexture.GetPixel(x,y).r > 0.5f && height - y > ores[2].maxSpawnHeight)
                        tileClass = tileAtlas.gold;

                    if (ores[3].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[3].maxSpawnHeight)
                        tileClass = tileAtlas.diamond;
                }
                else if (y < height - 1)
                    tileClass = _curBiome.tileAtlas.dirt;

                else //top layer
                {
                    tileClass =_curBiome.tileAtlas.grass;
                }

                if (generateCaves)
                {
                    if (caveNoiseTexture.GetPixel(x,y).r > 0.5f)
                        PlaceTile(tileClass, x, y, false);
                } else
                    PlaceTile(tileClass, x, y, false);     

                if (y >= height - 1)
                {

                    if (Random.Range(0, _curBiome.treeChance) == 1) //generate Tree or grass
                    {   
                        if (_worldTiles.Contains(new Vector2(x,y)))
                        {
                            GenerateTree(x, y+1, Random.Range(_curBiome.minTreeHeight, _curBiome.maxTreeHeight));
                        }
                    } 

                    if (Random.Range(0, _curBiome.grassChance) == 1) //place grass
                    {
                        if (_worldTiles.Contains(new Vector2(x,y)))
                        {
                            if (_curBiome.tileAtlas.tallGrass != null)
                                PlaceTile(_curBiome.tileAtlas.tallGrass, x, y+1, _curBiome.tileAtlas.tallGrass.inBackground);
                        }
                    }
                }

            }
        }

    }

    /*****************************************************************/

    private void GenerateNoiseTexture(float freq, float limit, Texture2D noiseTexture)
    {
        for (var x = 0; x < noiseTexture.width; x++)
            for (var y = 0; y < noiseTexture.height; y++)
            {
                var v = Mathf.PerlinNoise((x +seed) * freq, (y+seed) * freq);
                noiseTexture.SetPixel(x, y, (v > limit)? Color.white : Color.black);
            }

        noiseTexture.Apply();
    }

    /*****************************************************************/

    public void PlaceTile(TileClass tile, int x, int y, bool inBackground)
    {
        if (_worldTiles.Contains(new Vector2Int(x, y)) 
            && x >= 0 && x <= worldSize
            && y >= 0 && y <= worldSize)
            return;
        
        var newTile = new GameObject();

        var chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize);
        chunkCoord /= chunkSize;

        if (chunkCoord >= _worldChunks.Length) return;
        
        newTile.transform.parent = _worldChunks[(int)chunkCoord].transform;

        newTile.AddComponent<SpriteRenderer>();
        if (!inBackground)
        {
            newTile.AddComponent<BoxCollider2D>();
            newTile.GetComponent<BoxCollider2D>().size = Vector2.one;
            newTile.tag = "Ground";
        }
        
        var spriteIndex = Random.Range(0, tile.tileSprites.Length);
        newTile.GetComponent<SpriteRenderer>().sprite = tile.tileSprites[spriteIndex];
        
        if (tile.inBackground)
            newTile.GetComponent<SpriteRenderer>().sortingOrder = -10;
        else
            newTile.GetComponent<SpriteRenderer>().sortingOrder = -5;

        newTile.name = tile.tileSprites[0].name;
        newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);
        _worldTiles.Add(newTile.transform.position - (Vector3.one * 0.5f));
        _worldTileObjects.Add(newTile);
        _worldTileClasses.Add(tile);
    }
    /*****************************************************************/

    private void GenerateTree(int x, int y, int treeHeight)
    {   
        //generate logs
        for (var i = 0; i < treeHeight; i++) {
            PlaceTile(tileAtlas.log, x, y+i, tileAtlas.log.inBackground);
        }

        //generate leaves
        //main part
        PlaceTile(tileAtlas.leaf, x, y+treeHeight, tileAtlas.leaf.inBackground);
        PlaceTile(tileAtlas.leaf, x, y+treeHeight + 1, tileAtlas.leaf.inBackground);
        PlaceTile(tileAtlas.leaf, x, y+treeHeight + 2, tileAtlas.leaf.inBackground);
        PlaceTile(tileAtlas.leaf, x, y+treeHeight + 3, tileAtlas.leaf.inBackground);
        
        //right
        PlaceTile(tileAtlas.leaf, x+1, y+treeHeight-1, tileAtlas.leaf.inBackground);
        PlaceTile(tileAtlas.leaf, x+1, y+treeHeight, tileAtlas.leaf.inBackground);
        PlaceTile(tileAtlas.leaf, x+1, y+treeHeight+1, tileAtlas.leaf.inBackground);
        PlaceTile(tileAtlas.leaf, x+1, y+treeHeight+2, tileAtlas.leaf.inBackground);
        PlaceTile(tileAtlas.leaf, x+2, y+treeHeight, tileAtlas.leaf.inBackground);
        PlaceTile(tileAtlas.leaf, x+2, y+treeHeight+1, tileAtlas.leaf.inBackground);

        //left 
        PlaceTile(tileAtlas.leaf, x-1, y+treeHeight-1, tileAtlas.leaf.inBackground);
        PlaceTile(tileAtlas.leaf, x-1, y+treeHeight, tileAtlas.leaf.inBackground);
        PlaceTile(tileAtlas.leaf, x-1, y+treeHeight+1, tileAtlas.leaf.inBackground);
        PlaceTile(tileAtlas.leaf, x-1, y+treeHeight+2, tileAtlas.leaf.inBackground);
        PlaceTile(tileAtlas.leaf, x-2, y+treeHeight, tileAtlas.leaf.inBackground);
        PlaceTile(tileAtlas.leaf, x-2, y+treeHeight+1, tileAtlas.leaf.inBackground);

    }
    /*****************************************************************/

    public void RemoveTile(int x, int y)
    {
        if (!_worldTiles.Contains(new Vector2Int(x, y))
            && x >= 0 && x <= worldSize
            && y >= 0 && y <= worldSize) 
            return;
        
        Destroy(_worldTileObjects[_worldTiles.IndexOf(new Vector2(x, y))]); //remove from world
        _worldTileObjects.RemoveAt(_worldTiles.IndexOf(new Vector2(x, y))); //remove from all tiles
        _worldTileClasses.RemoveAt(_worldTiles.IndexOf(new Vector2(x, y))); //remove from all Tiles in List
        _worldTiles.RemoveAt(_worldTiles.IndexOf(new Vector2(x, y))); //remove from grid List

    }
    
    /*****************************************************************/

    private void RefreshChuncks()
    {
        for (var i = 0; i < _worldChunks.Length; i++)
        {
            if (Vector2.Distance(new Vector2((i * chunkSize) + (chunkSize / 2), 0), new Vector2(player.transform.position.x, 0)) > Camera.main.orthographicSize * 4f)
                 _worldChunks[i].SetActive(false);
            else
                _worldChunks[i].SetActive(true);
            
        }
    }
    
    /*****************************************************************/
}
