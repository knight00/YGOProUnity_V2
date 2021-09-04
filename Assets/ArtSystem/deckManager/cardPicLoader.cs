using UnityEngine;
using YGOSharp;

public class cardPicLoader : MonoBehaviour
{
    public int loaded_code = -1;

    public int code;

    public Texture2D defaults;

    public ban_icon ico;

    public Collider coli;

    public UITexture uiTexture;

    public Banlist loaded_banlist;

    public Card data { get; set; }

    private void Update()
    {
        if (coli != null)
            if (Program.InputGetMouseButtonDown_0)
                if (Program.pointedCollider == coli)
                    Program.I().cardDescription.setData(CardsManager.Get(code), GameTextureManager.myBack, "", true);
        if (Program.I().deckManager != null)
        {
            if (loaded_code != code)
            {
                var t = GameTextureManager.get(code, GameTextureType.card_picture, defaults);
                if (t != null)
                {
                    uiTexture.mainTexture = t;
                    uiTexture.aspectRatio = t.width / (float) t.height;
                    uiTexture.forceWidth((int) (uiTexture.height * uiTexture.aspectRatio));
                    loaded_code = code;
                    loaded_banlist = null;
                }
            }

            if (loaded_banlist != Program.I().deckManager.currentBanlist)
            {
                loaded_banlist = Program.I().deckManager.currentBanlist;
                if (ico != null)
                {
                    if (loaded_banlist == null)
                    {
                        ico.show(3);
                        return;
                    }

                    ico.show(loaded_banlist.GetQuantity(code));
                }
            }
        }
    }

    public void clear()
    {
        loaded_code = 0;
        code = 0;
        ico.show(3);
        uiTexture.mainTexture = null;
    }

    public void reCode(int c)
    {
        loaded_code = 0;
        code = c;
    }

    public void relayer(int l)
    {
        uiTexture.depth = 50 + l * 2;
        var t = ico.gameObject.GetComponent<UITexture>();
        t.depth = 51 + l * 2;
    }
}