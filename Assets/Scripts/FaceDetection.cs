using UnityEngine;
using System.Collections;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp;
using System;

public class FaceDetection : MonoBehaviour {
    static CascadeClassifier cascade;
    static OpenCvSharp.CPlusPlus.Rect lastFace;
    
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
            if (lastFace != new OpenCvSharp.CPlusPlus.Rect()) {
                double k = 0.5;
                face.X = (int)(face.X * k + lastFace.X * (1 - k));
                face.Y = (int)(face.Y * k + lastFace.Y * (1 - k));
                face.Width = (int)(face.Width * k + lastFace.Width * (1 - k));
                face.Height = (int)(face.Height * k + lastFace.Height * (1 - k));
            }
            x = face.X;
            y = face.Y;
            width = face.Width;
            height = face.Height;
            lastFace = face;
            return true;
        }
        if (lastFace != new OpenCvSharp.CPlusPlus.Rect()) {
            x = lastFace.X;
            y = lastFace.Y;
            width = lastFace.Width;
            height = lastFace.Height;
            return true;
        } else {
            x = 0;
            y = 0;
            width = 0;
            height = 0;
            return false;
        }
    }
}