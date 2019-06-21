using Panacea.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Panacea;
using ServiceStack.Text;
using Panacea.Modularity.TerminalPairing;
using System.Net;
using System.Net.Sockets;
using Panacea.Modules.TerminalPairing;

namespace TestTerminalPairing
{
    class Program
    {
        static void Main(string[] args)
        {
            PanaceaSerializer serializer = new PanaceaSerializer();
            BoundTerminalManager MasterManager = CreateMaster(serializer);
            BoundTerminalManager SlaveManager = CreateSlave(serializer);
            MasterManager.connect();
            SlaveManager.connect();
            MasterManager.GetBoundTerminal().Connected += Master_Connected;
            SlaveManager.GetBoundTerminal().Connected += Slave_Connected;
            Console.ReadLine();
        }

        private static void Slave_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("Slave Connected!");
        }

        private static void Master_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("Master Connected!");
        }

        static BoundTerminalManager CreateMaster(ISerializer serializer)
        {
            return new BoundTerminalManager(serializer, new Panacea.Modularity.TerminalPairing.TerminalInfoResponse()
            {
                Relation = TerminalRelation.Master,
                Ip = "127.0.0.1",
                TerminalName = "MasterTerminal",
                BoundTerminal = new Terminal()
                {
                    Ip = "127.0.0.1",
                    MacAddresses = new List<string>() { "" },
                    Name = "SlaveTerminal"
                }
            });
        }
        static BoundTerminalManager CreateSlave(ISerializer serializer)
        {
            return new BoundTerminalManager(serializer, new Panacea.Modularity.TerminalPairing.TerminalInfoResponse()
            {
                Relation = TerminalRelation.Slave,
                Ip = "127.0.0.1",
                TerminalName = "SlaveTerminal",
                BoundTerminal = new Terminal()
                {
                    Ip = "127.0.0.1",
                    MacAddresses = new List<string>() { "" },
                    Name = "MasterTerminal"
                }
            });
        }
        public static string GetLocalIPAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                throw new Exception("No network available");
            }
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
    public class PanaceaSerializer : ISerializer
    {
        public PanaceaSerializer()
        {
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;
        }

        public T Deserialize<T>(string text)
        {
            return JsonSerializer.DeserializeFromString<T>(text);
        }

        public object Deserialize(string text, Type t)
        {
            return JsonSerializer.DeserializeFromString(text, t);
        }

        public string Serialize<T>(T obj)
        {
            return JsonSerializer.SerializeToString<T>(obj);
        }
    }
}
