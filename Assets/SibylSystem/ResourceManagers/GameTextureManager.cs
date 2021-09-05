using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class GameTextureManager
{
    private static bool bLock;
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
    public static Color chainColor = Color.white;

    internal static void initialize()
    {
        attack = UIHelper.GetTexture2D("texture/duel/attack.png");
        myBack = UIHelper.GetTexture2D("texture/duel/me.jpg");
        opBack = UIHelper.GetTexture2D("texture/duel/opponent.jpg");
        unknown = UIHelper.GetTexture2D("texture/duel/unknown.jpg");
        negated = UIHelper.GetTexture2D("texture/duel/negated.png");
        bar = UIHelper.GetTexture2D("texture/duel/healthBar/bg.png");
        exBar = UIHelper.GetTexture2D("texture/duel/healthBar/excited.png");
        time = UIHelper.GetTexture2D("texture/duel/healthBar/t.png");
        lp = UIHelper.GetTexture2D("texture/duel/healthBar/lp.png");
        L = UIHelper.GetTexture2D("texture/duel/L.png");
        R = UIHelper.GetTexture2D("texture/duel/R.png");
        LINK = UIHelper.GetTexture2D("texture/duel/link.png");
        LINKm = UIHelper.GetTexture2D("texture/duel/linkMask.png");
        Chain = UIHelper.GetTexture2D("texture/duel/chain.png");
        Mask = UIHelper.GetTexture2D("texture/duel/mask.png");
        nt = UIHelper.GetTexture2D("texture/duel/phase/nt.png");
        bp = UIHelper.GetTexture2D("texture/duel/phase/bp.png");
        ep = UIHelper.GetTexture2D("texture/duel/phase/ep.png");
        mp1 = UIHelper.GetTexture2D("texture/duel/phase/mp1.png");
        mp2 = UIHelper.GetTexture2D("texture/duel/phase/mp2.png");
        dp = UIHelper.GetTexture2D("texture/duel/phase/dp.png");
        sp = UIHelper.GetTexture2D("texture/duel/phase/sp.png");
        phase = UIHelper.GetTexture2D("texture/duel/phase/phase.png");
        rs = UIHelper.GetTexture2D("texture/duel/phase/rs.png");
        ts = UIHelper.GetTexture2D("texture/duel/phase/ts.png");
        N = new Texture2D(10, 10);
        for (var i = 0; i < 10; i++)
        for (var a = 0; a < 10; a++)
            N.SetPixel(i, a, new Color(0, 0, 0, 0));
        N.Apply();
        ColorUtility.TryParseHtmlString(File.ReadAllText("texture/duel/chainColor.txt"), out chainColor);
    }

    public static void clearAll()
    {
        loadedPicture.Clear();
        loadedCloseUp.Clear();
    }

    private static readonly Dictionary<int, Task<Texture2D>> loadedPicture = new();
    private static readonly Dictionary<int, Task<Texture2D>> loadedCloseUp = new();
    private static readonly Dictionary<string, Texture2D> loadedUI = new();

    public static async Task<Texture2D> GetCardPicture(int code)
    {
        if (code == 0) return null;
        if (loadedPicture.TryGetValue(code, out var cached)) return await cached;
        foreach (var extname in new[] {".png", ".jpg"})
        {
            var path = $"picture/card/{code}{extname}";
            if (File.Exists(path))
            {
                var result = UIHelper.GetTexture2DAsync(path);
                loadedPicture.Add(code, result);
                return await result;
            }
        }

        return null;
    }

    public static async Task<Texture2D> GetCardCloseUp(int code)
    {
        if (loadedCloseUp.TryGetValue(code, out var cached)) return await cached;
        var path = $"picture/closeup/{code}.png";
        if (File.Exists(path))
        {
            var result = UIHelper.GetTexture2DAsync(path);
            loadedCloseUp.Add(code, result);
            return await result;
        }

        return null;
    }

    public static Texture2D GetUI(string name)
    {
        var path = $"texture/ui/{name}.png";
        if (loadedUI.TryGetValue(path, out var cached)) return cached;
        var result = UIHelper.GetTexture2D(path);
        loadedUI.Add(path, result);
        return result;
    }
}