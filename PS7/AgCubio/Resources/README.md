# AgCubio
### By Brayden Lopez and Eric Miramontes - Team: "You buildin a little team there?"

## Designs
The project (PS7) is set up with the following classes:

### Models
- Cube.cs
  -  Is the model representing cubes, be they food or player.
- World.cs
  - Is the model that holds other cubes representing the playing field.
  - Contains different subroutines for interacting with cubes **(in a threadsafe fashion)**
  
### Networking
- ClientNetworkManager.cs
  - A singleton type class interacting with the AgCubio server.
  - Can send commands, the name, and a quit message
- ServerNetworkManager.cs
  - An object that forks out client connections to a seperate thread and handles them as data comes in
  - Keeps track of all the clients with their respective IDs (IDs are generated elsewhere to maintain the SoC mentality) 

### Client
- ConnectForm.cs
  - This is a dialog box for the user to enter their gamer-tag and join a game.
- GameWindow.cs
  - This is the main game window.  It zooms in on the player's cube depending on size and has various game stats in the upper     right hand corner.
- Program.cs
  - Will keep opening GameWindow.cs, unless specified. 

### Server
- Program.cs
 -  This is the brains of the server. It has abstracted out the networking components into the ServerNetworkManager.cs
 -  Handles all the game mechanics such as moving, eating, and dying.

## Defining a protocol

We currently have defined the following protocol for specifying cube UIDs:
- 0 to 9 (10 slots) is for viruses.
- 10 to Food.Max - 1 (Food.Max slots) is for food cubes.
- Food.Max to int.Max (virtually unlimited) is for player cubes.

This route was chosen because it's best for choosing an ID and easy to identify the cube from the ID (from a debug standpoint)

## Notes:
- Contains unit tests for the two classes in the Model: Cube and World.
- Was made with scalibility in mind. Adding different types of blocks should be relatively trivial.

Project finished on 12/3/17
