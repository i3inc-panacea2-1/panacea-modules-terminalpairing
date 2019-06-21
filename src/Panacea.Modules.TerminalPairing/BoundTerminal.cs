using Panacea.Core;
using Panacea.Modularity.TerminalPairing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace Panacea.Modules.TerminalPairing
{
    public abstract class BoundTerminal : IBoundTerminal
    {
        public event EventHandler Disconnected;
        protected TcpClient Client;
        protected StreamReader ClientReader;
        protected StreamWriter ClientWriter;
        private readonly Dictionary<string, List<CallbackPair>> _actions;
        private readonly Timer _timer;
        private readonly ISerializer _serializer;
        public string HostName { get; protected set; }
        public string Ip { get; protected set; }

        protected BoundTerminal(string host, string ip, ISerializer serializer) : this()
        {
            HostName = host;
            Ip = ip;
            _serializer = serializer;
            _timer = new Timer(3300);
            _timer.Elapsed += _timer_Elapsed;

        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Ping();
        }

        public bool IsConnected
        {
            get
            {
                try
                {
                    if (Client == null) return false;
                    return Client.Connected;
                }
                catch
                {
                    return false;
                }
            }
        }

        public abstract TerminalRelation Relation { get; }

        public abstract Task TryConnect();

        protected BoundTerminal()
        {
            _actions = new Dictionary<string, List<CallbackPair>>();
        }

        public void On<T>(string action, Action<T> callback)
        {
            if (!_actions.ContainsKey(action)) _actions.Add(action, new List<CallbackPair>());
            _actions[action].Add(new CallbackPair() { Action = (o) => callback((T)o), Callback = callback, Type = typeof(T) });
        }

        public void Off<T>(string action, Action<T> callback)
        {
            if (!_actions.ContainsKey(action)) return;
            var c = _actions[action].FirstOrDefault(a => a.Callback == callback);
            if (c == null) return;
            _actions[action].Remove(c);
        }

        private class CallbackPair
        {
            public Type Type { get; set; }
            public Action<object> Action { get; set; }
            public object Callback { get; set; }
        }

        protected void Ping()
        {
            if (ClientWriter == null) return;
            try
            {
                lock (ClientWriter)
                {
                    ClientWriter.WriteLine(
                       _serializer.Serialize<object>(new CommunicationMessage<object>() { Action = "ping" }));
                    ClientWriter.Flush();
                }
            }
            catch
            {

            }
        }

        public void Send<T>(string action, T obj)
        {
            if (ClientWriter == null) return;
            try
            {
                lock (ClientWriter)
                {
                    ClientWriter.WriteLine(
                        _serializer.Serialize<object>(new CommunicationMessage<T>() { Action = action, Object = obj }));
                    ClientWriter.Flush();
                }
            }
            catch
            {

            }
        }

        protected void CloseClient()
        {
            try
            {
                Client?.Close();
            }
            finally
            {
                ClientWriter?.Dispose();
                ClientWriter = null;
                ClientReader?.Dispose();
                ClientReader = null;
                Client?.Dispose();
                Client = null;
            }
        }

        public event EventHandler Connected;

        protected void PrepareTcpClient()
        {
            Connected?.Invoke(this, null);
            ClientWriter = new StreamWriter(Client.GetStream()) { AutoFlush = true };
            ClientReader = new StreamReader(Client.GetStream());
            Client.NoDelay = true;
            Client.ReceiveTimeout = 7000;
            Client.SendTimeout = 7000;
            _timer.Start();
            Task.Run(async () =>
            {
                while (true)
                {
                    string str = null;
                    try
                    {
                        str = ClientReader.ReadLine();
                    }
                    catch (Exception ex)
                    {
                        _timer.Stop();
                        Disconnected?.Invoke(this, null);
                        CloseClient();
                        await Task.Delay(2000);
                        await TryConnect();
                        return;
                    }
                    try
                    {
                        if (str == null) continue;
                        var obj = _serializer.Deserialize<CommunicationMessage>(str);
                        if (obj == null) return;
                        if (obj.Action == "ping") continue;
                        if (string.IsNullOrEmpty(obj.Action)) continue;
                        if (!_actions.ContainsKey(obj.Action)) continue;
                        var action = _actions[obj.Action].ToList();

                        foreach (var pair in action)
                        {
                            var message = typeof(CommunicationMessage<>).MakeGenericType(pair.Type);
                            dynamic act = _serializer.Deserialize(str, message);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                pair.Action(act.Object);
                            });

                        }
                    }
                    catch
                    {
                    }
                }
            });
        }
    }
}
