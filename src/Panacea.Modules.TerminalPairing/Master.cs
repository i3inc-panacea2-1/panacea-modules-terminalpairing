using Panacea.Core;
using Panacea.Modularity.TerminalPairing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.TerminalPairing
{
    public class Master : BoundTerminal
    {
        public override TerminalRelation Relation => TerminalRelation.Master;
        private readonly TcpListener _listener;

        public Master(string slaveName, string ip, ISerializer serializer) : base(slaveName, ip, serializer)
        {
            _listener = new TcpListener(IPAddress.Any, 9010);
            _listener.Start();
        }

        private bool _waiting;

        public override async Task TryConnect()
        {
            if (_waiting) return;
            _waiting = true;

            await AcceptClient();

        }
        private async Task AcceptClient()
        {
            try
            {

                Client = await _listener.AcceptTcpClientAsync();

                var ip = (Client.Client.RemoteEndPoint as IPEndPoint).Address.ToString();
                Console.WriteLine("Slave conncted");
                _waiting = false;
                if (ip.Equals(Ip))
                {
                    PrepareTcpClient();
                }
                else
                {
                    CloseClient();

                    await TryConnect();
                }
            }
            catch
            {
                _waiting = false;
                await TryConnect();
            }
            finally
            {

            }


        }
    }
}
