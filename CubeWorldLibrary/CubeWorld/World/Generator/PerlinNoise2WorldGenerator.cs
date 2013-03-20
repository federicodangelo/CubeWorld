using System;
using CubeWorld.Tiles;

namespace CubeWorld.World.Generator
{
    public class PerlinNoise2WorldGenerator : CubeWorldGenerator
    {
        private WorldSizeRelativeValue fromYRV;
        private WorldSizeRelativeValue toYRV;
        private byte tileType;
		private int octaves;
		private float freq;

        public PerlinNoise2WorldGenerator(
			WorldSizeRelativeValue fromYRV,
            WorldSizeRelativeValue toYRV,
			int octaves,
			float freq,
			byte tileType)
        {
            this.fromYRV = fromYRV;
            this.toYRV = toYRV;
            this.tileType = tileType;
			this.octaves = octaves;
			this.freq = freq;
        }
		
		static private int offX;
		static private int offY;
		static private bool offInitialized;

        public override bool Generate(CubeWorld world)
        {
			if (offInitialized == false)
			{
				Random rnd = new Random();
				offX = rnd.Next(0, 9999999);
				offY = rnd.Next(0, 9999999);
				offInitialized = true;
			}

			TileManager tileManager = world.tileManager;
			
            int fromY = fromYRV.EvaluateInt(world);
            int toY = toYRV.EvaluateInt(world);
			
			int cutY = (int) (fromY + (toY - fromY) * 0.9f);
            int midY = fromY + (toY - fromY) / 2;
			
            for (int x = 0; x < tileManager.sizeX; x++)
			{
                for (int z = 0; z < tileManager.sizeZ; z++)
				{
                    float d = FBM(x + offX, z + offY, octaves, 0.5f, freq, 1.0f / octaves, 2.0f);
                    //float d = MultiFractal(x, z, octaves, 0.5f, freq, 1.0f, 2.0f, 1.0f);
					
					int y = midY + (int) (d * (toY - fromY));

                    //This line below can be used to make calderas
                    //if (y > cutY)
                    //    y = cutY - (y - cutY);  
					
					if (y > cutY)
						y = cutY;
					else if (y < fromY - 1)
						y = fromY - 1;

                    while (y >= fromY)
                        tileManager.SetTileType(new TilePosition(x, y--, z), tileType);
				}
			}
			

            return true;
        }

        static double cosineinterpolation(double number1, double number2, double x)
        {
            double ft;
            double f;
            double ret;
            ft = x * 3.1415927;
            f = (1 - Math.Cos(ft)) * .5;
            ret = number1 * (1 - f) + number2 * f;
            return ret;
        }

        static double randomnumber(int x, int y)
        {
            int n = x + y * 57;
            n = (n << 13) ^ n;
            double ret;
            ret = (1 - ((n * (n * n * 15731 + 789221) + 1376312589) & 2147483647) / 1073741824.0);
            return ret;
        }

        static float smoothrandom(int x, int y)
        {
            double corners = (randomnumber(x - 1, y - 1) + randomnumber(x + 1, y - 1) + randomnumber(x - 1, y + 1) + randomnumber(x + 1, y + 1)) / 16;
            double sides = (randomnumber(x - 1, y) + randomnumber(x + 1, y) + randomnumber(x, y - 1) + randomnumber(x, y + 1)) / 8;
            double center = randomnumber(x, y) / 4;
            float ret = (float) (corners + sides + center);
            return ret;
        }

        //Can use smoothrandom or randomnumber
        static float noise(float x, float y)
        {
            int xinterger = (int) x;
            float fractionx = x - xinterger;
            int yinteger = (int) y;
            float fractiony = y - yinteger;
            double v1, v2, v3, v4, i1, i2;
            float ret;
            v1 = randomnumber(xinterger, yinteger);
            v2 = randomnumber(xinterger + 1, yinteger);
            v3 = randomnumber(xinterger, yinteger + 1);
            v4 = randomnumber(xinterger + 1, yinteger + 1);
            i1 = cosineinterpolation(v1, v2, fractionx);
            i2 = cosineinterpolation(v3, v4, fractionx);
            ret = (float) cosineinterpolation(i1, i2, fractiony);
            return ret;
        }

        static float MultiFractal(float x, float y, int octaves, float amplitude, float frequency, float h, float lacunarity, float offset)
        {
            float ret = 1;
            for (int i = 0; i < octaves; i++)
            {
                ret *= (offset) * ((float) simplexNoise(x * frequency, y * frequency) * amplitude);
                frequency *= lacunarity;
                amplitude *= h;
            }
            return ret;
        }

        static float FBM(float x, float y, int octaves, float amplitude, float frequency, float h, float lacunarity)
        {
            float ret = 0;
            for (int i = 0; i < octaves; i++)
            {
                ret += ((float) simplexNoise(x * frequency, y * frequency) * amplitude);
                frequency *= lacunarity;
                amplitude *= h;
            }
            return ret;
        }


        public override string ToString()
        {
            return "Filling terrain with perlin noise";
        }
		
		
		private static double dot(int[] g, double x, double y) {
			return g[0]*x + g[1]*y; }
		  private static double dot(int[] g, double x, double y, double z) {
			return g[0]*x + g[1]*y + g[2]*z; }
		  private static double dot(int[] g, double x, double y, double z, double w) {
			return g[0]*x + g[1]*y + g[2]*z + g[3]*w; }		private static int fastfloor(double x) {
			return x>0 ? (int)x : (int)x-1;
		}		
		
		private static int[] perm;
		
		private static int[][] grad3;
								 
		private static int[] p;							 
		
		// 2D simplex noise
	  public static double simplexNoise(double xin, double yin) {
		  
		  int i;
		  
		  if (perm == null)
		  {
			  grad3 = new int[12][];
			  grad3[0] = new int[] {1,1,0};
			  grad3[1] = new int[] {-1,1,0};
			  grad3[2] = new int[] {1,-1,0};
			  grad3[3] = new int[] {-1,-1,0};
			  grad3[4] = new int[] {1,0,1};
			  grad3[5] = new int[] {-1,0,1};
			  grad3[6] = new int[] {1,0,-1};
			  grad3[7] = new int[] {-1,0,-1};
				grad3[8] = new int[] {0,1,1};
				grad3[9] = new int[] {0,-1,1};
				grad3[10] = new int[] {0,1,-1};
				grad3[11] = new int[] {0,-1,-1};
								 
		p = new int[] {151,160,137,91,90,15,
		  131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
		  190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
		  88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
		  77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
		  102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
		  135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
		  5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
		  223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
		  129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
		  251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
		  49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
		  138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180};									 
			  
			   perm = new int[512];
			  for(i=0; i<512; i++) perm[i]=p[i & 255];
		  }
		  
		double n0, n1, n2; // Noise contributions from the three corners
		// Skew the input space to determine which simplex cell we're in
		double F2 = 0.5*(Math.Sqrt(3.0)-1.0);
		double s = (xin+yin)*F2; // Hairy factor for 2D
		i = fastfloor(xin+s);
		int j = fastfloor(yin+s);
		double G2 = (3.0-Math.Sqrt(3.0))/6.0;
		double t = (i+j)*G2;
		double X0 = i-t; // Unskew the cell origin back to (x,y) space
		double Y0 = j-t;
		double x0 = xin-X0; // The x,y distances from the cell origin
		double y0 = yin-Y0;
		// For the 2D case, the simplex shape is an equilateral triangle.
		// Determine which simplex we are in.
		int i1, j1; // Offsets for second (middle) corner of simplex in (i,j) coords
		if(x0>y0) {i1=1; j1=0;} // lower triangle, XY order: (0,0)->(1,0)->(1,1)
		else {i1=0; j1=1;}      // upper triangle, YX order: (0,0)->(0,1)->(1,1)
		// A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
		// a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
		// c = (3-sqrt(3))/6
		double x1 = x0 - i1 + G2; // Offsets for middle corner in (x,y) unskewed coords
		double y1 = y0 - j1 + G2;
		double x2 = x0 - 1.0 + 2.0 * G2; // Offsets for last corner in (x,y) unskewed coords
		double y2 = y0 - 1.0 + 2.0 * G2;
		// Work out the hashed gradient indices of the three simplex corners
		int ii = i & 255;
		int jj = j & 255;
		int gi0 = perm[ii+perm[jj]] % 12;
		int gi1 = perm[ii+i1+perm[jj+j1]] % 12;
		int gi2 = perm[ii+1+perm[jj+1]] % 12;
		// Calculate the contribution from the three corners
		double t0 = 0.5 - x0*x0-y0*y0;
		if(t0<0) n0 = 0.0;
		else {
		  t0 *= t0;
		  n0 = t0 * t0 * dot(grad3[gi0], x0, y0);  // (x,y) of grad3 used for 2D gradient
		}
		double t1 = 0.5 - x1*x1-y1*y1;
		if(t1<0) n1 = 0.0;
		else {
		  t1 *= t1;
		  n1 = t1 * t1 * dot(grad3[gi1], x1, y1);
		}    double t2 = 0.5 - x2*x2-y2*y2;
		if(t2<0) n2 = 0.0;
		else {
		  t2 *= t2;
		  n2 = t2 * t2 * dot(grad3[gi2], x2, y2);
		}
		// Add contributions from each corner to get the final noise value.
		// The result is scaled to return values in the interval [-1,1].
		return 70.0 * (n0 + n1 + n2);
	  }		
    }
}