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
 -  Also handles functionality for viruses.  If a player tries to eat one, they will split apart.  Algorithm uses is very similar to the one for splitting.  Viruses have uid's from 0 to 9.
 
### Web server
The web server is pretty close to a natural MVC style of webserver (almost). It will take in a number of routes /games /score /eaten. Stuff like that. When you go to a route that is undefined [http://localhost:11100/not-a-real-route])http://localhost:11100/not-a-real-route), it will give you a 404 error. If for some reason, there is an error in any of the controllers, a 500 error (with stack trace) will be displayed as well.

## Defining a protocol

We currently have defined the following protocol for specifying cube UIDs:
- 0 to 9 (10 slots) is for viruses.
- 10 to Food.Max - 1 (Food.Max slots) is for food cubes.
- Food.Max to int.Max (virtually unlimited) is for player cubes.

This route was chosen because it's best for choosing an ID and easy to identify the cube from the ID (from a debug standpoint)

## Notes:
- Contains unit tests for the two classes in the Model: Cube and World.
- Was made with scalibility in mind. Adding different types of blocks should be relatively trivial.

Project finished on 12/11/17
