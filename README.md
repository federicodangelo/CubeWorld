# CubeWorld 
=========
<details open="open">
  <summary>Table of Contents</summary>
  <ol>
    <li>
        <a href="#project-description">Project Description</a>
    </li>
    <li>
        <a href="#how-to-install">How to Install</a>
    </li>
    <li>
		<a href="#how-to-contribute">How to Contribute</a>
    </li>
    <li>
        <a href="#controls">Controls</a>
    </li>
     <li>
		<a href="#screenshots">Screenshots</a>
    </li>
  </ol>
</details>

## Project Description
- Minecraft-like game demo made using Unity 3D.
- Features easy configuration of terrain generation, tile definitions, and voxel update rules via XML files located at CubeWorld/Assets/Resources
- This project is divided into three parts
  - **CubeWorldLibrary:** A base voxel game library created in pure C# using Visual Studio 2008. It implements simulation logic, sectors, items, tile update logic, and even basic multiplayer support.
  - **CubeWorld:** A demo game built in Unity that utilizes CubeWorldLibrary.
  - **CubeWorldWeb:** A small Google App Engine application responsible for discovering multiplayer games. When a server is created, it automatically publishes its IP to this web server for discovery by other clients.

## How to Install
 - Create a fork of the project by clicking fork in the top right of the GitHub page
 - Download the latest version of Unity from https://unity.com/download
 - Open the CubeWorld portion of the project in Unity Hub
 - Click on CubeWorld in the Projects menu of Unity Hub
 - Click the play button at the top of the newly opened window

  ## How to Contribute
  - Create a fork of the project by clicking fork in the top right of the GitHub page
  - Clone your fork to your machine utilizing GitHub Desktop or GitBash
  - If using GitBash, run the command **gitclone https://github.com/(yourusernamehere)/CubeWorld**
  - If using GitHub Desktop, click file in the top left, click clone  repository, and then put the link to your forked repository.
  - Refer to the Project Description section for locations of different aspects of the project
  - Commit and push changes
  - Make a pull request when finished with your modifications

  ## Controls
  - Press 'I' to open inventory
  - Right-click to place objects
  - Left click to activate/destroy objects
  - ESC for options


  ## Screenshots
  ![Screenshot](/Screenshots/ss1.png)
  ![Screenshot](https://github.com/Serperino/CubeWorld/assets/80271301/2eb1df51-0efc-45df-8785-7e9dd63b313b)
  ![Screenshot](https://github.com/Serperino/CubeWorld/assets/80271301/f89169e6-ecac-473e-95de-f5d90b04f9c8)



