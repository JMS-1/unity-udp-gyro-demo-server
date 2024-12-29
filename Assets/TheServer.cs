using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;


public class GyroData
{
    public float X;

    public float Y;

    public float Z;
}

public class TheServer : MonoBehaviour
{
    public int Port = 30402;

    private Label _LastMessage;

    private string _LastReceived;

    private Thread _Receiver;

    // Start is called before the first frame update
    void Start()
    {
        UIDocument doc = GetComponent<UIDocument>();

        Label ipLabel = (Label)doc.rootVisualElement.Q("TheIP");

        ipLabel.text = string.Format("{0}:{1}", GetIP(), Port);

        _LastMessage = (Label)doc.rootVisualElement.Q("TheData");
        _LastReceived = _LastMessage.text;

        _Receiver = new(ReceiveData) { IsBackground = true };
        _Receiver.Start();
    }

    // Update is called once per frame
    void Update()
    {
        _LastMessage.text = _LastReceived;
    }

    private static string GetIP()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();

        return string.Empty;
    }

    private void ReceiveData()
    {
        for (var client = new UdpClient(Port); ;)
            try
            {
                var anyIP = new IPEndPoint(IPAddress.Any, 0);
                var json = Encoding.UTF8.GetString(client.Receive(ref anyIP));
                var gyro = JsonUtility.FromJson<GyroData>(json);

                _LastReceived = $"{Math.Round(gyro.X * 100)},{Math.Round(gyro.Y * 100)},{Math.Round(gyro.Z * 100)}";
            }
            catch (ThreadAbortException)
            {
                break;
            }
            catch (Exception err)
            {
                Debug.LogError(err.ToString());
            }
    }

    void OnDestroy()
    {
        _Receiver?.Abort();
    }
}
