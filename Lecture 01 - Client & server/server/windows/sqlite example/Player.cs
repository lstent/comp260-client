//Based on the work of https://github.com/Phil-Sparkes/comp260-server-1617/blob/master/MUD2/Server/Server/Player.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public class Player
    {
        // set variable for player
        public Dungeon dungeonRef;
        public Room currentRoom;
        public String playerName;
        public String clientName;
        public List<String> Items = new List<string>();

        //when the client joins start them in the forrested area on the dungeon
        public void Init()
        {
            currentRoom = dungeonRef.roomMap["Forested Area"];
        }
    }
}
