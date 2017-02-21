using UnityEngine;
using System.Collections;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp;
using System;

public class FaceDetection : MonoBehaviour {
    public int Width = 640;
    public int Height = 480;
    public int FPS = 30;

    public bool Mirror = false;

    public Camera Camera;
    Camera _Camera;

    CvCapture cvCapture;
    public int VideoIndex = 2;

    Texture2D texture;

    CascadeClassifier cascade;

    public GameObject Object;
    
    void Start() { 
        var devices = WebCamTexture.devices;
        for (int i = 0; i < devices.Length; i++) {
            print(string.Format("index {0}:{1}", i, devices[i].name));
        }

        cvCapture = CvCapture.FromCamera(0);

        print(string.Format("{0},{1}", Width, Height));
        
        cascade = new CascadeClassifier(Application.dataPath + @"/haarcascade_frontalface_alt.xml");
        
        texture = new Texture2D(Width, Height, TextureFormat.RGB24, false);
        GetComponent<Renderer>().material.mainTexture = texture;
        
        _Camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        print(string.Format("({0},{1})({2},{3})", Screen.width, Screen.height, _Camera.pixelWidth, _Camera.pixelHeight));
    }
    
    void Update() {
        using (Mat image = new Mat(cvCapture.QueryFrame())) {
            var faces = cascade.DetectMultiScale(image);
            if (faces.Length > 0) {
                var face = faces[0];
                
                image.Rectangle(face, new Scalar(255, 0, 0), 2);
                
                var x = face.TopLeft.X + (face.Size.Width / 2);
                var y = face.TopLeft.Y + (face.Size.Height / 2);
                
                if (Object != null) {
                    Object.transform.localPosition = Vector2ToVector3(new Vector2(x, y));
                }
            }
            
            texture.LoadImage(image.ImEncode(".png"));
            texture.Apply();
        }
    }

    void OnApplicationQuit() {
        if (cvCapture != null) {
            cvCapture.Dispose();
            cvCapture = null;
        }
    }
    
    private Vector3 Vector2ToVector3(Vector2 vector2) {
        if (Camera == null) {
            throw new Exception("");
        }
        
        vector2.x = vector2.x * Screen.width / Width;
        vector2.y = vector2.y * Screen.height / Height;
        
        var vector3 = _Camera.ScreenToWorldPoint(vector2);
        
        vector3.y *= -1;
        vector3.z = 0;

        return vector3;
    }
}