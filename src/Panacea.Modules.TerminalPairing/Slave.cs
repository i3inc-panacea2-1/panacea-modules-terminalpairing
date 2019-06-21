using Panacea.Core;
using Panacea.Modularity.TerminalPairing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.TerminalPairing
{
    public class Slave : BoundTerminal
    {

        public Slave(string masterName,string ip, ISerializer serializer)
            : base(masterName, ip, serializer)
        {

        }

        public override TerminalRelation Relation => TerminalRelation.Slave;

        private bool _connecting = false;
        private int _interval = 1000;

        public override async Task TryConnect()
        {
            lock (this)
            {
                if (_connecting) return;
                _connecting = true;
            }
            if (Client == null)
            {
                Client = new TcpClient();
            }
            await Task.Run(async () =>
            {
                while (!Client.Connected)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("Trying to connect to master");

                        var result = Client.BeginConnect(Ip, 9010, null, null);

                        var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                        if (!success) throw new Exception("Unable to connect to master");
                        PrepareTcpClient();
                        _interval = 1000;
                        System.Diagnostics.Debug.WriteLine("Connected to master");
                        _connecting = false;
                        break;
                    }
                    catch
                    {
                        Client.Dispose();
                        Client = new TcpClient();
                    }
                    await Task.Delay(_interval);
                    if (_interval < 6000) _interval += 100;
                }
            });
        }
    }
}
