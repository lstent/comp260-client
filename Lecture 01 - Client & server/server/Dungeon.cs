//based on the work of https://github.com/Phil-Sparkes/comp260-server-1617/blob/master/MUD2/Server/Server/Dungeon.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Server
{
    public class Dungeon
    {
        public Dictionary<String, Room> roomMap;

        Room currentRoom;

        public void Init()
        {
            roomMap = new Dictionary<string, Room>();
            {
                var room = new Room("Forested Area", "You awake in a forested area, you don't know how you got there... ");
                room.north = "Well Clearing";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Well Clearing", "You arrive in a clearing with a well in the middle, you can hear a faint whispering... ");
                room.south = "Forested Area";
                room.west = "Cow Field";
                room.east = "Old House";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Old House", " You come across a strange old house with a thatch roof, the whispering has turned into a loud mumbleing... ");
                room.north = "Back Garden";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Cow Field", "You've just stumbled into a shit covered cow field and fallen on your face. !!!You died of dysentery!!!");
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Back Garden", "Your round the back of the old house the mumbling has turned into a cry coming from the spider covered shed to the west... ");
                room.south = "Old House";
                room.west = "Ye Olde Lavatory";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Ye Olde Lavatory", "You burst open the door it's an old toilet, a ghost comes up behind you and pushed you in. !!!You died of dysentery!!! ");
                roomMap.Add(room.name, room);
            }

            currentRoom = roomMap["Forested Area"];
        }


        public String GiveInfo(Player player)
        {
            currentRoom = player.currentRoom;
            String info = "";
            info += currentRoom.desc;

            for (var i = 0; i < currentRoom.exits.Length; i++)
            {
                if (currentRoom.exits[i] != null)
                {
                    info += ( Room.exitNames[i] + " There is a way through  ");
                }
            }

            info += "\nPlayers in room: ";

            foreach (Player otherPlayer in program.PlayerList)
            {
                if (player.currentRoom == otherPlayer.currentRoom)
                {
                    info += "[";
                    info += otherPlayer.playerName;
                    info += "]";
                }
            }
            info += "\n";
            return info;
        }

        public string Process(string Key,Player player)
        {
            currentRoom = player.currentRoom;
             String returnString = ""; GiveInfo(player);
             var input = Key.Split(' ');

            switch (input[0].ToLower())
            {
                case "help":
                    returnString += "\nCommands are ....\n";
                    returnString += "help - for this screen\n";
                    returnString += "look - to look around\n";
                    returnString += "go [north | south | east | west]  - to travel between locations\n";
                    returnString += "say - local chat\n";
                    returnString += "\nPress any key to continue\n";
                    //Console.ReadKey(true);
                    //break;

                    return returnString;

                case "look":
                    //loop straight back
                    //Console.Clear();
                    Thread.Sleep(1000);
                    returnString += ("\n" + currentRoom.desc);
                    returnString += ("\nExits");
                    for (var i = 0; i < currentRoom.exits.Length; i++)
                    {
                        if (currentRoom.exits[i] != null)
                        {
                            returnString += (" " + Room.exitNames[i] + " ");
                        }
                    }
                    return returnString;



                case "say":
                    returnString += (player.playerName);
                    returnString += ("] ");
                    for (var i = 1; i < input.Length; i++)
                    {
                        returnString += (input[i] + " ");
                    }

                    Thread.Sleep(1000);
                    //Console.Clear();
                    returnString += ("\n" + currentRoom.desc);
                    returnString += ("\nExits");
                    for (var i = 0; i < currentRoom.exits.Length; i++)
                    {
                        if (currentRoom.exits[i] != null)
                        {
                            returnString += (" " + Room.exitNames[i] + " ");
                        }
                    }
                    return returnString;

                case "go":
                    String direction = "";

                    if (input.Length > 1)
                    {
                        direction = input[1].ToLower();
                    }

                    if ((direction == "north") && (currentRoom.north != null))
                    {
                        player.currentRoom = roomMap[currentRoom.north];
                        returnString += GiveInfo(player);
                    }
                    else
                    {
                        if ((direction == "south") && (currentRoom.south != null))
                        {
                            player.currentRoom = roomMap[currentRoom.south];
                            returnString += GiveInfo(player);
                        }
                        else
                        {
                            if ((direction == "east") && (currentRoom.east != null))
                            {
                                player.currentRoom = roomMap[currentRoom.east];
                                returnString += GiveInfo(player);
                            }
                            else
                            {
                                if ((direction == "west") && (currentRoom.west != null))
                                {
                                    player.currentRoom = roomMap[currentRoom.west];
                                    returnString += GiveInfo(player);
                                }
                                else
                                {
                                    //handle error
                                    returnString += GiveInfo(player);
                                    returnString += ("\nERROR");
                                    returnString += ("\nCan not go " + input[1] + " from here");
                                    returnString += ("\nPress any key to continue");
                                    //Console.ReadKey(true);
                                }
                            }
                        }
                    }

                    return returnString;

                default:
                    //handle error
                    returnString += ("\nERROR");
                    returnString += ("\nCan not " + Key);
                    returnString += ("\nPress any key to continue");
                    //Console.ReadKey(true);
                    return returnString;
            }

        }
    }
}
