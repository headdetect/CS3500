# AgCubio
### By Brayden Lopez and Eric Miramontes - Team: "You buildin a little team there?"

## Designs
The project (PS7) is set up with the following classes:
- Cube.cs
  -  Is the modal representing cubes, be they food or player.
- World.cs
  - Is the modal that holds other cubes representing the playing field.
  - Contains utilital subroutines for grabbing different types of cubes.
- NetworkManager.cs
  - A singleton type class interacting with the AgCubio server.
  - Can send commands, the name, and a quit message
- ConnectForm.cs
- GameWindow.cs
- Program.cs
  - Will keep opening GameWindow.cs, unless specified. 

### Dealing with the server

There were so many problems dealing with the implementation of the server.  
When closing the connection (sending `QUIT` as well as closing the socket),  
the server will sometimes have issues closing the socket and be stuck in a loop  
where it can't accept anymore connections.  
  
Sometimes, we would get disconnected upon connection.

Blah blah blah.
