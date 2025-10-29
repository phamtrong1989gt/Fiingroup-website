using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Net;

public class TimeOutSocket
{
    private static bool _isConnectionSuccessful = false;
    private static Exception _socketException;
    //TODO: Check if readonly works
    private static readonly ManualResetEvent TimeoutObject = new ManualResetEvent(false);

    public static TcpClient Connect(IPEndPoint remoteEndPoint, int timeoutMSec)
    {
        TimeoutObject.Reset();
        _socketException = null;

        string serverip = Convert.ToString(remoteEndPoint.Address);
        int serverport = remoteEndPoint.Port;
        TcpClient tcpclient = new TcpClient();

        tcpclient.BeginConnect(serverip, serverport, new AsyncCallback(CallBackMethod), tcpclient);

        if (TimeoutObject.WaitOne(timeoutMSec, false))
        {
            if (_isConnectionSuccessful)
            {
                return tcpclient;
            }
            else
            {
                throw _socketException;
            }
        }
        else
        {
            tcpclient.Close();
            throw new TimeoutException("TimeOut Exception");
        }
    }
    private static void CallBackMethod(IAsyncResult asyncresult)
    {
        try
        {
            _isConnectionSuccessful = false;
            TcpClient tcpclient = asyncresult.AsyncState as TcpClient;

            if (tcpclient.Client != null)
            {
                tcpclient.EndConnect(asyncresult);
                _isConnectionSuccessful = true;
            }
        }
        catch (Exception ex)
        {
            _isConnectionSuccessful = false;
            _socketException = ex;
        }
        finally
        {
            TimeoutObject.Set();
        }
    }
}
