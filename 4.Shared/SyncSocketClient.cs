using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using PT.Shared;

public class SyncSocketClient
{
    public delegate void ConnectDoneHandler(SyncSocketClient client, bool status);
    public ConnectDoneHandler ConnectDone;
    public delegate void SendCommandDoneHandler(SyncSocketClient client, bool status);
    public SendCommandDoneHandler SendCommandDone;
    public delegate void DataReceivedHandler(SyncSocketClient client, string data);
    public DataReceivedHandler DataReceived;

    public string HostIp { get; set; }
    public int Port { get; set; }
    public int MillisecondTimeout { get; set; }
    public bool KeepConnection { get; set; }
    public bool ActiveHealthCheck { get; set; }
    public int HealthCheckInterval { get; set; }
    public string IdentityKey { get; set; }

    private TcpClient _client;
    private static NetworkStream _clientStream;
    Timer _healthCheckTimer;
    private readonly byte[] _healthCheckPackage;
    private DateTime _lastConnection;
    private int _localPort;

    public SyncSocketClient(string hostIp, int port, int millisecondTimeout, bool keepConnection, bool activeHealthCheck, int healthCheckInterval, byte[] healthCheckPackage, string id)
    {
        this.HostIp = hostIp;
        this.Port = port;
        this.MillisecondTimeout = millisecondTimeout;
        this.KeepConnection = keepConnection;
        this.ActiveHealthCheck = activeHealthCheck;
        this.HealthCheckInterval = healthCheckInterval;
        _healthCheckPackage = healthCheckPackage;
    }

    public void Start(bool isReceiveData= false )
    {
        try
        {
            //_client = new TcpClient(new IPEndPoint(IPAddress.Parse(_clientIp), _clientPort));
            _client = new TcpClient();
            IPAddress ipAddress = IPAddress.Parse(HostIp);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, Port);
            _client = TimeOutSocket.Connect(remoteEP, MillisecondTimeout);
            _lastConnection = DateTime.Now;

            IPEndPoint localEP = (IPEndPoint)_client.Client.LocalEndPoint;
            _localPort = localEP.Port;

            if (this.ActiveHealthCheck)
            {
                StartTimer();
                // nhan phan hoi tu server
                try
                {
                    
                }
                catch
                {
                    // ignore
                }
            }
            if(isReceiveData)
            {
                Thread _thread = new Thread(ReceiveData);
                _thread.Start();
            }
        }
        catch (Exception)
        {
           // Utilities.WriteLog($"SyncSocketClient_Start_{HostIp}_{Port}", ex.ToString());
            _client?.Close();
            //throw ex;
        }
    }

    public void Stop()
    {
        try
        {
            if (this.ActiveHealthCheck)
                StopTimer();

            //_clientStream?.Close();
            _client?.Close();
        }
        catch (Exception)
        {
           // Utilities.WriteLog($"SyncSocketClient_Stop_{HostIp}_{Port}", ex.ToString());
            //throw ex;
        }
    }

    public void Restart()
    {
        try
        {
            // mat ket noi, khoi dong lai socket
            Stop();
            //Thread.Sleep(10);
            Start();
        }
        catch (Exception)
        {
           // Utilities.WriteLog($"SyncSocketClient_Restart_{HostIp}_{Port}", ex.ToString());
        }
    }

    public void StartTimer()
    {
        try
        {
            if (this.HealthCheckInterval > 0)
                _healthCheckTimer = new Timer(CheckConnection, null, this.HealthCheckInterval, this.HealthCheckInterval);
        }
        catch (Exception)
        {
          //  Utilities.WriteLog("SyncSocketClient_StartTimer", ex.ToString());
        }
    }

    public void StopTimer()
    {
        try
        {
            if (_healthCheckTimer != null)
            {
                _healthCheckTimer.Dispose();
                _healthCheckTimer = null;
            }
        }
        catch (Exception)
        {
           // Utilities.WriteLog("SyncSocketClient_StopTimer", ex.ToString());
        }
    }

    public bool Send(string data)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(data);
        return Send(buffer);
    }

    public bool Send(byte[] buffer)
    {
        try
        {
            try
            {
                if (_client == null || !_client.Connected)
                {
                 //   Utilities.WriteLog($"SyncSocketClient_{_localPort}_Restart_{HostIp}_{Port}", "Connection is disconnected, restarting...");
                    //Console.WriteLine($"SyncSocketClient_{_localPort}_Restart_{HostIp}_{Port}: Connection is disconnected, restarting...");
                    Restart();
                }

                _clientStream = _client.GetStream();
                _clientStream.Write(buffer, 0, buffer.Length);
                _lastConnection = DateTime.Now;
                if (buffer == _healthCheckPackage)
                {
                   // Utilities.WriteLog($"Clien_{_localPort}t_HealthCheck_Response_{HostIp}_{Port}: ", Encoding.UTF8.GetString(buffer) + " - " + Utilities.GetBitStr(buffer));
                    //Console.WriteLine($"Client_{_localPort}_HealthCheck_Response_{HostIp}_{Port}: {Encoding.UTF8.GetString(buffer)} - {Utilities.GetBitStr(buffer)}");
                }
                else
                {
                 //   Utilities.WriteLog($"Client_{_localPort}_Sent_{HostIp}_{Port}: ", Encoding.UTF8.GetString(buffer) + " - " + Utilities.GetBitStr(buffer));
                    //Console.WriteLine($"Client_{_localPort}_Sent_{HostIp}_{Port}: {Encoding.UTF8.GetString(buffer)} - {Utilities.GetBitStr(buffer)}");
                }
            }
            catch
            {
                //Utilities.WriteLog($"SyncSocketClient_Restart_{HostIp}_{Port}", "Failed to send, restarting...");
                //Console.WriteLine($"SyncSocketClient_Restart_{HostIp}_{Port}: Failed to send, restarting...");
                Restart();

                if (_client != null && _client.Connected)
                {
                    _clientStream = _client.GetStream();
                    _clientStream.Write(buffer, 0, buffer.Length);
                    _lastConnection = DateTime.Now;
                    if (buffer == _healthCheckPackage)
                    {
                   //     Utilities.WriteLog($"Client_{_localPort}_HealthCheck_Response_{HostIp}_{Port}: ", Encoding.UTF8.GetString(buffer) + " - " + Utilities.GetBitStr(buffer));
                        //Console.WriteLine($"Client_{_localPort}_HealthCheck_Response_{HostIp}_{Port}: {Encoding.UTF8.GetString(buffer)} - {Utilities.GetBitStr(buffer)}");
                    }
                    else
                    {
                     //   Utilities.WriteLog($"Client_{_localPort}_Sent_{HostIp}_{Port}: ", Encoding.UTF8.GetString(buffer) + " - " + Utilities.GetBitStr(buffer));
                        //Console.WriteLine($"Client_{_localPort}_Sent_{HostIp}_{Port}: {Encoding.UTF8.GetString(buffer)} - {Utilities.GetBitStr(buffer)}");
                    }
                }
            }
            finally
            {
                if (!this.KeepConnection)
                {
                    try
                    {
                        //_clientStream?.Close();
                        _client?.Close();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            return true;
        }
        catch (Exception)
        {
            string data = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            //Utilities.WriteLog($"SyncSocketClient_Send_{HostIp}_{Port}_{data}", ex.ToString());

            return false;
        }
    }

    private void ReceiveData()
    {
        try
        {
            if (_client != null && _client.Connected)
            {
                _clientStream = _client.GetStream();

                while (true)
                {
                    try
                    {
                        int bytesRead = 0;
                        byte[] response = new byte[_client.ReceiveBufferSize];

                        try
                        {
                            bytesRead = _clientStream.Read(response, 0, response.Length);
                        }
                        catch
                        {
                            break;
                        }

                        if (bytesRead > 0)
                        {
                            byte[] data = new byte[bytesRead];
                            Array.Copy(response, data, bytesRead);
                        //    Utilities.WriteLog($"Client_{_localPort}_Received_{HostIp}_{Port}: ", Encoding.UTF8.GetString(data) + " - " + Utilities.GetBitStr(data));
                            Console.WriteLine($"Client_{_localPort}_Received_{HostIp}_{Port}: {Encoding.UTF8.GetString(data)} - {Functions.GetBitStr(data)}");
                            string msg = Unicode2TCVN3.ToUnicode(data);
                            // receive health check message, response to server
                            _lastConnection = DateTime.Now;
                            //Console.WriteLine($"Client_{_localPort}_LastConnection: {_lastConnection}");
                            //Send(_healthCheckPackage);
                            Console.WriteLine(msg);
                        }
                        else
                        {
                            // The connection has closed gracefully, so stop the
                            // thread.
                            break;
                        }
                    }
                    catch
                    {
                        // Handle the exception...
                        break;
                        //Utilities.WriteLog("SyncSocketClient_ReceiveData_Loop", ex1.ToString());
                    }
                }
            }
        }
        catch (Exception)
        {
           // Utilities.WriteLog($"SyncSocketClient_ReceiveData_{HostIp}_{Port}", ex.ToString());
        }
        finally
        {
            // mat ket noi, restart socket
        }
    }

    private void CheckConnection(Object stateInfo)
    {
        if (!this.ActiveHealthCheck)
            return;

        try
        {
            if (_lastConnection.AddMilliseconds(this.HealthCheckInterval * 2) < DateTime.Now)
            {
                // mat ket noi qua thoi gian cho phep, khoi dong lai socket
               // Utilities.WriteLog($"SyncSocketClient_{_localPort}_Restart_{HostIp}_{Port}", $"Last Connection: {_lastConnection} - {DateTime.Now}. Connection is inactive too long, restarting...");
                //Console.WriteLine($"SyncSocketClient_{_localPort}_Restart_{HostIp}_{Port}: Last Connection: {_lastConnection} - {DateTime.Now}. Connection is inactive too long, restarting...");
                Restart();
                return;
            }
        }
        catch (Exception)
        {
           // Utilities.WriteLog($"SyncSocketClient_CheckConnection_{HostIp}_{Port}", ex.ToString());
        }
    }
}
