using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;

public class CaptureSimulator : MonoBehaviour {
    const int PORT = 8888;
    private Thread mainThread = null;

    public int captureID = 0;
    private WebCamTexture webCamTexture;
    private byte[] imageData;
    private string serverIP;
    private int sleepTime = 50;

    void OnApplicationQuit() {
        endClient();
    }

    void Start() {
        webCamTexture = new WebCamTexture();
        webCamTexture.Play();
        serverIP = Network.player.ipAddress;
    }

    void Update() {
        if (mainThread != null) {
            Texture2D texture = new Texture2D(webCamTexture.width, webCamTexture.height);
            texture.SetPixels(webCamTexture.GetPixels());
            imageData = texture.EncodeToJPG();
            Destroy(texture);
        }
    }

    void OnGUI() {
        if (mainThread == null) {
            GUI.color = Color.white;
            serverIP = GUI.TextArea(new Rect(0, 50, 200, 50), serverIP);
            if (GUI.Button(new Rect(200, 50, 200, 50), "connect to teacher")) {
                startClient();
            }
        } else {
            GUI.color = Color.gray;
            GUI.TextArea(new Rect(0, 50, 200, 50), serverIP);
            GUI.color = Color.white;
            if (GUI.Button(new Rect(200, 50, 200, 50), "disconnect")) {
                endClient();
            }
        }
        GUI.color = Color.white;
        GUI.Label(new Rect(0, 100, 50, 20), "sleep time:");
        sleepTime = int.Parse(GUI.TextArea(new Rect(50, 100, 50, 20), sleepTime.ToString()));
    }

    private void startClient() {
        mainThread = new Thread(hostClient);
        mainThread.Start();
    }

    private void hostClient() {
        TcpClient client = new TcpClient();
        client.Connect(serverIP, PORT);
        Stream sr = new StreamReader(client.GetStream()).BaseStream;
        Stream sw = new StreamWriter(client.GetStream()).BaseStream;

        while (mainThread != null) {
            if (imageData != null) {
                try {
                    byte[] info = new byte[4];
                    int len = imageData.Length;
                    info[0] = (byte)captureID;
                    info[1] = (byte)((len & 0xff0000) >> 16);
                    info[2] = (byte)((len & 0xff00) >> 8);
                    info[3] = (byte)(len & 0xff);
                    sw.Write(info, 0, 4);
                    sw.Write(imageData, 0, len);
                    sw.Flush();
                    imageData = null;
                    sr.ReadByte();
                } catch {
                    break;
                }
            }
            Thread.Sleep(sleepTime);
        }
        
        client.Close();
    }

    private void endClient() {
        mainThread = null;
    }
}
