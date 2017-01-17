using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

public class OptitrackSimulator : MonoBehaviour {
    const int PORT = 8520;

    private Thread mainTread = null;
    private Rect buttonRect = new Rect(0, 0, 100, 20);

    private void Start() {

    }

    private void Update() {

    }

    private void OnApplicationQuit() {
        endServer();
    }

    private void OnGUI() {
        if (mainTread == null) {
            if (GUI.Button(buttonRect, "start optitrack")) {
                startServer();
            }
        } else {
            if (GUI.Button(buttonRect, "stop optitrack")) {
                endServer();
            }
        }
    }

    private void startServer() {
        string ipAddress = Network.player.ipAddress;
        //string ipAddress = "127.0.0.1";
        mainTread = new Thread(() => hostServer(ipAddress));
        mainTread.Start();
    }

    private void hostServer(string ipAddress) {
        IPAddress serverIP = IPAddress.Parse(ipAddress);
        TcpListener listener = new TcpListener(serverIP, PORT);

        listener.Start();
        while (mainTread != null) {
            if (listener.Pending()) {
                TcpClient client = listener.AcceptTcpClient();
                Thread thread = new Thread(() => msgThread(client));
                thread.Start();
            }
            Thread.Sleep(10);
        }
        listener.Stop();
    }

    private void endServer() {
        mainTread = null;
    }

    private void msgThread(TcpClient client) {
        StreamWriter sw = new StreamWriter(client.GetStream());

        float ry = 0;
        while (mainTread != null) {
            sw.WriteLine("begin");
            sw.WriteLine("rb 0 0 0 1 0 " + ry.ToString() + " 0");
            sw.WriteLine("end");
            sw.Flush();
            Thread.Sleep(10);
            ry += 1.0f;
        }
    }
}
