using Panacea.Core;
using Panacea.Modularity.TerminalPairing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.TerminalPairing
{
    public class BoundTerminalManager : IBoundTerminalManager
    {
        private TerminalInfoResponse TerminalInfo;
        private BoundTerminal BoundTerminal;
        private ISerializer _serializer;
        public BoundTerminalManager(ISerializer serializer, TerminalInfoResponse terminalInfo)
        {
            this._serializer = serializer;
            this.TerminalInfo = terminalInfo;
        }
        public IBoundTerminal GetBoundTerminal()
        {
            return BoundTerminal;
        }

        public bool IsBound()
        {
            return BoundTerminal != null;
        }
        public async void connect()
        {

            if (TerminalInfo.Terminal != null)
            {
                if (TerminalInfo.Relation == TerminalRelation.Master)
                {
                    BoundTerminal = new Master(TerminalInfo.Terminal.Name, TerminalInfo.Terminal.Ip, _serializer);
                }
                else
                {
                    BoundTerminal = new Slave(TerminalInfo.Terminal.Name, TerminalInfo.Terminal.Ip, _serializer);
                }
                await BoundTerminal.TryConnect();
            }
        }
    }
}
