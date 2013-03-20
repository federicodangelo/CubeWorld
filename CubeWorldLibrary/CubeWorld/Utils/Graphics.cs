using System;
using System.Collections.Generic;
using CubeWorld.Tiles;
using CubeWorld.Tiles.Rules;

namespace CubeWorld.Utils
{
    public class Graphics
    {
        public const float TILE_SIZE = 1.0f;
        public const float HALF_TILE_SIZE = TILE_SIZE / 2.0f;
        public const float QUART_TILE_SIZE = TILE_SIZE / 4.0f;
        public const float ITEM_TILE_SIZE = TILE_SIZE * 0.25f;

        static public Vector3 TilePositionToVector3(int x, int y, int z)
        {
            return new Vector3(x * TILE_SIZE, y * TILE_SIZE, z * TILE_SIZE);
        }

        static public Vector3 TilePositionToVector3(TilePosition pos)
        {
            return new Vector3(pos.x * TILE_SIZE, pos.y * TILE_SIZE, pos.z * TILE_SIZE);
        }

        static public TilePosition Vector3ToTilePosition(Vector3 vec)
        {
            return new TilePosition(
                (int)Math.Round(vec.x / TILE_SIZE),
                (int)Math.Round(vec.y / TILE_SIZE),
                (int)Math.Round(vec.z / TILE_SIZE)
                    );
        }

        static public int FloatToTilePosition(float f)
        {
            return (int)Math.Round(f / TILE_SIZE);
        }

        static public float TilePositionToFloat(int pos)
        {
            return pos * TILE_SIZE;
        }

        public enum Faces
        {
            Back = 0,
            Front,
            Bottom,
            Top,
            Right,
            Left
        }

        public class RaycastTileResult
        {
            public bool hit = false;
            public TilePosition position;
            public Faces face;
            public Tile tile;
        }

        static public TilePosition GetFaceNormal(Faces face)
        {
            switch (face)
            {
                case Faces.Back:
                    return new TilePosition(0, 0, 1);
                case Faces.Front:
                    return new TilePosition(0, 0, -1);
                case Faces.Bottom:
                    return new TilePosition(0, -1, 0);
                case Faces.Top:
                    return new TilePosition(0, 1, 0);
                case Faces.Right:
                    return new TilePosition(1, 0, 0);
                case Faces.Left:
                    return new TilePosition(-1, 0, 0);
            }

            return new TilePosition(0, 0, 0);
        }

        static public Faces GetOpposingFace(Faces face)
        {
            switch (face)
            {
                case Faces.Back:
                    return Faces.Front;
                case Faces.Front:
                    return Faces.Back;
                case Faces.Bottom:
                    return Faces.Top;
                case Faces.Top:
                    return Faces.Bottom;
                case Faces.Right:
                    return Faces.Left;
                case Faces.Left:
                    return Faces.Right;
            }

            return Faces.Left;
        }

        static public RaycastTileResult RaycastTile(World.CubeWorld world, Vector3 from, Vector3 fwd, float maxDistance, bool ignoreNonSolid, bool ignoreNonClickable)
        {
            Vector3 pos = from;

            RaycastTileResult result = new RaycastTileResult();
			
			TileManager tileManager = world.tileManager;

            while (true)
            {
                Vector3 near = pos;

                //Find near planes
                for (int d = 0; d < 3; d++)
                {
                    near[d] = (float) Math.Round(near[d] / Graphics.TILE_SIZE) * Graphics.TILE_SIZE;
                    near[d] += Graphics.HALF_TILE_SIZE * Math.Sign(fwd[d]);
                }

                //Calculate nearest plane
                float[] ns = new float[3];

                for (int d = 0; d < 3; d++)
                {
                    if (fwd[d] != 0.0f)
                        ns[d] = (near[d] - pos[d]) / fwd[d];
                    else
                        ns[d] = 1.0f;
                }

                float minN = ns[0];
                int lockedAxis = 0;

                for (int d = 1; d < 3; d++)
                {
                    if (ns[d] < minN)
                    {
                        minN = ns[d];
                        lockedAxis = d;
                    }
                }

                //Advance until the border of the nearest plane
                pos += fwd * minN;

                if ((pos - from).magnitude >= maxDistance)
                    break;

                //See what tile is there
                Vector3 d2 = new Vector3();
                d2[lockedAxis] = Math.Sign(fwd[lockedAxis]) * Graphics.QUART_TILE_SIZE;

                TilePosition cursorPosition = Graphics.Vector3ToTilePosition(pos + d2);

                //Advance a little to stop the next loop from selecting the same tile / border
                pos += fwd * Graphics.TILE_SIZE * 0.0001f;


                //Now validate what to do :-)

                if (tileManager.IsValidTile(cursorPosition))
                {
                    byte tileType = tileManager.GetTileType(cursorPosition);

                    if (tileType != TileDefinition.EMPTY_TILE_TYPE &&
                        ((ignoreNonSolid == false || tileManager.GetTileSolid(cursorPosition) == true) ||
                        (ignoreNonClickable == false && 
                         tileManager.GetTileDefinition(tileType).GetRulesForAction(TileActionRule.ActionType.CLICKED) != null)))
                    {
                        switch (lockedAxis)
                        {
                            case 0:
                                if (Math.Sign(fwd[lockedAxis]) > 0)
                                    result.face = Faces.Left;
                                else
                                    result.face = Faces.Right;
                                break;
                            case 1:
                                if (Math.Sign(fwd[lockedAxis]) > 0)
                                    result.face = Faces.Bottom;
                                else
                                    result.face = Faces.Top;
                                break;
                            case 2:
                                if (Math.Sign(fwd[lockedAxis]) > 0)
                                    result.face = Faces.Front;
                                else
                                    result.face = Faces.Back;
                                break;
                        }

                        result.position = cursorPosition;
                        result.hit = true;
                        result.tile = tileManager.GetTile(cursorPosition);
                        break;
                    }
                }
            }

            return result;
        }
    }
}