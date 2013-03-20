namespace CubeWorld.Sectors
{
    public interface ISectorGraphics
    {
		void SetSector(Sector sector);
		
        void UpdateMesh();
        void UpdateAmbientLight();
    }
}