using UnityEngine;
using CubeWorld.Tiles;

public class GraphicsUnity
{
    public const int TILE_PER_MATERIAL_ROW = 16;

    static public Vector3 TilePositionToVector3(int x, int y, int z)
    {
        return new Vector3(x * CubeWorld.Utils.Graphics.TILE_SIZE, y * CubeWorld.Utils.Graphics.TILE_SIZE, z * CubeWorld.Utils.Graphics.TILE_SIZE);
    }

    static public Vector3 TilePositionToVector3(TilePosition pos)
    {
        return new Vector3(pos.x * CubeWorld.Utils.Graphics.TILE_SIZE, pos.y * CubeWorld.Utils.Graphics.TILE_SIZE, pos.z * CubeWorld.Utils.Graphics.TILE_SIZE);
    }

    static public TilePosition Vector3ToTilePosition(Vector3 vec)
    {
        return new TilePosition(
            (int)Mathf.Round(vec.x / CubeWorld.Utils.Graphics.TILE_SIZE),
            (int)Mathf.Round(vec.y / CubeWorld.Utils.Graphics.TILE_SIZE),
            (int)Mathf.Round(vec.z / CubeWorld.Utils.Graphics.TILE_SIZE)
                );
    }

    static public int FloatToTilePosition(float f)
    {
        return (int)Mathf.Round(f / CubeWorld.Utils.Graphics.TILE_SIZE);
    }

    static public float TilePositionToFloat(int pos)
    {
        return pos * CubeWorld.Utils.Graphics.TILE_SIZE;
    }

    static public Vector3 CubeWorldVector3ToVector3(CubeWorld.Utils.Vector3 vec)
    {
        return new Vector3(vec.x, vec.y, vec.z);
    }

    static public CubeWorld.Utils.Vector3 Vector3ToCubeWorldVector3(Vector3 vec)
    {
        return new CubeWorld.Utils.Vector3(vec.x, vec.y, vec.z);
    }

    static public Color CubeWorldColorToColor(CubeWorld.Utils.Color cubeWorldColor)
    {
        return new Color(cubeWorldColor.r, cubeWorldColor.g, cubeWorldColor.b, cubeWorldColor.a);
    }

    static public Texture2D GetTilesetTexture(Texture2D tilesetTexture, int tileNumber)
    {
        int uvdelta = tilesetTexture.width / GraphicsUnity.TILE_PER_MATERIAL_ROW;

        int uvx = uvdelta * (tileNumber % GraphicsUnity.TILE_PER_MATERIAL_ROW);
        int uvy = tilesetTexture.height - uvdelta * (tileNumber / GraphicsUnity.TILE_PER_MATERIAL_ROW);

        return GetTextureSlice(tilesetTexture, uvx, uvy, uvdelta, uvdelta);
    }

    static public Texture2D GetTextureSlice(Texture2D from, int x, int y, int width, int height)
    {
        Texture2D tileTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);

        tileTexture.SetPixels(from.GetPixels(x, y - height, width, height));
        tileTexture.filterMode = FilterMode.Point;

        tileTexture.Apply();

        return tileTexture;
    }
}

