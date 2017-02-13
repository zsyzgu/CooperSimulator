using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System;
using System.Text;

public class CaptureSimulator : MonoBehaviour {
    const int PORT = 8888;
    private Thread mainThread = null;

    public int captureID = 0;
    private WebCamTexture webCamTexture;
    private byte[] imageData;
    private bool imageDataLock = false;

    void OnApplicationQuit() {
        endServer();
    }

    void Start() {
        webCamTexture = new WebCamTexture();
        webCamTexture.Play();
    }

    void Update() {
        if (mainThread != null) {
            Texture2D texture = new Texture2D(webCamTexture.width, webCamTexture.height);
            texture.SetPixels(webCamTexture.GetPixels());
            while (imageDataLock) {
                Thread.Sleep(1);
            }
            imageData = texture.EncodeToJPG();
            Destroy(texture);
        }
    }

    void OnGUI() {
        GUI.color = Color.gray;
        GUI.TextArea(new Rect(0, 50, 200, 50), Network.player.ipAddress);
        GUI.color = Color.white;
        if (mainThread == null) {
            if (GUI.Button(new Rect(200, 50, 200, 50), "start capture server")) {
                startServer();
            }
        } else {
            if (GUI.Button(new Rect(200, 50, 200, 50), "end capture server")) {
                endServer();
            }
        }
    }

    private void startServer() {
        imageDataLock = false;
        string ipAddress = Network.player.ipAddress;
        mainThread = new Thread(() => hostServer(ipAddress));
        mainThread.Start();
    }

    private void hostServer(string ipAddress) {
        IPAddress serverIP = IPAddress.Parse(ipAddress);
        TcpListener listener = new TcpListener(serverIP, PORT);

        listener.Start();
        while (mainThread != null) {
            if (listener.Pending()) {
                TcpClient client = listener.AcceptTcpClient();
                Thread thread = new Thread(() => msgThread(client));
                thread.Start();
            }
            Thread.Sleep(10);
        }
        listener.Stop();
    }

    private void msgThread(TcpClient client) {
        Stream sr = new StreamReader(client.GetStream()).BaseStream;
        Stream sw = new StreamWriter(client.GetStream()).BaseStream;

        while (mainThread != null) {
            if (imageData != null) {
                try {
                    byte[] info = new byte[4];
                    imageDataLock = true;
                    int len = imageData.Length;
                    info[0] = (byte)captureID;
                    info[1] = (byte)((len & 0xff0000) >> 16);
                    info[2] = (byte)((len & 0xff00) >> 8);
                    info[3] = (byte)(len & 0xff);
                    sw.Write(info, 0, 4);
                    sw.Write(imageData, 0, len);
                    imageDataLock = false;
                    sw.Flush();
                    imageData = null;
                    sr.ReadByte();
                } catch {
                    break;
                }
            }
        }

        client.Close();
    }

    private void endServer() {
        mainThread = null;
    }
}
