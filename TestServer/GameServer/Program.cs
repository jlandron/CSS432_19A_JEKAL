using System;

namespace GameServer {
    class Program {
        public static void Main( string[] args ) {
            Server.StartServer( );
            //console stays open
            Console.Read( );
        }
    }
}
