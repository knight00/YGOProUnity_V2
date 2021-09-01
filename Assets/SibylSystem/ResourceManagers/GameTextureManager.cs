using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using YGOSharp;
using YGOSharp.OCGWrapper.Enums;
using Color = UnityEngine.Color;

public enum GameTextureType
{
    card_picture = 0,
    card_verticle_drawing = 1,
    card_feature = 3
}

public class GameTextureManager
{
    private static bool bLock;

    private static readonly Stack<PictureResource> waitLoadStack = new Stack<PictureResource>();

    private static readonly Dictionary<ulong, PictureResource> loadedList = new Dictionary<ulong, PictureResource>();

    private static readonly Dictionary<ulong, bool> addedMap = new Dictionary<ulong, bool>();

    private static readonly BetterList<UIPictureResource> allUI = new BetterList<UIPictureResource>();

    public static Texture2D myBack;

    public static Texture2D opBack;

    public static Texture2D unknown;

    public static Texture2D attack;

    public static Texture2D negated;

    public static Texture2D bar;

    public static Texture2D exBar;

    public static Texture2D lp;

    public static Texture2D time;

    public static Texture2D L;

    public static Texture2D R;

    public static Texture2D Chain;

    public static Texture2D Mask;

    public static Texture2D N;

    public static Texture2D LINK;
    public static Texture2D LINKm;


    public static Texture2D nt;

    public static Texture2D bp;

    public static Texture2D ep;

    public static Texture2D mp1;

    public static Texture2D mp2;

    public static Texture2D dp;

    public static Texture2D sp;


    public static Texture2D phase;


    public static Texture2D rs;

    public static Texture2D ts;

    public static bool uiLoaded;

    public static Color chainColor = Color.white;

    public static void clearUnloaded()
    {
        while (true)
            try
            {
                while (waitLoadStack.Count > 0)
                {
                    var a = waitLoadStack.Pop();
                    addedMap.Remove(((ulong) a.type << 32) | (ulong) a.code);
                }

                break;
            }
            catch (Exception e)
            {
                Thread.Sleep(10);
                Debug.Log(e);
            }
    }

    public static void clearAll()
    {
        while (true)
            try
            {
                waitLoadStack.Clear();
                loadedList.Clear();
                addedMap.Clear();
                break;
            }
            catch (Exception e)
            {
                Thread.Sleep(10);
                Debug.Log(e);
            }
    }

    private static void thread_run()
    {
        while (Program.Running)
            try
            {
                Thread.Sleep(50);
                var thu = 0;
                while (waitLoadStack.Count > 0)
                {
                    thu++;
                    if (thu == 10)
                    {
                        Thread.Sleep(50);
                        thu = 0;
                    }

                    if (bLock == false)
                    {
                        PictureResource pic;

                        pic = waitLoadStack.Pop();
                        try
                        {
                            pic.pCard = (CardsManager.Get((int) pic.code).Type & (int) CardType.Pendulum) > 0;
                        }
                        catch (Exception e)
                        {
                            Debug.Log("e 0" + e);
                        }

                        if (pic.type == GameTextureType.card_feature)
                            try
                            {
                                ProcessingCardFeature(pic);
                            }
                            catch (Exception e)
                            {
                                Debug.Log("e 1" + e);
                            }

                        if (pic.type == GameTextureType.card_picture)
                            try
                            {
                                ProcessingCardPicture(pic);
                            }
                            catch (Exception e)
                            {
                                Debug.Log("e 2" + e);
                            }

                        if (pic.type == GameTextureType.card_verticle_drawing)
                            try
                            {
                                ProcessingVerticleDrawing(pic);
                            }
                            catch (Exception e)
                            {
                                Debug.Log("e 3" + e);
                            }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("erroe 1" + e);
            }
    }

    private static BitmapHelper getCloseup(PictureResource pic)
    {
        BitmapHelper bitmap = null;
        var found = false;
        var code = pic.code.ToString();
        foreach (var zip in GameZipManager.Zips)
        {
            if (zip.Name.ToLower().EndsWith("script.zip"))
                continue;
            foreach (var file in zip.EntryFileNames)
                if (Regex.IsMatch(file.ToLower(), "closeup/" + code + "\\.png$"))
                {
                    var ms = new MemoryStream();
                    var e = zip[file];
                    e.Extract(ms);
                    bitmap = new BitmapHelper(ms);
                    found = true;
                    break;
                }

            if (found)
                break;
        }

        if (!found)
        {
            var path = "picture/closeup/" + code + ".png";
            if (File.Exists(path)) bitmap = new BitmapHelper(path);
        }

        return bitmap;
    }

    private static byte[] getPicture(PictureResource pic, out bool EightEdition)
    {
        EightEdition = false;
        var code = pic.code.ToString();
        foreach (var zip in GameZipManager.Zips)
        {
            if (zip.Name.ToLower().EndsWith("script.zip"))
                continue;
            foreach (var file in zip.EntryFileNames)
                if (Regex.IsMatch(file.ToLower(), "pics/" + code + "\\.(jpg|png)$"))
                {
                    var ms = new MemoryStream();
                    var e = zip[file];
                    e.Extract(ms);
                    return ms.ToArray();
                }
        }

        var path = "picture/card/" + code + ".png";
        if (!File.Exists(path)) path = "picture/card/" + code + ".jpg";
        if (!File.Exists(path))
        {
            EightEdition = true;
            path = "picture/cardIn8thEdition/" + code + ".png";
        }

        if (!File.Exists(path))
        {
            EightEdition = true;
            path = "picture/cardIn8thEdition/" + code + ".jpg";
        }

        if (File.Exists(path))
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                file.Seek(0, SeekOrigin.Begin);
                var data = new byte[file.Length];
                file.Read(data, 0, (int) file.Length);
                return data;
            }

        return new byte[0];
    }

    private static void ProcessingCardFeature(PictureResource pic)
    {
        if (loadedList.ContainsKey(hashPic(pic.code, pic.type))) return;
        var EightEdition = false;
        var bitmap = getCloseup(pic);
        if (bitmap != null)
        {
            int left;
            int right;
            int up;
            int down;
            CutTop(bitmap, out left, out right, out up, out down);
            up = CutLeft(bitmap, up);
            down = CutRight(bitmap, down);
            right = CutButton(bitmap, right);
            var width = right - left;
            var height = down - up;
            pic.hashed_data = new float[width, height, 4];
            for (var w = 0; w < width; w++)
            for (var h = 0; h < height; h++)
            {
                var color = bitmap.GetPixel(left + w, up + h);
                var a = color.A / 255f;
                if (w < 40)
                    if (a > w / (float) 40)
                        a = w / (float) 40;
                if (w > width - 40)
                    if (a > 1f - (w - (width - 40)) / (float) 40)
                        a = 1f - (w - (width - 40)) / (float) 40;
                if (h < 40)
                    if (a > h / (float) 40)
                        a = h / (float) 40;
                if (h > height - 40)
                    if (a > 1f - (h - (height - 40)) / (float) 40)
                        a = 1f - (h - (height - 40)) / (float) 40;
                pic.hashed_data[w, height - h - 1, 0] = color.R / 255f;
                pic.hashed_data[w, height - h - 1, 1] = color.G / 255f;
                pic.hashed_data[w, height - h - 1, 2] = color.B / 255f;
                pic.hashed_data[w, height - h - 1, 3] = a;
            }

            caculateK(pic);

            loadedList.Add(hashPic(pic.code, pic.type), pic);
        }
        else
        {
            var data = getPicture(pic, out EightEdition);
            if (data.Length == 0)
            {
                pic.hashed_data = new float[10, 10, 4];
                for (var w = 0; w < 10; w++)
                for (var h = 0; h < 10; h++)
                {
                    pic.hashed_data[w, h, 0] = 0;
                    pic.hashed_data[w, h, 1] = 0;
                    pic.hashed_data[w, h, 2] = 0;
                    pic.hashed_data[w, h, 3] = 0;
                }

                loadedList.Add(hashPic(pic.code, pic.type), pic);
            }
            else
            {
                var stream = new MemoryStream(data);
                bitmap = new BitmapHelper(stream);
                pic.hashed_data = getCuttedPic(bitmap, pic.pCard, EightEdition);
                var width = pic.hashed_data.GetLength(0);
                var height = pic.hashed_data.GetLength(1);
                var size = (int) (height * 0.8);
                var empWidth = (width - size) / 2;
                var empHeight = (height - size) / 2;
                var right = width - empWidth;
                var buttom = height - empHeight;
                for (var w = 0; w < width; w++)
                for (var h = 0; h < height; h++)
                {
                    var a = pic.hashed_data[w, h, 3];
                    if (w < empWidth)
                        if (a > w / (float) empWidth)
                            a = w / (float) empWidth;
                    if (h < empHeight)
                        if (a > h / (float) empHeight)
                            a = h / (float) empHeight;
                    if (w > right)
                        if (a > 1f - (w - right) / (float) empWidth)
                            a = 1f - (w - right) / (float) empWidth;
                    if (h > buttom)
                        if (a > 1f - (h - buttom) / (float) empHeight)
                            a = 1f - (h - buttom) / (float) empHeight;
                    pic.hashed_data[w, h, 3] = a * 0.7f;
                }

                loadedList.Add(hashPic(pic.code, pic.type), pic);
            }
        }
    }

    private static void caculateK(PictureResource pic)
    {
        //int width = pic.hashed_data.GetLength(0);
        //int height = pic.hashed_data.GetLength(1);
        //int left = 0;
        //int right = width;
        //if (width > height)
        //{
        //    left = (width - height) / 2;
        //    right = width - left;
        //}
        //int all = 0;
        //for (int h = 0; h < height; h++)
        //{
        //    for (int w = left; w < right; w++)
        //    {
        //        if (pic.hashed_data[w, h, 3] > 0.05f)
        //        {
        //            all += 1;
        //        }
        //    }
        //}
        //float result = ((float)all) / (((float)height) * ((float)(height)));
        //pic.k = result + 0.4f;
        //if (pic.k > 1)
        //{
        //    pic.k = 1f;
        //}
        //if (pic.k < 0)
        //{
        //    pic.k = 0.1f;
        //}

        var width = pic.hashed_data.GetLength(0);
        var height = pic.hashed_data.GetLength(1);
        var h = 0;
        for (h = height - 1; h > 0; h--)
        {
            var all = 0;
            for (var w = 0; w < width; w++)
                if (pic.hashed_data[w, h, 3] > 0.05f)
                    all += 1;
            if (all * 5 > width) break;
        }

        pic.k = h / (float) height;
        if (pic.k > 1) pic.k = 1f;
        if (pic.k < 0) pic.k = 0.1f;
    }

    private static float[,,] getCuttedPic(BitmapHelper bitmap, bool pCard, bool EightEdition)
    {
        int left = 0, top = 0, right = bitmap.colors.GetLength(0), buttom = bitmap.colors.GetLength(1);
        //right is width and buttom is height now
        if (EightEdition)
        {
            if (pCard)
            {
                left = (int) (16f * right / 177f);
                right = (int) (162f * right / 177f);
                top = (int) (50f * buttom / 254f);
                buttom = (int) (158f * buttom / 254f);
            }
            else
            {
                left = (int) (26f * right / 177f);
                right = (int) (152f * right / 177f);
                top = (int) (55f * buttom / 254f);
                buttom = (int) (180f * buttom / 254f);
            }
        }
        else
        {
            if (pCard)
            {
                left = (int) (25f * right / 322f);
                right = (int) (290f * right / 322f);
                top = (int) (73f * buttom / 402f);
                buttom = (int) (245f * buttom / 402f);
            }
            else
            {
                left = (int) (40f * right / 322f);
                right = (int) (280f * right / 322f);
                top = (int) (75f * buttom / 402f);
                buttom = (int) (280f * buttom / 402f);
            }
        }

        var returnValue = new float[right - left, buttom - top, 4];
        for (var w = 0; w < right - left; w++)
        for (var h = 0; h < buttom - top; h++)
        {
            var color = bitmap.GetPixel(left + w, buttom - 1 - h);
            returnValue[w, h, 0] = color.R / 255f;
            returnValue[w, h, 1] = color.G / 255f;
            returnValue[w, h, 2] = color.B / 255f;
            returnValue[w, h, 3] = color.A / 255f;
        }

        return returnValue;
    }

    private static int CutButton(BitmapHelper bitmap, int right)
    {
        for (var w = bitmap.colors.GetLength(0) - 1; w >= 0; w--)
        for (var h = 0; h < bitmap.colors.GetLength(1); h++)
        {
            var color = bitmap.GetPixel(w, h);
            if (color.A > 10)
            {
                right = w;
                return right;
            }
        }

        return right;
    }

    private static int CutRight(BitmapHelper bitmap, int down)
    {
        for (var h = bitmap.colors.GetLength(1) - 1; h >= 0; h--)
        for (var w = 0; w < bitmap.colors.GetLength(0); w++)
        {
            var color = bitmap.GetPixel(w, h);
            if (color.A > 10)
            {
                down = h;
                return down;
            }
        }

        return down;
    }

    private static int CutLeft(BitmapHelper bitmap, int up)
    {
        for (var h = 0; h < bitmap.colors.GetLength(1); h++)
        for (var w = 0; w < bitmap.colors.GetLength(0); w++)
        {
            var color = bitmap.GetPixel(w, h);
            if (color.A > 10)
            {
                up = h;
                return up;
            }
        }

        return up;
    }

    private static void CutTop(BitmapHelper bitmap, out int left, out int right, out int up, out int down)
    {
        ///////切边算法
        left = 0;
        right = bitmap.colors.GetLength(0);
        up = 0;
        down = bitmap.colors.GetLength(1);
        for (var w = 0; w < bitmap.colors.GetLength(0); w++)
        for (var h = 0; h < bitmap.colors.GetLength(1); h++)
        {
            var color = bitmap.GetPixel(w, h);
            if (color.A > 10)
            {
                left = w;
                return;
            }
        }
    }

    private static void ProcessingVerticleDrawing(PictureResource pic)
    {
        if (loadedList.ContainsKey(hashPic(pic.code, pic.type))) return;
        var bitmap = getCloseup(pic);
        if (bitmap == null)
        {
            bool EightEdition;
            var data = getPicture(pic, out EightEdition);
            if (data.Length == 0) return;
            var stream = new MemoryStream(data);
            bitmap = new BitmapHelper(stream);
            pic.hashed_data = getCuttedPic(bitmap, pic.pCard, EightEdition);
            softVtype(pic, 0.5f);
            pic.k = 1;
            //pic.autoMade = true;
        }
        else
        {
            int left;
            int right;
            int up;
            int down;
            CutTop(bitmap, out left, out right, out up, out down);
            up = CutLeft(bitmap, up);
            down = CutRight(bitmap, down);
            right = CutButton(bitmap, right);
            var width = right - left;
            var height = down - up;
            pic.hashed_data = new float[width, height, 4];
            for (var w = 0; w < width; w++)
            for (var h = 0; h < height; h++)
            {
                var color = bitmap.GetPixel(left + w, up + h);
                pic.hashed_data[w, height - h - 1, 0] = color.R / 255f;
                pic.hashed_data[w, height - h - 1, 1] = color.G / 255f;
                pic.hashed_data[w, height - h - 1, 2] = color.B / 255f;
                pic.hashed_data[w, height - h - 1, 3] = color.A / 255f;
            }

            float wholeUNalpha = 0;
            for (var w = 0; w < width; w++)
            {
                if (pic.hashed_data[w, 0, 3] > 0.1f) wholeUNalpha += Math.Abs(w - width / 2) / (float) (width / 2);
                if (pic.hashed_data[w, height - 1, 3] > 0.1f) wholeUNalpha += 1;
            }

            for (var h = 0; h < height; h++)
            {
                if (pic.hashed_data[0, h, 3] > 0.1f) wholeUNalpha += 1;
                if (pic.hashed_data[width - 1, h, 3] > 0.1f) wholeUNalpha += 1;
            }

            if (wholeUNalpha >= (width + height) * 0.5f * 0.12f) softVtype(pic, 0.7f);
            caculateK(pic);
        }

        loadedList.Add(hashPic(pic.code, pic.type), pic);
    }

    private static void softVtype(PictureResource pic, float si)
    {
        var width = pic.hashed_data.GetLength(0);
        var height = pic.hashed_data.GetLength(1);
        var size = (int) (height * si);
        var empWidth = (width - size) / 2;
        var empHeight = (height - size) / 2;
        var right = width - empWidth;
        var buttom = height - empHeight;
        var dui = (float) Math.Sqrt(width / 2 * (width / 2) + height / 2 * (height / 2));
        for (var w = 0; w < width; w++)
        for (var h = 0; h < height; h++)
        {
            var a = pic.hashed_data[w, h, 3];

            if (h < height / 2)
            {
                var l = (float) Math.Sqrt((width / 2 - w) * (width / 2 - w) + (height / 2 - h) * (height / 2 - h));
                l -= width * 0.3f;
                if (l < 0) l = 0;
                var alpha = 1f - l / (0.6f * (dui - width * 0.3f));
                if (alpha < 0) alpha = 0;
                if (a > alpha)
                    a = alpha;
            }

            if (w < empWidth)
                if (a > w / (float) empWidth)
                    a = w / (float) empWidth;
            if (h < empHeight)
                if (a > h / (float) empHeight)
                    a = h / (float) empHeight;
            if (w > right)
                if (a > 1f - (w - right) / (float) empWidth)
                    a = 1f - (w - right) / (float) empWidth;
            if (h > buttom)
                if (a > 1f - (h - buttom) / (float) empHeight)
                    a = 1f - (h - buttom) / (float) empHeight;
            pic.hashed_data[w, h, 3] = a;
        }
    }

    private static void ProcessingCardPicture(PictureResource pic)
    {
        if (loadedList.ContainsKey(hashPic(pic.code, pic.type))) return;

        bool EightEdition;
        var data = getPicture(pic, out EightEdition);
        if (data.Length > 0)
        {
            pic.data = data;
            loadedList.Add(hashPic(pic.code, pic.type), pic);
        }
        else
        {
            if (pic.code > 0)
                pic.u_data = unknown;
            else
                pic.u_data = myBack;
            loadedList.Add(hashPic(pic.code, pic.type), pic);
        }
    }

    private static ulong hashPic(long code, GameTextureType type)
    {
        return ((ulong) type << 32) | (ulong) code;
    }

    public static Texture2D get(long code, GameTextureType type, Texture2D nullReturnValue = null)
    {
        try
        {
            PictureResource r;
            if (loadedList.TryGetValue(hashPic(code, type), out r))
            {
                Texture2D re = null;
                if (r.u_data != null)
                {
                    if (r.u_data == myBack)
                        return nullReturnValue;
                    return r.u_data;
                }

                if (r.data != null)
                {
                    re = new Texture2D(400, 600);
                    re.LoadImage(r.data);
                    r.u_data = re;
                    return re;
                }

                if (r.hashed_data != null)
                {
                    var width = r.hashed_data.GetLength(0);
                    var height = r.hashed_data.GetLength(1);
                    var cols = new Color[width * height];
                    re = new Texture2D(width, height);
                    for (var h = 0; h < height; h++)
                    for (var w = 0; w < width; w++)
                        cols[h * width + w] = new Color(r.hashed_data[w, h, 0], r.hashed_data[w, h, 1],
                            r.hashed_data[w, h, 2], r.hashed_data[w, h, 3]);
                    re.SetPixels(0, 0, width, height, cols);
                    re.Apply();
                    r.u_data = re;
                    return re;
                }
            }
            else
            {
                if (!addedMap.ContainsKey(hashPic(code, type)))
                {
                    var a = new PictureResource(type, code, nullReturnValue);
                    bLock = true;
                    waitLoadStack.Push(a);
                    bLock = false;
                    addedMap.Add(((ulong) type << 32) | (ulong) code, true);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("BIGERROR1:" + e);
        }

        return null;
    }

    public static float getK(long code, GameTextureType type)
    {
        float ret = 1;
        PictureResource r;
        if (loadedList.TryGetValue(hashPic(code, type), out r)) ret = r.k;
        return ret;
    }

    public static Texture2D get(string name)
    {
        if (uiLoaded == false)
        {
            uiLoaded = true;
            var fileInfos = new DirectoryInfo("texture/ui").GetFiles();
            for (var i = 0; i < fileInfos.Length; i++)
                if (fileInfos[i].Name.Length > 4)
                    if (fileInfos[i].Name.Substring(fileInfos[i].Name.Length - 4, 4) == ".png")
                    {
                        var r = new UIPictureResource();
                        r.name = fileInfos[i].Name.Substring(0, fileInfos[i].Name.Length - 4);
                        r.data = UIHelper.getTexture2D("texture/ui/" + fileInfos[i].Name);
                        allUI.Add(r);
                    }
        }

        Texture2D re = null;
        for (var i = 0; i < allUI.size; i++)
            if (allUI[i].name == name)
            {
                re = allUI[i].data;
                break;
            }

        if (re == null)
        {
        }

        return re;
    }

    internal static void initialize()
    {
        attack = UIHelper.getTexture2D("texture/duel/attack.png");
        myBack = UIHelper.getTexture2D("texture/duel/me.jpg");
        opBack = UIHelper.getTexture2D("texture/duel/opponent.jpg");
        unknown = UIHelper.getTexture2D("texture/duel/unknown.jpg");
        negated = UIHelper.getTexture2D("texture/duel/negated.png");
        bar = UIHelper.getTexture2D("texture/duel/healthBar/bg.png");
        exBar = UIHelper.getTexture2D("texture/duel/healthBar/excited.png");
        time = UIHelper.getTexture2D("texture/duel/healthBar/t.png");
        lp = UIHelper.getTexture2D("texture/duel/healthBar/lp.png");
        L = UIHelper.getTexture2D("texture/duel/L.png");
        R = UIHelper.getTexture2D("texture/duel/R.png");
        LINK = UIHelper.getTexture2D("texture/duel/link.png");
        LINKm = UIHelper.getTexture2D("texture/duel/linkMask.png");
        Chain = UIHelper.getTexture2D("texture/duel/chain.png");
        Mask = UIHelper.getTexture2D("texture/duel/mask.png");


        nt = UIHelper.getTexture2D("texture/duel/phase/nt.png");
        bp = UIHelper.getTexture2D("texture/duel/phase/bp.png");
        ep = UIHelper.getTexture2D("texture/duel/phase/ep.png");
        mp1 = UIHelper.getTexture2D("texture/duel/phase/mp1.png");
        mp2 = UIHelper.getTexture2D("texture/duel/phase/mp2.png");
        dp = UIHelper.getTexture2D("texture/duel/phase/dp.png");
        sp = UIHelper.getTexture2D("texture/duel/phase/sp.png");

        phase = UIHelper.getTexture2D("texture/duel/phase/phase.png");

        rs = UIHelper.getTexture2D("texture/duel/phase/rs.png");
        ts = UIHelper.getTexture2D("texture/duel/phase/ts.png");

        N = new Texture2D(10, 10);
        for (var i = 0; i < 10; i++)
        for (var a = 0; a < 10; a++)
            N.SetPixel(i, a, new Color(0, 0, 0, 0));
        N.Apply();
        try
        {
            ColorUtility.TryParseHtmlString(File.ReadAllText("texture/duel/chainColor.txt"), out chainColor);
        }
        catch (Exception)
        {
        }

        var main = new Thread(thread_run);
        main.Start();
    }

    public class BitmapHelper
    {
        public System.Drawing.Color[,] colors;

        public BitmapHelper(string path)
        {
            Bitmap bitmap;
            try
            {
                bitmap = (Bitmap) Image.FromFile(path);
            }
            catch (Exception)
            {
                bitmap = new Bitmap(10, 10);
                for (var i = 0; i < 10; i++)
                for (var w = 0; w < 10; w++)
                    bitmap.SetPixel(i, w, System.Drawing.Color.White);
            }

            init(bitmap);
        }

        public BitmapHelper(MemoryStream stream)
        {
            Bitmap bitmap;
            try
            {
                bitmap = (Bitmap) Image.FromStream(stream);
            }
            catch (Exception)
            {
                bitmap = new Bitmap(10, 10);
                for (var i = 0; i < 10; i++)
                for (var w = 0; w < 10; w++)
                    bitmap.SetPixel(i, w, System.Drawing.Color.White);
            }

            init(bitmap);
        }

        private void init(Bitmap bitmap)
        {
            var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            var ptr = bmpData.Scan0;
            var bytes = Math.Abs(bmpData.Stride) * bitmap.Height;
            var rgbValues = new byte[bytes];
            Marshal.Copy(ptr, rgbValues, 0, bytes);
            colors = new System.Drawing.Color[bitmap.Width, bitmap.Height];
            for (var counter = 0; counter < rgbValues.Length; counter += 4)
            {
                var i_am = counter / 4;
                colors[i_am % bitmap.Width, i_am / bitmap.Width]
                    =
                    System.Drawing.Color.FromArgb(
                        rgbValues[counter + 3],
                        rgbValues[counter + 2],
                        rgbValues[counter + 1],
                        rgbValues[counter + 0]);
            }

            bitmap.UnlockBits(bmpData);
            bitmap.Dispose();
        }

        public System.Drawing.Color GetPixel(int a, int b)
        {
            return colors[a, b];
        }
    }

    private class PictureResource
    {
        public readonly long code;

        //public bool autoMade = false;
        public byte[] data;
        public float[,,] hashed_data;
        public float k = 1;
        public Texture2D nullReturen;
        public bool pCard;
        public readonly GameTextureType type;
        public Texture2D u_data;

        public PictureResource(GameTextureType t, long c, Texture2D n)
        {
            type = t;
            code = c;
            nullReturen = n;
        }
    }

    private class UIPictureResource
    {
        public Texture2D data;
        public string name;
    }
}