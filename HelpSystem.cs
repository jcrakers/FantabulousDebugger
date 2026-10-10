namespace FantabulousDebugger;

public static class HelpSystem
{
    public static void HandleHelpCommand(string[] commandParts)
    {
        if (commandParts.Length > 1)
        {
            FantabulousDebugger.Logger.LogInfo(HelpForCommand(commandParts[1]));
            return;
        }

        FantabulousDebugger.Logger.LogInfo(@"Available commands:
help: Tells you about available commands. Pass the command name as an argument to get more info about it.
godmode: Toggles godmode.
noclip: Toggles noclip.
tp: Teleports to the coordinates.
level: Loads the level provided.
scan: Scans and prints game objects. Use 'scan simple' for basic output.
inspect: Inspects object you're looking at or provide object name. Use 'inspect object component' for specific component inspection.
fullbright: Toggles fullbright lighting mode.
maxhealth: Sets the player's maximum health.
weapons: Unlocks all weapons.
machinegun: Enables machine gun mode.
sausage: Adds sausages to the player's count.
destroy: Destroys the object you're looking at.
wireframe: Toggles wireframe mode.
collision: Toggles collision visualization.
pointer: Creates/manages a debug pointer marker.");
    }

    public static string HelpForCommand(string command)
    {
        switch (command.ToLower())
        {
            case "help":
                return @"Command: help
Description: Shows available commands or detailed help for specific commands
Usage: 
  help                    - Shows all available commands
  help <command>          - Shows detailed help for specific command

Examples:
  help                    - Lists all commands
  help tp                 - Shows help for teleport command";
                
            case "godmode":
                return @"Command: godmode
Description: Toggles invincibility mode
Usage: godmode

Effects:
  - When enabled: Player health is constantly set to maximum
  - When disabled: Player health returns to normal
  - Useful for: Testing difficult areas, avoiding death
  
Note: Health is automatically restored each frame while enabled";
                
            case "noclip":
                return @"Command: noclip [speed <value>|boost <multiplier>]
Description: Toggles collision-free movement mode or adjusts speed settings
Usage: 
  noclip                    - Toggle noclip mode
  noclip speed <value>      - Set normal movement speed
  noclip boost <multiplier> - Set boost speed as multiplier of normal speed

Effects:
  - When enabled: Player can move through walls and objects
  - When disabled: Normal collision detection restored
  - Movement: WASD + Space (up) + Ctrl (down) + Shift (boost)
  - Default speeds: Normal (50), Boost (100)
  
Speed Settings:
  - speed: Sets the base movement speed (must be > 0)
  - boost: Sets boost speed as multiplier of normal speed (must be > 0)
  - Example: 'noclip speed 75' sets normal speed to 75
  - Example: 'noclip boost 3' sets boost to 3x normal speed

Note: Disables gravity and collision while enabled";
                
            case "tp":
                return @"Command: tp <x> <y> <z> OR tp <object_name>
Description: Teleports player to specified coordinates or to a game object's position
Usage: tp <x> <y> <z> or tp <object_name>

Parameters:
  x, y, z - World coordinates to teleport to
  object_name - Name of game object to teleport to

Examples:
  tp 0 10 5            - Teleport to coordinates (0, 10, 5)
  tp -100.5 2.3 45.2  - Teleport with decimal coordinates
  tp Player             - Teleport to Player object's position
  tp door               - Teleport to door object's position
  tp orangeSphere        - Teleport to orangeSphere object's position

Note: Supports partial name matching for object names";
                
            case "level":
                return @"Command: level <name|index>
Description: Loads specified level
Usage: level <level_name> or level <level_index>

Available Levels:
  0: title
  1: intro
  2: hub
  3: circlefriendfightyplace
  4: ShamrockFakeCastle
  5: signvilleussr
  6: signmanBattle
  7: thefantabula
  8: hubdarkened
  9: prison
  10: Sham5KFight1
  11: ShamrockCastle
  12: ShamrockKingBoss

Examples:
  level hub               - Loads hub level
  level 3                 - Loads circlefriendfightyplace
  level ShamrockCastle     - Loads ShamrockCastle level

Note: Level names are case-insensitive";
                
            case "scan":
                return @"Command: scan [simple|detailed]
Description: Scans and lists all game objects in the scene
Usage: scan or scan simple or scan detailed

Parameters:
  simple    - Shows basic object information (default)
  detailed  - Shows detailed object information

Examples:
  scan                  - Shows basic object list
  scan simple           - Shows basic object list
  scan detailed          - Shows detailed object information

Note: Useful for finding object names for other commands";
                
            case "inspect":
                return @"Command: inspect [object_name] [component_name]
Description: Shows detailed information about game objects or components
Usage: inspect or inspect <object_name> or inspect <object_name> <component>

Parameters:
  object_name   - Name of object to inspect (optional)
  component    - Specific component to inspect (optional)

Examples:
  inspect                    - Inspects object you're looking at
  inspect Player             - Inspects Player object
  inspect Player rigidbody   - Inspects Rigidbody component on Player
  inspect door boxcollider   - Inspects BoxCollider component on door object

Note: Supports partial name matching and component-specific details";
                
            case "fullbright":
                return @"Command: fullbright [on|off]
Description: Toggles enhanced lighting mode and removes fog for maximum visibility
Usage: 
  fullbright           - Toggle fullbright mode on/off
  fullbright on        - Enable fullbright mode
  fullbright off       - Disable fullbright mode

Effects:
  - When enabled: Enhances all scene lighting and removes fog for maximum visibility
  - When disabled: Restores normal lighting and original fog settings
  - Creates directional light if no lights exist
  - Increases intensity of existing lights
  - Completely removes atmospheric fog

Examples:
  fullbright           - Toggle fullbright mode
  fullbright on        - Enable enhanced lighting and remove fog
  fullbright off       - Restore normal lighting and fog

Note: Useful for exploring dark areas, debugging lighting issues, or maximum visibility";
            case "maxhealth":
                return @"Command: maxhealth <health>
Description: Sets the player's maximum health
Usage: maxhealth <health>

Parameters:
  health        - Maximum health value to set

Examples:
  maxhealth 3          - Set maximum health to 3
  maxhealth 10         - Set maximum health to 10

Note: Also sets current health to match the new maximum";
            case "weapons":
                return @"Command: weapons
Description: Unlocks all weapons
Usage: weapons

Note: Unlocks all weapons";
            case "machinegun":
                return @"Command: machinegun <fireRate>
Description: Enables machine gun mode
Usage: machinegun <fireRate>

Parameters:
  fireRate      - Fire rate in shots per second (optional)

Examples:
  machinegun           - Enable machine gun mode
  machinegun 30        - Enable machine gun mode with 30 shots per second

Note: Enables machine gun mode";

            case "sausage":
                return @"Command: sausage <count>
Description: Adds sausages to the player's inventory
Usage: sausage <count>

Parameters:
  count         - Number of sausages to add (optional, default: 1)

Examples:
  sausage           - Add 1 sausage
  sausage 5         - Add 5 sausages

Note: Adds sausages to the player's current count and total count";
            case "destroy":
                return @"Command: destroy <objectName>
Description: Destroys an object by name or by raycasting to find the object the player is looking at
Usage: destroy <objectName>

Parameters:
  objectName    - Name of the object to destroy (optional)

Examples:
  destroy           - Destroy the object the player is looking at
  destroy sausage   - Destroy the object named 'sausage'

Note: Destroys the specified object or the object the player is looking at";
            case "wireframe":
                return @"Command: wireframe
Description: Toggles wireframe mode
Usage: wireframe

Note: Toggles wireframe mode";

            case "collision":
                return @"Command: collision
Description: Toggles collision visualization
Usage: collision

Note: Toggles collision visualization, recommended to use with wireframe mode";

            case "pointer":
            case "point":
                return @"Command: pointer [set|move|rotate]
Description: Creates and manages a debug pointer marker that persists between scenes
Usage:
  pointer                  - Shows pointer position and rotation
  pointer set              - Creates/moves pointer where player is looking
  pointer set <x> <y> <z> - Creates/moves pointer at coordinates
  pointer set <object>    - Creates/moves pointer at object's position
  pointer move <x|y|z> <amount> - Moves pointer by amount on axis
  pointer move <object>   - Moves pointer to object's position
  pointer move reset      - Resets pointer position to (0, 0, 0)
  pointer rotate <x|y|z> <degrees> - Rotates pointer by degrees on axis
  pointer rotate <object> - Rotates pointer to match object's rotation
  pointer rotate reset    - Resets pointer rotation to (0, 0, 0)

Shortcuts:
  point                    - Same as pointer
  pointer m <axis> <amount> - Same as pointer move
  pointer r <axis> <degrees> - Same as pointer rotate

Examples:
  pointer                  - Show current pointer info
  pointer set              - Create pointer where you're looking
  pointer set 10 5 3       - Move pointer to coordinates (10, 5, 3)
  pointer set door         - Move pointer to door object
  pointer move y 1         - Move pointer up by 1 unit
  pointer m x -2           - Move pointer left by 2 units
  pointer move Player      - Move pointer to Player's position
  pointer move reset       - Reset pointer position to origin
  pointer rotate y 45      - Rotate pointer 45 degrees around Y axis
  pointer r z 90           - Rotate pointer 90 degrees around Z axis
  pointer rotate door      - Rotate pointer to match door's rotation
  pointer rotate reset     - Reset pointer rotation to identity

Note: The pointer persists between scene changes and uses a sphere with two arrows (yellow for forward, cyan for up) to indicate orientation";

            default:
                return $"'{command}' is not a recognized command. Type 'help' to see available commands.";
        }
    }
}
