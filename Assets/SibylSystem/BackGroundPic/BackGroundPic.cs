using System.IO;
using UnityEngine;

public class BackGroundPic : Servant
{
    private GameObject backGround;

    public override void initialize()
    {
        backGround = Program.I().mod_simple_ngui_background_texture;
        var file = new FileStream("texture/common/desk.jpg", FileMode.Open, FileAccess.Read);
        file.Seek(0, SeekOrigin.Begin);
        var data = new byte[file.Length];
        file.Read(data, 0, (int) file.Length);
        file.Close();
        file.Dispose();
        file = null;
        var pic = new Texture2D(1920, 1080);
        pic.LoadImage(data);
        backGround.GetComponent<UITexture>().mainTexture = pic;
        backGround.GetComponent<UITexture>().depth = -100;
    }

    public override void applyShowArrangement()
    {
        var s = (float) Utils.UIHeight() / Screen.height;
        var tex = backGround.GetComponent<UITexture>().mainTexture;
        var ss = tex.height / (float) tex.width;
        var width = (int) (Screen.width * s);
        var height = (int) (width * ss);
        if (height < Screen.height)
        {
            height = (int) (Screen.height * s);
            width = (int) (height / ss);
        }

        backGround.GetComponent<UITexture>().height = height + 2;
        backGround.GetComponent<UITexture>().width = width + 2;
    }

    public override void applyHideArrangement()
    {
        applyShowArrangement();
    }
}