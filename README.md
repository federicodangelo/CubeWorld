CubeWorld
=========

Minecraft-like game demo made using Unity 3D.

The interesting part is that the terrain generation and tiles definition is very easy to configurate since all of it is in XML files located at "CubeWorld/Assets/Resources", even the rules used to update the voxels in the engine are defined in there.

The project is split in 3 parts:

- CubeWorldLibrary: Base voxel game library, made in pure C# using Visual Studio 2008, implements simulation logic, sectors, items, tiles update logic, MULTIPLAYER support (yes, it has a very basic multiplayer support :-) )

- CubeWorld: Demo game made in Unity that uses CubeWorldLibrary

- CubeWorldWeb: Small Google App Engine application that handles the discovery of multiplayer games, when you create a server it automatically publishes it's IP to this web server for discovery by other clients.

Screenshot:

![Screenshot](/Screenshots/ss1.png)
