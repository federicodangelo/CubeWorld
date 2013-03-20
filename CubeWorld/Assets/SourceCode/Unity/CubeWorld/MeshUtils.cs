using UnityEngine;
using CubeWorld.Tiles;

public struct MeshUtils
{
    static public Vector3[] faceVectorsNormal;
    static public Vector3[] faceVectorsFire;
    static public Vector3[] faceNormals;
    static public bool[] faceVectorsFireAvailable;
    static public TilePosition[] faceNormalsTile;
    static public float[] faceBright;
    static public float[] luminanceMapper;

    public const int MAX_LIQUID_LEVELS = 8;

    static public float GetLiquidHeightForLevel(int level)
    {
        if (level <= MAX_LIQUID_LEVELS)
            return (MAX_LIQUID_LEVELS - level) / (float)MAX_LIQUID_LEVELS;
        else
            return 0;
    }

    static public void InitStaticValues()
    {
        faceVectorsNormal = new Vector3[6 * 4];
        faceNormals = new Vector3[6];
        faceBright = new float[6];
        faceNormalsTile = new TilePosition[6];
        luminanceMapper = new float[Tile.MAX_LUMINANCE + 1];

        //back
        faceVectorsNormal[0] = new Vector3(-CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE);
        faceVectorsNormal[1] = new Vector3(-CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE);
        faceVectorsNormal[2] = new Vector3(CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE);
        faceVectorsNormal[3] = new Vector3(CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE);

        faceNormals[0] = Vector3.back;

        //front
        faceVectorsNormal[4] = new Vector3(CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE);
        faceVectorsNormal[5] = new Vector3(CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE);
        faceVectorsNormal[6] = new Vector3(-CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE);
        faceVectorsNormal[7] = new Vector3(-CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE);

        faceNormals[1] = Vector3.forward;

        //bottom
        faceVectorsNormal[8] = new Vector3(CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE);
        faceVectorsNormal[9] = new Vector3(CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE);
        faceVectorsNormal[10] = new Vector3(-CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE);
        faceVectorsNormal[11] = new Vector3(-CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE);

        faceNormals[2] = Vector3.down;

        //top
        faceVectorsNormal[12] = new Vector3(CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE);
        faceVectorsNormal[13] = new Vector3(CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE);
        faceVectorsNormal[14] = new Vector3(-CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE);
        faceVectorsNormal[15] = new Vector3(-CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE);

        faceNormals[3] = Vector3.up;

        //right
        faceVectorsNormal[16] = new Vector3(CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE);
        faceVectorsNormal[17] = new Vector3(CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE);
        faceVectorsNormal[18] = new Vector3(CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE);
        faceVectorsNormal[19] = new Vector3(CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE);

        faceNormals[4] = Vector3.right;

        //left
        faceVectorsNormal[20] = new Vector3(-CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE);
        faceVectorsNormal[21] = new Vector3(-CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE);
        faceVectorsNormal[22] = new Vector3(-CubeWorld.Utils.Graphics.HALF_TILE_SIZE, CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE);
        faceVectorsNormal[23] = new Vector3(-CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE, -CubeWorld.Utils.Graphics.HALF_TILE_SIZE);

        faceNormals[5] = Vector3.left;

        faceBright[0] = faceBright[1] = 0.6f * 0.6f;
        faceBright[2] = faceBright[3] = 1.0f * 0.6f;
        faceBright[4] = faceBright[5] = 0.8f * 0.6f;

        for (int i = 0; i < 6; i++)
            faceNormalsTile[i] = new TilePosition((int)faceNormals[i].x, (int)faceNormals[i].y, (int)faceNormals[i].z);

        //Fire face vectors, displaced from the original position
        faceVectorsFire = new Vector3[6 * 4];
        faceVectorsFireAvailable = new bool[6];

        for (int i = 0; i < 6 * 4; i++)
        {
            faceVectorsFire[i] = faceVectorsNormal[i];

            if (faceVectorsFire[i].x == CubeWorld.Utils.Graphics.HALF_TILE_SIZE)
                faceVectorsFire[i].x += CubeWorld.Utils.Graphics.HALF_TILE_SIZE * 0.1f;
            else if (faceVectorsFire[i].x == -CubeWorld.Utils.Graphics.HALF_TILE_SIZE)
                faceVectorsFire[i].x -= CubeWorld.Utils.Graphics.HALF_TILE_SIZE * 0.1f;

            if (faceVectorsFire[i].z == CubeWorld.Utils.Graphics.HALF_TILE_SIZE)
                faceVectorsFire[i].z += CubeWorld.Utils.Graphics.HALF_TILE_SIZE * 0.1f;
            else if (faceVectorsFire[i].z == -CubeWorld.Utils.Graphics.HALF_TILE_SIZE)
                faceVectorsFire[i].z -= CubeWorld.Utils.Graphics.HALF_TILE_SIZE * 0.1f;
        }

        //No fire on top or bottom face of tiles on fire
        faceVectorsFireAvailable[(int)CubeWorld.Utils.Graphics.Faces.Right] = true;
        faceVectorsFireAvailable[(int)CubeWorld.Utils.Graphics.Faces.Left] = true;
        faceVectorsFireAvailable[(int)CubeWorld.Utils.Graphics.Faces.Front] = true;
        faceVectorsFireAvailable[(int)CubeWorld.Utils.Graphics.Faces.Back] = true;
        faceVectorsFireAvailable[(int)CubeWorld.Utils.Graphics.Faces.Top] = false;
        faceVectorsFireAvailable[(int)CubeWorld.Utils.Graphics.Faces.Bottom] = false;

        luminanceMapper[Tile.MAX_LUMINANCE] = 1.0f;
        for (int i = Tile.MAX_LUMINANCE - 1; i >= 0; i--)
            luminanceMapper[i] = luminanceMapper[i + 1] * 0.8f;

        //luminanceMapper[0] = 0.0f;
    }
}

