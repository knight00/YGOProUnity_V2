using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using YGOSharp;
using YGOSharp.OCGWrapper.Enums;

public class selectDeck : WindowServantSP
{
    private UIDeckPanel deckPanel;


    private string deckSelected = "";

    private string preString = "";


    private readonly cardPicLoader[] quickCards = new cardPicLoader[200];
    private UIInput searchInput;

    private readonly string sort = "sortByTimeDeck";

    private UIselectableList superScrollView;

    public override void initialize()
    {
        SetWindow(Program.I().remaster_deckManager);
        deckPanel = gameObject.GetComponentInChildren<UIDeckPanel>();
        UIHelper.registEvent(gameObject, "exit_", onClickExit);
        superScrollView = gameObject.GetComponentInChildren<UIselectableList>();
        superScrollView.selectedAction = onSelected;
        UIHelper.registEvent(gameObject, "sort_", onSort);
        setSortLable();
        UIHelper.registEvent(gameObject, "edit_", onEdit);
        UIHelper.registEvent(gameObject, "new_", onNew);
        UIHelper.registEvent(gameObject, "dispose_", onDispose);
        UIHelper.registEvent(gameObject, "copy_", onCopy);
        UIHelper.registEvent(gameObject, "rename_", onRename);
        UIHelper.registEvent(gameObject, "code_", onCode);
        searchInput = UIHelper.getByName<UIInput>(gameObject, "search_");
        superScrollView.install();
        for (var i = 0; i < quickCards.Length; i++)
        {
            quickCards[i] = deckPanel.createCard();
            quickCards[i].relayer(i);
        }

        SetActiveFalse();
    }

    private void onSearch()
    {
        printFile();
        superScrollView.toTop();
    }

    private void onEdit()
    {
        if (!superScrollView.Selected()) return;
        if (!isShowed) return;
        KF_editDeck(superScrollView.selectedString);
    }

    private void returnToSelect()
    {
        if (Program.exitOnReturn)
            Program.I().menu.onClickExit();
        else
            Program.I().shiftToServant(Program.I().selectDeck);
    }

    public override void preFrameFunction()
    {
        base.preFrameFunction();
        Menu.checkCommend();
        if (searchInput.value != preString)
        {
            preString = searchInput.value;
            onSearch();
        }
    }

    public void KF_editDeck(string deckName)
    {
        var path = "deck/" + deckName + ".ydk";
        if (File.Exists(path))
        {
            Config.Set("deckInUse", deckName);
            Program.I().deckManager.shiftCondition(DeckManager.Condition.editDeck);
            Program.I().shiftToServant(Program.I().deckManager);
            Program.I().deckManager.loadDeckFromYDK(path);
            Program.I().cardDescription.setTitle(deckName);
            Program.I().deckManager.setGoodLooking();
            Program.I().deckManager.returnAction =
                () =>
                {
                    if (Program.I().deckManager.deckDirty)
                        RMSshow_yesOrNoOrCancle(
                            "deckManager_returnAction"
                            , InterString.Get("要保存卡组的变更吗？")
                            , new messageSystemValue {hint = "yes", value = "yes"}
                            , new messageSystemValue {hint = "no", value = "no"}
                            , new messageSystemValue {hint = "cancle", value = "cancle"}
                        );
                    else
                        returnToSelect();
                };
        }
    }

    public override void ES_RMS(string hashCode, List<messageSystemValue> result)
    {
        base.ES_RMS(hashCode, result);
        if (hashCode == "deckManager_returnAction")
        {
            if (result[0].value == "yes")
                if (Program.I().deckManager.onSave())
                    returnToSelect();
            if (result[0].value == "no") returnToSelect();
        }

        if (hashCode == "onNew")
            try
            {
                File.Create("deck/" + result[0].value + ".ydk").Close();
                RMSshow_none(InterString.Get("「[?]」创建完毕。", result[0].value));
                superScrollView.selectedString = result[0].value;
                printFile();
            }
            catch (Exception)
            {
                RMSshow_none(InterString.Get("创建卡组失败！请检查输入的文件名，以及文件夹权限。"));
            }

        if (hashCode == "onDispose")
            if (result[0].value == "yes")
                try
                {
                    File.Delete("deck/" + superScrollView.selectedString + ".ydk");
                    RMSshow_none(InterString.Get("「[?]」删除完毕。", superScrollView.selectedString));
                    printFile();
                }
                catch (Exception)
                {
                    RMSshow_none(InterString.Get("删除卡组失败！请检查文件夹权限。"));
                }

        if (hashCode == "onCopy")
            try
            {
                File.Copy("deck/" + superScrollView.selectedString + ".ydk", "deck/" + result[0].value + ".ydk");
                RMSshow_none(InterString.Get("「[?]」复制完毕。", superScrollView.selectedString));
                superScrollView.selectedString = result[0].value;
                printFile();
            }
            catch (Exception)
            {
                RMSshow_none(InterString.Get("复制卡组失败！请检查输入的文件名，以及文件夹权限。"));
            }

        if (hashCode == "onRename")
            try
            {
                File.Move("deck/" + superScrollView.selectedString + ".ydk", "deck/" + result[0].value + ".ydk");
                RMSshow_none(InterString.Get("「[?]」重命名完毕。", superScrollView.selectedString));
                superScrollView.selectedString = result[0].value;
                printFile();
            }
            catch (Exception)
            {
                RMSshow_none(InterString.Get("重命名卡组失败！请检查输入的文件名，以及文件夹权限。"));
            }
    }

    private void onNew()
    {
        RMSshow_input("onNew", InterString.Get("请输入要创建的卡组名"), UIHelper.getTimeString());
    }

    private void onDispose()
    {
        if (!superScrollView.Selected()) return;
        var path = "deck/" + superScrollView.selectedString + ".ydk";
        if (File.Exists(path))
            RMSshow_yesOrNo(
                "onDispose"
                , InterString.Get("确认删除「[?]」吗？", superScrollView.selectedString)
                , new messageSystemValue {hint = "yes", value = "yes"}
                , new messageSystemValue {hint = "no", value = "no"}
            );
    }

    private void onCopy()
    {
        if (!superScrollView.Selected()) return;
        var path = "deck/" + superScrollView.selectedString + ".ydk";
        if (File.Exists(path))
        {
            var newname = InterString.Get("[?]的副本", superScrollView.selectedString);
            var newnamer = newname;
            var i = 1;
            while (File.Exists("deck/" + newnamer + ".ydk"))
            {
                newnamer = newname + i;
                i++;
            }

            RMSshow_input("onCopy", InterString.Get("请输入复制后的卡组名"), newnamer);
        }
    }

    private void onRename()
    {
        if (!superScrollView.Selected()) return;
        var path = "deck/" + superScrollView.selectedString + ".ydk";
        if (File.Exists(path)) RMSshow_input("onRename", InterString.Get("新的卡组名"), superScrollView.selectedString);
    }

    private void onCode()
    {
        if (!superScrollView.Selected()) return;
        var path = "deck/" + superScrollView.selectedString + ".ydk";
        if (File.Exists(path))
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN //编译器、Windows
            Process.Start("notepad.exe", path);
#elif UNITY_STANDALONE_OSX //Mac OS X
                System.Diagnostics.Process.Start("open", "-e " + path);
#elif UNITY_STANDALONE_LINUX //Linux
                System.Diagnostics.Process.Start("gedit", path);
#endif
        }
    }

    private void setSortLable()
    {
        if (Config.Get(sort, "1") == "1")
            UIHelper.trySetLableText(gameObject, "sort_", InterString.Get("时间排序"));
        else
            UIHelper.trySetLableText(gameObject, "sort_", InterString.Get("名称排序"));
    }

    private void onSort()
    {
        if (Config.Get(sort, "1") == "1")
            Config.Set(sort, "0");
        else
            Config.Set(sort, "1");
        setSortLable();
        printFile();
    }

    private void onSelected()
    {
        if (deckSelected == superScrollView.selectedString) onEdit();
        deckSelected = superScrollView.selectedString;
        printSelected();
    }

    private void printSelected()
    {
        GameTextureManager.clearUnloaded();
        Deck deck;
        DeckManager.FromYDKtoCodedDeck("deck/" + deckSelected + ".ydk", out deck);
        var mainAll = 0;
        var mainMonster = 0;
        var mainSpell = 0;
        var mainTrap = 0;
        var sideAll = 0;
        var sideMonster = 0;
        var sideSpell = 0;
        var sideTrap = 0;
        var extraAll = 0;
        var extraFusion = 0;
        var extraLink = 0;
        var extraSync = 0;
        var extraXyz = 0;
        var currentIndex = 0;

        var hangshu = UIHelper.get_decklieshuArray(deck.Main.Count);
        foreach (var item in deck.Main)
        {
            mainAll++;
            var c = CardsManager.Get(item);
            if ((c.Type & (uint) CardType.Monster) > 0) mainMonster++;
            if ((c.Type & (uint) CardType.Spell) > 0) mainSpell++;
            if ((c.Type & (uint) CardType.Trap) > 0) mainTrap++;
            quickCards[currentIndex].reCode(item);
            var v = UIHelper.get_hang_lieArry(mainAll - 1, hangshu);
            quickCards[currentIndex].transform.localPosition = new Vector3
            (
                -176.3f + UIHelper.get_left_right_indexZuo(0, 352f, (int) v.y, hangshu[(int) v.x], 10)
                ,
                161.6f - v.x * 60f
                ,
                0
            );
            if (currentIndex <= 198) currentIndex++;
        }

        foreach (var item in deck.Side)
        {
            sideAll++;
            var c = CardsManager.Get(item);
            if ((c.Type & (uint) CardType.Monster) > 0) sideMonster++;
            if ((c.Type & (uint) CardType.Spell) > 0) sideSpell++;
            if ((c.Type & (uint) CardType.Trap) > 0) sideTrap++;
            quickCards[currentIndex].reCode(item);
            quickCards[currentIndex].transform.localPosition = new Vector3
            (
                -176.3f + UIHelper.get_left_right_indexZuo(0, 352f, sideAll - 1, deck.Side.Count, 10)
                ,
                -181.1f
                ,
                0
            );
            if (currentIndex <= 198) currentIndex++;
        }

        foreach (var item in deck.Extra)
        {
            extraAll++;
            var c = CardsManager.Get(item);
            if ((c.Type & (uint) CardType.Fusion) > 0) extraFusion++;
            if ((c.Type & (uint) CardType.Synchro) > 0) extraSync++;
            if ((c.Type & (uint) CardType.Xyz) > 0) extraXyz++;
            if ((c.Type & (uint) CardType.Link) > 0) extraLink++;
            quickCards[currentIndex].reCode(item);
            quickCards[currentIndex].transform.localPosition = new Vector3
            (
                -176.3f + UIHelper.get_left_right_indexZuo(0, 352f, extraAll - 1, deck.Extra.Count, 10)
                ,
                -99.199f
                ,
                0
            );
            if (currentIndex <= 198) currentIndex++;
        }

        while (true)
        {
            quickCards[currentIndex].clear();
            if (currentIndex <= 198)
                currentIndex++;
            else
                break;
        }

        deckPanel.leftMain.text = GameStringHelper._zhukazu + mainAll;
        deckPanel.leftExtra.text = GameStringHelper._ewaikazu + extraAll;
        deckPanel.leftSide.text = GameStringHelper._fukazu + sideAll;
        deckPanel.rightMain.text = GameStringHelper._guaishou + mainMonster + " " + GameStringHelper._mofa + mainSpell +
                                   " " + GameStringHelper._xianjing + mainTrap;
        deckPanel.rightExtra.text = GameStringHelper._ronghe + extraFusion + " " + GameStringHelper._tongtiao +
                                    extraSync + " " + GameStringHelper._chaoliang + extraXyz + " " +
                                    GameStringHelper._lianjie + extraLink;
        deckPanel.rightSide.text = GameStringHelper._guaishou + sideMonster + " " + GameStringHelper._mofa + sideSpell +
                                   " " + GameStringHelper._xianjing + sideTrap;
    }

    public override void show()
    {
        base.show();
        printFile();
        superScrollView.toTop();
        superScrollView.selectedString = Config.Get("deckInUse", "miaowu");
        printSelected();
        Program.charge();
    }

    public override void hide()
    {
        if (isShowed)
            if (superScrollView.Selected())
                Config.Set("deckInUse", superScrollView.selectedString);
        base.hide();
    }

    private void printFile()
    {
        var deckInUse = Config.Get("deckInUse", "miaowu");
        superScrollView.clear();
        var fileInfos = new DirectoryInfo("deck").GetFiles();
        if (Config.Get(sort, "1") == "1")
            Array.Sort(fileInfos, UIHelper.CompareTime);
        else
            Array.Sort(fileInfos, UIHelper.CompareName);
        for (var i = 0; i < fileInfos.Length; i++)
            if (fileInfos[i].Name.Length > 4)
                if (fileInfos[i].Name.Substring(fileInfos[i].Name.Length - 4, 4) == ".ydk")
                    if (fileInfos[i].Name.Substring(0, fileInfos[i].Name.Length - 4) == deckInUse)
                        if (searchInput.value == "" ||
                            Regex.Replace(fileInfos[i].Name, searchInput.value, "miaowu", RegexOptions.IgnoreCase) !=
                            fileInfos[i].Name)
                            superScrollView.add(fileInfos[i].Name.Substring(0, fileInfos[i].Name.Length - 4));
        for (var i = 0; i < fileInfos.Length; i++)
            if (fileInfos[i].Name.Length > 4)
                if (fileInfos[i].Name.Substring(fileInfos[i].Name.Length - 4, 4) == ".ydk")
                    if (fileInfos[i].Name.Substring(0, fileInfos[i].Name.Length - 4) != deckInUse)
                        if (searchInput.value == "" ||
                            Regex.Replace(fileInfos[i].Name, searchInput.value, "miaowu", RegexOptions.IgnoreCase) !=
                            fileInfos[i].Name)
                            superScrollView.add(fileInfos[i].Name.Substring(0, fileInfos[i].Name.Length - 4));
        if (superScrollView.Selected() == false) superScrollView.selectTop();
    }

    private void onClickExit()
    {
        if (Program.exitOnReturn)
            Program.I().menu.onClickExit();
        else
            Program.I().shiftToServant(Program.I().menu);
    }
}