using UnityEngine;
using System.Net.Sockets;
using System.Threading;

public class CaptureSimulator : MonoBehaviour {
    const int PORT = 8888;
    private Thread mainThread = null;

    public int captureID = 0;
    private WebCamTexture webCamTexture;
    private byte[] imageData;
    private string serverIP;

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
    }

    private void startClient() {
        mainThread = new Thread(hostClient);
        mainThread.Start();
    }

    private void hostClient() {
        TcpClient client = new TcpClient();
        client.Connect(serverIP, PORT);
        NetworkStream networkStream = client.GetStream();

        while (mainThread != null) {
            if (imageData != null) {
                networkStream.Write(imageData, 0, imageData.Length);
                networkStream.Flush();
                imageData = null;
            }
            Thread.Sleep(10);
        }
    }

    private void endClient() {
        mainThread = null;
    }
}
