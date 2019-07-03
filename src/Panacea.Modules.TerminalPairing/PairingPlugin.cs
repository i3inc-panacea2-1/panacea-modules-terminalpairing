using Panacea.Core;
using Panacea.Modularity.TerminalPairing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.TerminalPairing
{
    public class PairingPlugin : IPairingPlugin
    {
        BoundTerminalManager BoundTerminalManager;
        private readonly PanaceaServices _core;

        public PairingPlugin(PanaceaServices core)
        {
            this._core = core;
        }
        public IBoundTerminalManager GetBoundTerminalManager()
        {
            return BoundTerminalManager;
        }
        public async Task BeginInit()
        {
            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var success = false;
            while (!success)
            {
                try
                {
                    TerminalInfoResponse terminalInfo = await GetSettingsAsync();
                    BoundTerminalManager = new BoundTerminalManager(this._core.Serializer, terminalInfo);
                    BoundTerminalManager.Connect();
                    success = true;
                }
                catch (Exception ex)
                {
                    _core.Logger.Error(this, ex.Message);
                    await Task.Delay(5000);
                }
            }
        }

        public Task EndInit()
        {
            return Task.CompletedTask;
        }
        public void Dispose()
        {
            return;
        }

        public Task Shutdown()
        {
            return Task.CompletedTask;
        }
        internal async Task<TerminalInfoResponse> GetSettingsAsync()
        {
            try
            {
                var terminalInfoTask = await _core.HttpClient.GetObjectAsync<TerminalInfoResponse>("get_terminal_info/");
                if (terminalInfoTask.Success)
                {
                    var TerminalInfo = terminalInfoTask.Result;
                    if (TerminalInfo.Ip != null)
                    {
                        //Utils.StartupArgs["Ip"] = TerminalInfo.Ip;
                    }
                    return TerminalInfo;
                }
                else
                {
                    throw new Exception("(get terminal info) Not registered");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("(get terminal info) Uncaught exception: " + ex.Message, ex);
            }
        }
    }
}
