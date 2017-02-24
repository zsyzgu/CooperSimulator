using UnityEngine;
using System.Collections;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp;
using System;

public class FaceDetection : MonoBehaviour {
    static CascadeClassifier cascade;
    
    void Awake() {
        cascade = new CascadeClassifier(Application.dataPath + @"/haarcascade_frontalface_alt.xml");
    }

    public static bool faceDetect(Texture2D texture, out int x, out int y, out int height, out int width) {
        Mat image = Mat.FromImageData(texture.EncodeToJPG());
        var faces = cascade.DetectMultiScale(image);
        if (faces.Length > 0) {
            int id = 0;
            int maxArea = 0;
            for (int i = 0; i < faces.Length; i++) {
                if (faces[i].Height * faces[i].Width > maxArea) {
                    id = i;
                    maxArea = faces[i].Height * faces[i].Width;
                }
            }
            var face = faces[id];
            x = face.X;
            y = face.Y;
            width = face.Width;
            height = face.Height;
            return true;
        }
        x = 0;
        y = 0;
        width = 0;
        height = 0;
        return false;
    }
}