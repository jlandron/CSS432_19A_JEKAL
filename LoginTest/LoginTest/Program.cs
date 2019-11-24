using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LoginTest
{
    class Program
    {
        static ByteBuffer SendLogin(string playerName, bool badMessage = false)
        {
            var loginMsg = new ByteBuffer();

            // Create LOGIN message
            if (!badMessage)
            {
                loginMsg.Write(1);
            }
            else
            {
                loginMsg.Write(3);
            }
            loginMsg.Write(playerName);

            // Create and login with client
            string serverName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(serverName);
            IPAddress ipAddress = Array.FindAll(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork)[0];
            var client = new TcpClient();
            client.Connect(ipAddress.ToString(), 4444);
            var netStream = client.GetStream();
            netStream.Write(loginMsg.ToArray(), 0, loginMsg.Count());
            
            loginMsg.Clear();

            // Receive return
            byte[] inBuffer;
            inBuffer = new byte[1024];

            do
            {
                int bytesRead = netStream.Read(inBuffer, 0, inBuffer.Length);
                byte[] temp = new byte[bytesRead];
                Array.Copy(inBuffer, temp, bytesRead);
                loginMsg.Write(temp);
            }
            while (netStream.DataAvailable);

            // Close stream and socket
            netStream.Close();
            client.Close();
            return loginMsg;
        }

        static void DisplayBuffer(ByteBuffer buffer)
        {
            int loginResponse = buffer.ReadInt();
            Console.WriteLine($"Login Message: {loginResponse}");
            if (loginResponse == 2)
            {
                Console.WriteLine($"Chat IP: {buffer.ReadString()}");
                Console.WriteLine($"Chat Port: {buffer.ReadInt()}");
                Console.WriteLine($"Game IP: {buffer.ReadString()}");
                Console.WriteLine($"Game Port: {buffer.ReadInt()}");
                Console.WriteLine($"Session ID: {buffer.ReadInt()}");
            }
            else if (loginResponse == 3)
            {
                Console.WriteLine($"REJECT: {buffer.ReadString()}");
            }
        }

        static void Main(string[] args)
        {
            int menuChoice = 0;

            while (menuChoice != 4)
            {
                Console.WriteLine("1. Send LOGIN Thaldin");
                Console.WriteLine("2. Send LOGIN Talshir");
                Console.WriteLine("3. Send bad message.");
                Console.WriteLine("4. End client");
                Console.WriteLine("");
                menuChoice = Convert.ToInt32(Console.ReadLine());

                ByteBuffer buffer;
                switch (menuChoice)
                {
                   
                    case 1:
                        buffer = SendLogin("Thaldin");
                        DisplayBuffer(buffer);
                        break;
                    case 2:
                        buffer = SendLogin("Talshir");
                        DisplayBuffer(buffer);
                        break;
                    case 3:
                        SendLogin("Talshir", true);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
