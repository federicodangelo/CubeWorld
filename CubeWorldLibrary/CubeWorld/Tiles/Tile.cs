namespace CubeWorld.Tiles
{
    public struct Tile
    {
        public const byte MAX_LUMINANCE = 15;
        public const byte MAX_ENERGY = 15;

        public byte tileType;
        public byte luminance;
        public byte extra;
        public byte extra2;

        public bool OnFire
        {
            get { return (extra2 & 0x1) != 0; }
            set { if (value) extra2 |= 1; else extra2 = (byte)(extra2 & 0xFE); }
        }

        public bool Dynamic
        {
            get { return (extra2 & 0x2) != 0; }
            set { if (value) extra2 |= 2; else extra2 = (byte)(extra2 & 0xFD); }
        }


        public byte ExtraData
        {
            get { return (byte)(extra2 >> 4); }
            set { extra2 = (byte)((extra2 & 0x0F) | (value << 4)); }
        }
		
        public bool CastShadow
        {
            get { return (extra & 0x1) != 0; }
            set { if (value) extra |= 1; else extra = (byte) (extra & 0xFE); } 
        }

        public bool LightSource
        {
            get { return (extra & 0x2) != 0; }
            set { if (value) extra |= 2; else extra = (byte)(extra & 0xFD); }
        }

        public bool Enqueued
        {
            get { return (extra & 0x4) != 0; }
            set { if (value) extra |= 4; else extra = (byte)(extra & 0xFB); }
        }

        public bool Destroyed
        {
            get { return (extra & 0x8) != 0; }
            set { if (value) extra |= 8; else extra = (byte)(extra & 0xF7); }
        }

        public byte Energy
        {
            get { return (byte)(extra >> 4); }
            set { extra = (byte)((extra & 0x0F) | (value << 4)); }
        }

        public byte AmbientLuminance
        {
            get { return (byte) (luminance & 0xF); }
            set { luminance = (byte)((luminance & 0xF0) | value); }
        }

        public byte LightSourceLuminance
        {
            get { return (byte) (luminance >> 4); }
            set { luminance = (byte) ((luminance & 0x0F) |(value << 4)); }
        }
		
		public int Serialize()
		{
			return tileType | (luminance << 8) | (extra << 16) | (extra2 << 24);
		}
		
		public void Deserialize(uint data)
		{
			tileType = (byte) ((data >> 0) & 0xFF);
			luminance = (byte) ((data >> 8) & 0xFF);
			extra = (byte) ((data >> 16) & 0xFF);
			extra2 = (byte) ((data >> 24) & 0xFF);
		}
    }
}