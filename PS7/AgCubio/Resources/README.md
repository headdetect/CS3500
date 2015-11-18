# AgCubio
### By Brayden Lopez and Eric Miramontes - Team: "You buildin a little team there?"

## Designs
The project (PS7) is set up with the following classes:
- Cube.cs
  -  Is the model representing cubes, be they food or player.
- World.cs
  - Is the model that holds other cubes representing the playing field.
  - Contains utilital subroutines for grabbing different types of cubes.
- NetworkManager.cs
  - A singleton type class interacting with the AgCubio server.
  - Can send commands, the name, and a quit message
- ConnectForm.cs
  - This is a dialog box for the user to enter their gamer-tag and join a game.
- GameWindow.cs
  - This is the main game window.  It zooms in on the player's cube depending on size and has various game stats in the upper     right hand corner.
- Program.cs
  - Will keep opening GameWindow.cs, unless specified. 

### Dealing with the server

There were so many problems dealing with the implementation of the server.  
When closing the connection (sending `QUIT` as well as closing the socket),  
the server will sometimes have issues closing the socket and be stuck in a loop  
where it can't accept anymore connections.  
  
Sometimes, we would get disconnected upon connection.

Blah blah blah.

## Notes:
Contains unit tests for the two classes in the Model: Cube and World.

Project finished on 11/17/2015
