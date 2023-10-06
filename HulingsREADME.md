At first glance, Cube World looks exactly like Minecraft with its graphics and block-like structures. Cube World even has similar block colors to Minecraft. For example, the grass blocks and trees look the same, and the biomes look similar. Where Cube World starts to differ is there are different “classes” to choose from. These classes have a basic attack, a secondary charged attack, some combat skills, and an advanced special attack. From what I have seen, it takes 10-15 hours to unlock all the skills fully. Some of these important skills that help the user move around the map include riding, sailing, and hang-gliding. Another thing that makes this game different from Minecraft is Cube World is more combat-based, focused on character progression, and has actual missions to complete like fighting monsters and exploring dungeons.

Without looking at Minecraft's code, I would say that some of Cube World's materials and prefabs could possibly be taken directly from Minecraft and altered slightly. There is a file called MainTexture.png. This file looks exactly like some of the blocks in Minecraft. Within the materials folder are some picture files that are .png. The png files seem like the cloud files from Mission Demolition. I think these files are used to determine the color of everything in the game. 

However, after researching, Cube World does not use any of Minecraft's code. Cube World and Minecraft are separate and independent games developed by different individuals and teams, and they have no direct code-sharing or technical connections. While both games share a blocky, voxel-based visual style and emphasize exploration, they were created with separate codebases and game engines. Cube World was developed by Wolfram von Funck (Wollay) using his own custom engine. On the other hand, Minecraft was initially created by Markus Persson (Notch) and later developed and published by Mojang Studios. Minecraft uses the Java programming language and its own proprietary game engine.

Looking at this project, I can open it in any version of Unity that I have, but it loads slightly different after converting everything. The game uses a lot of special mechanics, including its movement that allows the user to strife, dodge, and shoot. The game also uses a leveling-up system that unlocks the skills for a player's class. It looks like this project uses a lot of various scripts to make this game fully functional. I am curious if altering a small amount of one of the many scripts would cause the entire game to fail. While I did not directly find the prefabs in this project, I was able to look at the materials like I previously mentioned. I believe that most of the objects in this game become randomly generated. Although this game is very different from the hunting game I would like to build; I think that the open-world concept and the randomly generated biomes could be helpful in making my game. 


Original Text from README below
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
