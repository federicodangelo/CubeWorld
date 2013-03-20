using System;
using CubeWorld.Tiles;

namespace CubeWorld.World.Generator
{
    public class MidpointDisplacementWorldGenerator : CubeWorldGenerator
    {
        private byte tileType;
        public float roughness;
        private Random rnd = new Random();
        private float[,] map;
        private WorldSizeRelativeValue fromYRV;
        private WorldSizeRelativeValue toYRV;
        private int mapDimension;


        public MidpointDisplacementWorldGenerator(
            WorldSizeRelativeValue fromYRV,
            WorldSizeRelativeValue toYRV,
            float roughness,
			byte tileType)
        {
            this.fromYRV = fromYRV;
            this.toYRV = toYRV;
            this.roughness = roughness;
            this.tileType = tileType;
        }

        public override bool Generate(CubeWorld world)
        {
			TileManager tileManager = world.tileManager;

            int fromY = fromYRV.EvaluateInt(world);
            int toY = toYRV.EvaluateInt(world);

            width = tileManager.sizeX;
            height = tileManager.sizeZ;

            map = new float[width + 1, height + 1];

            //StartMidpointDisplacment();
            //Assign the four corners of the intial grid random color values
            //These will end up being the colors of the four corners of the applet.		

            float c1, c2, c3, c4;
            c1 = (float)rnd.NextDouble();
            c2 = (float)rnd.NextDouble();
            c3 = (float)rnd.NextDouble();
            c4 = (float)rnd.NextDouble();

            DivideGrid(0, 0, width, height, c1, c2, c3, c4);

            for (int x = 0; x < tileManager.sizeX; x++)
            {
                for (int z = 0; z < tileManager.sizeZ; z++)
                {
                    float d = map[x, z];

                    int y = fromY + (int)(d * (toY - fromY));

                    while (y >= fromY)
                        tileManager.SetTileType(new TilePosition(x, y--, z), tileType);
                }
            }

            return true;
        }

        public override string ToString()
        {
            return "Filling terrain with midpoint displacement";
        }





        private int width;
        private int height;

        //Randomly displaces value for midpoint depending on size
        //of grid piece.
        float Displace(float num)
        {
            float max = num / (float)(width + height) * roughness;
            return ((float)rnd.NextDouble() - 0.5f) * max;
        }
        
        //This is the recursive function that implements the random midpoint
        //displacement algorithm.  It will call itself until the grid pieces
        //become smaller than one pixel.	
        void DivideGrid(int x, int y, int width, int height, float c1, float c2, float c3, float c4)
        {
            float Edge1, Edge2, Edge3, Edge4, Middle;
            int newWidth = width / 2;
            int newHeight = height / 2;

            if (width > 1 || height > 1)
            {
                Middle = (c1 + c2 + c3 + c4) / 4 + Displace(newWidth + newHeight);	//Randomly displace the midpoint!
                Edge1 = (c1 + c2) / 2;	//Calculate the edges by averaging the two corners of each edge.
                Edge2 = (c2 + c3) / 2;
                Edge3 = (c3 + c4) / 2;
                Edge4 = (c4 + c1) / 2;

                //Make sure that the midpoint doesn't accidentally "randomly displaced" past the boundaries!
                if (Middle < 0)
                {
                    Middle = 0;
                }
                else if (Middle > 1.0f)
                {
                    Middle = 1.0f;
                }

                //Do the operation over again for each of the four new grids.			
                DivideGrid(x, y, newWidth, newHeight, c1, Edge1, Middle, Edge4);
                DivideGrid(x + newWidth, y, newWidth, newHeight, Edge1, c2, Edge2, Middle);
                DivideGrid(x + newWidth, y + newHeight, newWidth, newHeight, Middle, Edge2, c3, Edge3);
                DivideGrid(x, y + newHeight, newWidth, newHeight, Edge4, Middle, Edge3, c4);
            }
            else	//This is the "base case," where each grid piece is less than the size of a pixel.
            {
                //The four corners of the grid piece will be averaged and drawn as a single pixel.
                float c = (c1 + c2 + c3 + c4) / 4;

                map[(int)x, (int)y] = c;
            }
        }
        

        private float normalize(float v)
        {
            if (v > 1.0f)
                v = 1.0f;
            else if (v < 0.0f)
                v = 0.0f;
            return v;
        }

        private void StartMidpointDisplacment()
        {
            mapDimension = width;

            float tr, tl, br, bl, center;

            // top left
            map[0, 0] = (float)rnd.NextDouble();
            tl = map[0, 0];

            // bottom left
            map[0, mapDimension] = (float)rnd.NextDouble();
            bl = map[0, mapDimension];

            // top right
            map[mapDimension, 0] = (float)rnd.NextDouble();
            tr = map[mapDimension, 0];

            // bottom right
            map[mapDimension, mapDimension] = (float)rnd.NextDouble();
            br = map[mapDimension, mapDimension];

            // Center
            map[mapDimension / 2, mapDimension / 2] = map[0, 0] + map[0, mapDimension] + map[mapDimension, 0] + map[mapDimension, mapDimension] / 4;
            map[mapDimension / 2, mapDimension / 2] = normalize(map[mapDimension / 2, mapDimension / 2]);
            center = map[mapDimension / 2, mapDimension / 2];

            map[mapDimension / 2, mapDimension] = bl + br + center / 3;
            map[mapDimension / 2, 0] = tl + tr + center / 3;
            map[mapDimension, mapDimension / 2] = tr + br + center / 3;
            map[0, mapDimension / 2] = tl + bl + center / 3;

            MidpointDisplacment(mapDimension);
        }

        private void MidpointDisplacment(int dimension)
        {
		    int newDimension = dimension / 2;
            float topRight, topLeft, bottomLeft, bottomRight, center;
            int i, j;
    		
		    if (newDimension > 1){
			    for(i = newDimension; i <= width; i += newDimension){
				    for(j = newDimension; j <= height; j += newDimension)
                    {
					    int x = i - (newDimension / 2);
                        int y = j - (newDimension / 2);
    					
					    topLeft = map[i - newDimension, j - newDimension]; 
					    topRight = map[i,j - newDimension];
					    bottomLeft = map[i - newDimension,j];
					    bottomRight = map[i,j];
    					
					    // Center				
					    map[x,y] = (topLeft + topRight + bottomLeft + bottomRight) / 4 + Displace(dimension);
					    map[x,y] = normalize(map[x,y]);
					    center = map[x,y];	
    					
					    // Top
					    if(j - (newDimension * 2) + (newDimension / 2) > 0){
						    map[x,j - newDimension] = (topLeft + topRight + center + map[x,j - dimension + (newDimension / 2)]) / 4 + Displace(dimension);;
					    }else{
						    map[x,j - newDimension] = (topLeft + topRight + center) / 3+ Displace(dimension);
					    }
    					
					    map[x,j - newDimension] = normalize(map[x,j - newDimension]);
    			
					    // Bottom
					    if(j + (newDimension / 2) < mapDimension){
						    map[x,j] = (bottomLeft + bottomRight + center + map[x,j + (newDimension / 2)]) / 4+ Displace(dimension);
					    }else{
						    map[x,j] = (bottomLeft + bottomRight + center) / 3+ Displace(dimension);
					    }
    					
					    map[x,j] = normalize(map[x,j]);

    					
					    //Right
					    if(i + (newDimension / 2) < mapDimension){
						    map[i,y] = (topRight + bottomRight + center + map[i + (newDimension / 2),y]) / 4+ Displace(dimension);
					    }else{
						    map[i,y] = (topRight + bottomRight + center) / 3+ Displace(dimension);
					    }
    					
					    map[i,y] = normalize(map[i,y]);
    					
					    // Left
					    if(i - (newDimension * 2) + (newDimension / 2) > 0){
						    map[i - newDimension,y] = (topLeft + bottomLeft + center + map[i - dimension + (newDimension / 2),y]) / 4 + Displace(dimension);;
					    }else{
						    map[i - newDimension,y] = (topLeft + bottomLeft + center) / 3+ Displace(dimension);
					    }
    					
					    map[i - newDimension,y] = normalize(map[i - newDimension,y]);
				    }
			    }
			    MidpointDisplacment(newDimension);
		    }
	    }


    }
}