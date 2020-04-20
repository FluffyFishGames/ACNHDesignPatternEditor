using MyHorizons.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QRCode : MonoBehaviour
{
    public RenderTexture ResultTexture;
    public RawImage TopLeftPreviewImage;
    public RawImage TopRightPreviewImage;
    public RawImage BottomLeftPreviewImage;
    public RawImage BottomRightPreviewImage;
    public RawImage TopLeftQRCodeImage;
    public RawImage TopRightQRCodeImage;
    public RawImage BottomLeftQRCodeImage;
    public RawImage BottomRightQRCodeImage;
    public RawImage BigLeftImage;
    public RawImage QRCodeImage;
    public TMPro.TextMeshProUGUI Text;
    public GameObject OneQR;
    public GameObject FourQR;
    public Previews Previews;
    public Camera Camera;

    private xBRZNet.xBRZScaler Scaler = new xBRZNet.xBRZScaler();

    private Texture2D RenderImage(RenderTexture tex)
    {
        RenderTexture.active = tex;

        var image = new Texture2D(RenderTexture.active.width, RenderTexture.active.height, UnityEngine.TextureFormat.ARGB32, false);
        image.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
        image.Apply();
        RenderTexture.active = null;
        return image;
    }

    public byte[] Render(DesignPattern pattern, TextureBitmap qrCode1, TextureBitmap qrCode2 = null, TextureBitmap qrCode3 = null, TextureBitmap qrCode4 = null)
    {
        var patternBitmap = pattern.GetBitmap();

        var scaledImage = new TextureBitmap(pattern.Width * 4, pattern.Height * 4);
        int[] src = patternBitmap.ConvertToInt();
        int[] target = new int[scaledImage.Width * scaledImage.Height];
        Scaler.ScaleImage(4, src, target, patternBitmap.Width, patternBitmap.Height, new xBRZNet.ScalerCfg(), 0, int.MaxValue);
        scaledImage.FromInt(target);
        scaledImage.Apply();
        
        var scaledTexture = scaledImage.Texture;
        var preview = Previews.AllPreviews[pattern.Type];
        preview.SetTexture(scaledTexture);
        preview.ResetPosition();
        Text.text = pattern.Name;
        if (qrCode2 != null)
        {
            OneQR.SetActive(false);
            FourQR.SetActive(true);

            preview.Camera.Render(); 
            var img1 = RenderImage(preview.Camera.targetTexture);
            TopLeftPreviewImage.texture = img1;
            var qr1 = qrCode1.Texture;
            qr1.filterMode = FilterMode.Point;
            TopLeftQRCodeImage.texture = qr1;

            preview.Move(90f, 0f);
            preview.Camera.Render();

            var img2 = RenderImage(preview.Camera.targetTexture);
            TopRightPreviewImage.texture = img2;
            var qr2 = qrCode2.Texture;
            qr2.filterMode = FilterMode.Point;
            TopRightQRCodeImage.texture = qr2;

            preview.Move(90f, 0f);
            preview.Camera.Render();

            var img3 = RenderImage(preview.Camera.targetTexture);
            BottomLeftPreviewImage.texture = img3;
            var qr3 = qrCode3.Texture;
            qr3.filterMode = FilterMode.Point;
            BottomLeftQRCodeImage.texture = qr3;

            preview.Move(90f, 0f);
            preview.Camera.Render();

            var img4 = RenderImage(preview.Camera.targetTexture);
            BottomRightPreviewImage.texture = img4;
            var qr4 = qrCode4.Texture;
            qr4.filterMode = FilterMode.Point;
            BottomRightQRCodeImage.texture = qr4;

            Camera.Render();
            
            GameObject.Destroy(img1);
            GameObject.Destroy(qr1);
            GameObject.Destroy(img2);
            GameObject.Destroy(qr2);
            GameObject.Destroy(img3);
            GameObject.Destroy(qr3);
            GameObject.Destroy(img4);
            GameObject.Destroy(qr4);
        }
        else
        {
            OneQR.SetActive(true);
            FourQR.SetActive(false);

            preview.ResetPosition();
            preview.Camera.Render(); 
            var img1 = RenderImage(preview.Camera.targetTexture);
            BigLeftImage.texture = img1;
            var qr1 = qrCode1.Texture;
            qr1.filterMode = FilterMode.Point;
            QRCodeImage.texture = qr1;

            Camera.Render();
            GameObject.DestroyImmediate(img1);
            GameObject.DestroyImmediate(qr1);
        }
        GameObject.DestroyImmediate(scaledTexture);
        scaledImage.Dispose();
        patternBitmap.Dispose();
        var result = RenderImage(Camera.targetTexture);
        return result.EncodeToPNG();
    }
}
