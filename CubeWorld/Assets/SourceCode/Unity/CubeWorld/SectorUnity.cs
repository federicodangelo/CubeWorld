using UnityEngine;
using System.Collections.Generic;
using CubeWorld.Tiles;
using CubeWorld.Sectors;

public class SectorUnity : MonoBehaviour, ISectorGraphics
{
    public GameManagerUnity gameManagerUnity;
	
    private Sector sector;

    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private bool willRequireMeshUpdate;
    private bool willRequireLightUpdate;
    private bool visibleByPlayer;

    /*
     * Used for optimizations, if totalAmbientLuminance is 0, then there is no need to update the light when that method is called
     */
    private int totalAmbientLuminance = -1;

    void Awake()
    {
        mesh = new Mesh();
        mesh.bounds = new Bounds(new Vector3(SectorManager.SECTOR_SIZE / 2, SectorManager.SECTOR_SIZE / 2, SectorManager.SECTOR_SIZE / 2), new Vector3(SectorManager.SECTOR_SIZE + 1, SectorManager.SECTOR_SIZE + 1, SectorManager.SECTOR_SIZE + 1));

        meshFilter = (MeshFilter)gameObject.AddComponent(typeof(MeshFilter));
        meshRenderer = (MeshRenderer)gameObject.AddComponent(typeof(MeshRenderer));

        meshRenderer.castShadows = false;
        meshRenderer.receiveShadows = false;

        meshFilter.mesh = mesh;
    }
	
	public bool IsInUse()
	{
		return sector != null;
	}
	
	public Sector GetSector()
	{
		return sector;
	}
	
	public void SetSector(Sector sector)
	{
		this.sector = sector;
		this.willRequireMeshUpdate = true;
		this.willRequireLightUpdate = false;
        this.totalAmbientLuminance = -1;
		
		if (sector != null)
		{
			if (visibleByPlayer)
			{
	            gameManagerUnity.world.sectorManager.EnqueueInvalidatedSector(
	                sector.sectorPosition.x,
	                sector.sectorPosition.y,
	                sector.sectorPosition.z);
			}
		}
		else
		{
			mesh.Clear();
		}
	}

    public void OnBecameVisible()
    {
        visibleByPlayer = true;
		
		if (sector != null)
		{
	        if (willRequireMeshUpdate)
	        {
	            gameManagerUnity.world.sectorManager.EnqueueInvalidatedSector(
	                sector.sectorPosition.x,
	                sector.sectorPosition.y,
	                sector.sectorPosition.z);
	        }
	        else if (willRequireLightUpdate)
	        {
	            gameManagerUnity.world.sectorManager.EnqueueInvalidatedLight(
	                sector.sectorPosition.x,
	                sector.sectorPosition.y,
	                sector.sectorPosition.z);
	        }
		}
    }

    public void OnBecameInvisible()
    {
        visibleByPlayer = false;
    }

    static private List<Vector3> vertices = new List<Vector3>();
    static private List<Vector2> uvs = new List<Vector2>();
    static private List<Vector2> uvs2 = new List<Vector2>();
    static private List<Vector3> normals = new List<Vector3>();
    static private List<Color> colors = new List<Color>();

    static private List<int> trianglesNormal = new List<int>();
    static private List<int> trianglesTransparent = new List<int>();
    static private List<int> trianglesTranslucid = new List<int>();
    static private List<int> trianglesDamage = new List<int>();
	static private List<int> trianglesAnimated = new List<int>();

    public void UpdateMesh()
    {
        if (visibleByPlayer == false)
        {
            willRequireMeshUpdate = true;
            return;
        }

        mesh.Clear();

        CubeWorld.World.CubeWorld world = gameManagerUnity.world;
		TileManager tileManager = world.tileManager;
		
        colors.Clear();
        vertices.Clear();
        uvs.Clear();
        uvs2.Clear();
        normals.Clear();
        trianglesNormal.Clear();
        trianglesTransparent.Clear();
        trianglesTranslucid.Clear();
        trianglesDamage.Clear();
		trianglesAnimated.Clear();

        int index = 0;
        float uvdelta = 1.0f / GraphicsUnity.TILE_PER_MATERIAL_ROW;

        int tileOffsetX = sector.tileOffset.x;
        int tileOffsetY = sector.tileOffset.y;
        int tileOffsetZ = sector.tileOffset.z;

        float ambientLightIntensity = MeshUtils.luminanceMapper[world.dayCycleManager.ambientLightLuminance];

        totalAmbientLuminance = 0;

        for (int z = tileOffsetZ; z < SectorManager.SECTOR_SIZE + tileOffsetZ; z++)
        {
            for (int y = tileOffsetY; y < SectorManager.SECTOR_SIZE + tileOffsetY; y++)
            {
                for (int x = tileOffsetX; x < SectorManager.SECTOR_SIZE + tileOffsetX; x++)
                {
                    TilePosition pos = new TilePosition(x, y, z);

                    Tile tile = tileManager.GetTile(pos);

                    totalAmbientLuminance += tile.AmbientLuminance;

                    if (tile.tileType == TileDefinition.EMPTY_TILE_TYPE || tile.Dynamic)
                        continue;

                    Vector3 offset = GraphicsUnity.TilePositionToVector3(x - tileOffsetX, y - tileOffsetY, z - tileOffsetZ);

                    TileDefinition tileDefinition = tileManager.GetTileDefinition(tile.tileType);

                    List<int> triangles;
                    Vector3[] faceVectors = MeshUtils.faceVectorsNormal;

                    TileDefinition.DrawMode drawMode = tileDefinition.drawMode;

                    if (drawMode == TileDefinition.DrawMode.SOLID ||
                        drawMode == TileDefinition.DrawMode.LIQUID && tileDefinition.solid)
                    {
                        if (tile.Energy == tileDefinition.energy)
                            triangles = trianglesNormal;
                        else
                            triangles = trianglesDamage;
                    }
                    else if (drawMode == TileDefinition.DrawMode.SOLID_ALPHA)
                        triangles = trianglesTranslucid;
                    else
                        triangles = trianglesTransparent;

                    if (tileDefinition.animated)
                        triangles = trianglesAnimated;

                    bool drawingLiquidSurface = false;

                    if (drawMode == TileDefinition.DrawMode.LIQUID &&
                        tileManager.IsValidTile(pos + new TilePosition(0, 1, 0)) && 
                        tileManager.GetTileType(pos + new TilePosition(0, 1, 0)) != tile.tileType)
                    {
                        drawingLiquidSurface = true;
                    }

                    float[] liquidVertexHeights = null;

                    for (int face = 0; face < 6; face++)
                    {
                        int material = tileDefinition.materials[face];

                        if (material < 0)
                            continue;

                        TileDefinition.DrawMode nearTileDrawMode = TileDefinition.DrawMode.NONE;
                        TilePosition normalInt = MeshUtils.faceNormalsTile[face];
                        TilePosition near = pos + normalInt;

                        if (tileManager.IsValidTile(near))
                        {
                            nearTileDrawMode = tileManager.GetTileDrawMode(near);
                            bool nearDynamic = tileManager.GetTileDynamic(near);

                            TilePosition nearAbove = near + new TilePosition(0, 1, 0);

                            bool drawingLiquidSurfaceBorder = (drawMode == nearTileDrawMode && 
                                drawMode == TileDefinition.DrawMode.LIQUID && 
                                drawingLiquidSurface == false && 
                                tileManager.IsValidTile(nearAbove) &&
                                tileManager.GetTileType(nearAbove) != tile.tileType &&
                                face != (int)CubeWorld.Utils.Graphics.Faces.Top && 
                                face != (int)CubeWorld.Utils.Graphics.Faces.Bottom);

                            if (drawMode != nearTileDrawMode ||
                                 drawMode == TileDefinition.DrawMode.SOLID_ALPHA ||
                                 nearDynamic ||
                                 drawingLiquidSurfaceBorder)
                            {
                                float lightIntensity = ambientLightIntensity *
                                                        MeshUtils.luminanceMapper[tileManager.GetTileAmbientLuminance(near)] +
                                                        MeshUtils.luminanceMapper[tileManager.GetTileLightSourceLuminance(near)];

                                if (lightIntensity > 1.0f)
                                    lightIntensity = 1.0f;

                                Color faceColor = new Color(lightIntensity * MeshUtils.faceBright[face],
                                                            lightIntensity * MeshUtils.faceBright[face],
                                                            lightIntensity * MeshUtils.faceBright[face]);
                                Vector3 faceNormal = MeshUtils.faceNormals[face];

                                if (tile.OnFire && MeshUtils.faceVectorsFireAvailable[face])
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        vertices.Add(MeshUtils.faceVectorsFire[(face << 2) + i] + offset);
                                        normals.Add(faceNormal);
                                        colors.Add(faceColor);
                                    }

                                    trianglesTranslucid.Add(index + 0);
                                    trianglesTranslucid.Add(index + 1);
                                    trianglesTranslucid.Add(index + 2);

                                    trianglesTranslucid.Add(index + 2);
                                    trianglesTranslucid.Add(index + 3);
                                    trianglesTranslucid.Add(index + 0);

                                    int fireMaterial = gameManagerUnity.extraMaterials.fireMaterials[(x + y + z) % gameManagerUnity.extraMaterials.fireMaterials.Length];

                                    float uvxFire = uvdelta * (fireMaterial % GraphicsUnity.TILE_PER_MATERIAL_ROW);
                                    float uvyFire = 1.0f - uvdelta * (fireMaterial / GraphicsUnity.TILE_PER_MATERIAL_ROW);

                                    uvs.Add(new Vector2(uvxFire, uvyFire - uvdelta));
                                    uvs.Add(new Vector2(uvxFire, uvyFire));
                                    uvs.Add(new Vector2(uvxFire + uvdelta, uvyFire));
                                    uvs.Add(new Vector2(uvxFire + uvdelta, uvyFire - uvdelta));

                                    uvs2.Add(Vector2.zero);
                                    uvs2.Add(Vector2.zero);
                                    uvs2.Add(Vector2.zero);
                                    uvs2.Add(Vector2.zero);

                                    index += 4;
                                }

                                if ((drawingLiquidSurface || drawingLiquidSurfaceBorder) && liquidVertexHeights == null)
                                    liquidVertexHeights = GetLiquidVertexHeights(tileManager, pos, tile);

                                for (int i = 0; i < 4; i++)
                                {
                                    if (drawingLiquidSurface || drawingLiquidSurfaceBorder)
                                    {
                                        Vector3 liquidVertex = faceVectors[(face << 2) + i];

                                        if (drawingLiquidSurface && liquidVertex.y == CubeWorld.Utils.Graphics.HALF_TILE_SIZE ||
                                            drawingLiquidSurfaceBorder && liquidVertex.y == -CubeWorld.Utils.Graphics.HALF_TILE_SIZE)
                                        {
                                            if (liquidVertex.x == CubeWorld.Utils.Graphics.HALF_TILE_SIZE)
                                            {
                                                //x = 1
                                                if (liquidVertex.z == CubeWorld.Utils.Graphics.HALF_TILE_SIZE)
                                                {
                                                    //z = 1
                                                    liquidVertex.y = liquidVertexHeights[0];
                                                }
                                                else
                                                {
                                                    //z = -1
                                                    liquidVertex.y = liquidVertexHeights[1];
                                                }
                                            }
                                            else
                                            {
                                                //x = -1
                                                if (liquidVertex.z == CubeWorld.Utils.Graphics.HALF_TILE_SIZE)
                                                {
                                                    //z = 1
                                                    liquidVertex.y = liquidVertexHeights[3];
                                                }
                                                else
                                                {
                                                    //z = -1
                                                    liquidVertex.y = liquidVertexHeights[2];
                                                }
                                            }
                                        }

                                        vertices.Add(liquidVertex + offset);
                                    }
                                    else
                                    {
                                        vertices.Add(faceVectors[(face << 2) + i] + offset);
                                    }

                                    normals.Add(faceNormal);
                                    colors.Add(faceColor);
                                }

                                triangles.Add(index + 0);
                                triangles.Add(index + 1);
                                triangles.Add(index + 2);

                                triangles.Add(index + 2);
                                triangles.Add(index + 3);
                                triangles.Add(index + 0);

                                float uvx = uvdelta * (material % GraphicsUnity.TILE_PER_MATERIAL_ROW);
                                float uvy = 1.0f - uvdelta * (material / GraphicsUnity.TILE_PER_MATERIAL_ROW);

                                uvs.Add(new Vector2(uvx, uvy - uvdelta));
                                uvs.Add(new Vector2(uvx, uvy));
                                uvs.Add(new Vector2(uvx + uvdelta, uvy));
                                uvs.Add(new Vector2(uvx + uvdelta, uvy - uvdelta));

                                if (drawMode == TileDefinition.DrawMode.SOLID && tile.Energy < tileDefinition.energy)
                                {
                                    int materialDamageIndex = ((tileDefinition.energy - tile.Energy) * gameManagerUnity.extraMaterials.damageMaterials.Length) / tileDefinition.energy;
                                    if (materialDamageIndex >= gameManagerUnity.extraMaterials.damageMaterials.Length)
                                        materialDamageIndex = gameManagerUnity.extraMaterials.damageMaterials.Length - 1;

                                    int materialDamage = gameManagerUnity.extraMaterials.damageMaterials[materialDamageIndex];

                                    uvx = uvdelta * (materialDamage % GraphicsUnity.TILE_PER_MATERIAL_ROW);
                                    uvy = 1.0f - uvdelta * (materialDamage / GraphicsUnity.TILE_PER_MATERIAL_ROW);

                                    uvs2.Add(new Vector2(uvx, uvy - uvdelta));
                                    uvs2.Add(new Vector2(uvx, uvy));
                                    uvs2.Add(new Vector2(uvx + uvdelta, uvy));
                                    uvs2.Add(new Vector2(uvx + uvdelta, uvy - uvdelta));
                                }
                                else
                                {
                                    uvs2.Add(Vector2.zero);
                                    uvs2.Add(Vector2.zero);
                                    uvs2.Add(Vector2.zero);
                                    uvs2.Add(Vector2.zero);
                                }

                                index += 4;
                            }
                        }
                    }
				}
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.colors = colors.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.uv2 = uvs2.ToArray();
        mesh.subMeshCount = 4;

        mesh.bounds = new Bounds(new Vector3(SectorManager.SECTOR_SIZE / 2, SectorManager.SECTOR_SIZE / 2, SectorManager.SECTOR_SIZE / 2), new Vector3(SectorManager.SECTOR_SIZE + 1, SectorManager.SECTOR_SIZE + 1, SectorManager.SECTOR_SIZE + 1));

        List<Material> materials = new List<Material>();

        if (trianglesNormal.Count > 0)
            materials.Add(gameManagerUnity.material);

        if (trianglesTranslucid.Count > 0)
            materials.Add(gameManagerUnity.materialTranslucid);

        if (trianglesTransparent.Count > 0)
            materials.Add(gameManagerUnity.materialTransparent);

        if (trianglesDamage.Count > 0)
            materials.Add(gameManagerUnity.materialDamaged);
		
		if (trianglesAnimated.Count > 0)
			materials.Add(gameManagerUnity.materialLiquidAnimated);

        meshRenderer.sharedMaterials = materials.ToArray();

        int trianglesGroupIndex = 0;

        if (trianglesNormal.Count > 0)
            mesh.SetTriangles(trianglesNormal.ToArray(), trianglesGroupIndex++);
        if (trianglesTranslucid.Count > 0)
            mesh.SetTriangles(trianglesTranslucid.ToArray(), trianglesGroupIndex++);
        if (trianglesTransparent.Count > 0)
            mesh.SetTriangles(trianglesTransparent.ToArray(), trianglesGroupIndex++);
        if (trianglesDamage.Count > 0)
            mesh.SetTriangles(trianglesDamage.ToArray(), trianglesGroupIndex++);
		if (trianglesAnimated.Count > 0)
			mesh.SetTriangles(trianglesAnimated.ToArray(), trianglesGroupIndex++);

        willRequireMeshUpdate = false;
        willRequireLightUpdate = false;
    }

    /**
     * Only update lighting, it works only if the mesh hasn't changed, so we can change only the color components.
     */
    public void UpdateAmbientLight()
    {
        if (visibleByPlayer == false)
        {
            willRequireLightUpdate = true;
            return;
        }

        if (totalAmbientLuminance == 0)
        {
            willRequireLightUpdate = false;
            return;
        }

        //If the mesh is invalid, then we don't do anything since the light is going to be updated
        //when the mesh is updated
        if (willRequireMeshUpdate == false)
        {
            CubeWorld.World.CubeWorld world = gameManagerUnity.world;
            TileManager tileManager = world.tileManager;

            Color[] colors = mesh.colors;

            int index = 0;

            int tileOffsetX = sector.tileOffset.x;
            int tileOffsetY = sector.tileOffset.y;
            int tileOffsetZ = sector.tileOffset.z;

            float ambientLightIntensity = MeshUtils.luminanceMapper[world.dayCycleManager.ambientLightLuminance];

            for (int z = tileOffsetZ; z < SectorManager.SECTOR_SIZE + tileOffsetZ; z++)
            {
                for (int y = tileOffsetY; y < SectorManager.SECTOR_SIZE + tileOffsetY; y++)
                {
                    for (int x = tileOffsetX; x < SectorManager.SECTOR_SIZE + tileOffsetX; x++)
                    {
                        TilePosition pos = new TilePosition(x, y, z);

                        Tile tile = tileManager.GetTile(pos);
                        if (tile.tileType == TileDefinition.EMPTY_TILE_TYPE || tile.Dynamic)
                            continue;

                        TileDefinition tileDefinition = tileManager.GetTileDefinition(tile.tileType);

                        TileDefinition.DrawMode drawMode = tileDefinition.drawMode;

                        bool drawingLiquidSurface = false;

                        if (drawMode == TileDefinition.DrawMode.LIQUID &&
                            tileManager.IsValidTile(pos + new TilePosition(0, 1, 0)) &&
                            tileManager.GetTileType(pos + new TilePosition(0, 1, 0)) != tile.tileType)
                        {
                            drawingLiquidSurface = true;
                        }

                        for (int face = 0; face < 6; face++)
                        {
                            int material = tileDefinition.materials[face];

                            if (material < 0)
                                continue;

                            TileDefinition.DrawMode nearTileDrawMode = TileDefinition.DrawMode.NONE;
                            TilePosition normalInt = MeshUtils.faceNormalsTile[face];
                            TilePosition near = pos + normalInt;

                            if (tileManager.IsValidTile(near))
                            {
                                nearTileDrawMode = tileManager.GetTileDrawMode(near);
                                bool nearDynamic = tileManager.GetTileDynamic(near);

                                TilePosition nearAbove = near + new TilePosition(0, 1, 0);

                                bool drawingLiquidSurfaceBorder = (drawMode == nearTileDrawMode &&
                                    drawMode == TileDefinition.DrawMode.LIQUID &&
                                    drawingLiquidSurface == false &&
                                    tileManager.IsValidTile(nearAbove) &&
                                    tileManager.GetTileType(nearAbove) != tile.tileType &&
                                    face != (int)CubeWorld.Utils.Graphics.Faces.Top &&
                                    face != (int)CubeWorld.Utils.Graphics.Faces.Bottom);


                                if (drawMode != nearTileDrawMode ||
                                    drawMode == TileDefinition.DrawMode.SOLID_ALPHA ||
                                    nearDynamic ||
                                    drawingLiquidSurfaceBorder)
                                {
                                    float lightIntensity = ambientLightIntensity *
                                                            MeshUtils.luminanceMapper[tileManager.GetTileAmbientLuminance(near)] +
                                                            MeshUtils.luminanceMapper[tileManager.GetTileLightSourceLuminance(near)];

                                    if (lightIntensity > 1.0f)
                                        lightIntensity = 1.0f;

                                    Color faceColor = new Color(lightIntensity * MeshUtils.faceBright[face],
                                                                lightIntensity * MeshUtils.faceBright[face],
                                                                lightIntensity * MeshUtils.faceBright[face]);

                                    if (tile.OnFire && MeshUtils.faceVectorsFireAvailable[face])
                                        for (int i = 0; i < 4; i++)
                                            colors[index++] = faceColor;

                                    for (int i = 0; i < 4; i++)
                                        colors[index++] = faceColor;
                                }
                            }
                        }
                    }
                }
            }

            mesh.colors = colors;

            willRequireLightUpdate = false;
        }
    }


    //Vertex order inside returned VertexHeights array:
    // i    x    z
    // 0    1    1
    // 1    1   -1
    // 2   -1   -1
    // 3   -1    1
    static private float[] GetLiquidVertexHeights(TileManager tileManager, TilePosition pos, Tile tile)
    {
        float[] liquidVertexHeights = new float[4];

        for (int i = 0; i < 4; i++)
            liquidVertexHeights[i] = MeshUtils.GetLiquidHeightForLevel(tile.ExtraData);

        //Back
        UpdateLiquidVertexHeights(pos, tile, liquidVertexHeights, tileManager, new TilePosition(0, 0, -1), 1, 2);
        //Front
        UpdateLiquidVertexHeights(pos, tile, liquidVertexHeights, tileManager, new TilePosition(0, 0, 1), 0, 3);
        //Left
        UpdateLiquidVertexHeights(pos, tile, liquidVertexHeights, tileManager, new TilePosition(-1, 0, 0), 2, 3);
        //Right
        UpdateLiquidVertexHeights(pos, tile, liquidVertexHeights, tileManager, new TilePosition(1, 0, 0), 0, 1);
        //Back-Left
        UpdateLiquidVertexHeights(pos, tile, liquidVertexHeights, tileManager, new TilePosition(-1, 0, -1), 2, -1);
        //Back-Right
        UpdateLiquidVertexHeights(pos, tile, liquidVertexHeights, tileManager, new TilePosition(1, 0, -1), 1, -1);
        //Front-Left
        UpdateLiquidVertexHeights(pos, tile, liquidVertexHeights, tileManager, new TilePosition(-1, 0, 1), 3, -1);
        //Front-Right
        UpdateLiquidVertexHeights(pos, tile, liquidVertexHeights, tileManager, new TilePosition(1, 0, 1), 0, -1);

        for (int i = 0; i < 4; i++)
        {
            liquidVertexHeights[i] *= 0.25f;
            liquidVertexHeights[i] -= 0.5f;
        }

        return liquidVertexHeights;
    }

    static private void UpdateLiquidVertexHeights(
        TilePosition pos, 
        Tile tile, 
        float[] liquidVertexHeights,
        TileManager tileManager,
        TilePosition nearDelta, 
        int indexHeight1, 
        int indexHeight2)
    {
        TilePosition nearPos = pos + nearDelta;

        if (tileManager.IsValidTile(nearPos))
        {
            Tile nearTile = tileManager.GetTile(nearPos);

            //If we are evaluating a corner, validate that the liquid corner is connected through one
            //of the sides to this liquid
            if (indexHeight2 != -1 ||
                indexHeight2 == -1 &&
                (tileManager.GetTileType(pos + nearDelta * new TilePosition(1, 0, 0)) == tile.tileType ||
                tileManager.GetTileType(pos + nearDelta * new TilePosition(0, 0, 1)) == tile.tileType))
            {
                if (nearTile.tileType == tile.tileType)
                {
                    float nearLiquidVertexHeight = MeshUtils.GetLiquidHeightForLevel(nearTile.ExtraData);

                    liquidVertexHeights[indexHeight1] += nearLiquidVertexHeight;
                    if (indexHeight2 != -1)
                        liquidVertexHeights[indexHeight2] += nearLiquidVertexHeight;
                }
                else
                {
                    liquidVertexHeights[indexHeight1] += 0.5f;
                    if (indexHeight2 != -1)
                        liquidVertexHeights[indexHeight2] += 0.5f;
                }
            }
            else
            {
                float f = MeshUtils.GetLiquidHeightForLevel(tile.ExtraData + 1);
                //Not connected, use this tile height - 1
                liquidVertexHeights[indexHeight1] += f;
                if (indexHeight2 != -1)
                    liquidVertexHeights[indexHeight2] += f;
            }
        }
    }
}

