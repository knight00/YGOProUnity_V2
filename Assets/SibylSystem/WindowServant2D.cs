using UnityEngine;

public class WindowServant2D : Servant
{
    public override void applyHideArrangement()
    {
        if (gameObject != null)
        {
            UIHelper.clearITWeen(gameObject);
            iTween.MoveTo(gameObject,
                Program.I().camera_main_2d.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height * 1.5f, 0)),
                0.6f);
        }
    }

    public override void applyShowArrangement()
    {
        if (gameObject != null)
        {
            UIHelper.clearITWeen(gameObject);
            iTween.MoveTo(gameObject,
                Program.I().camera_main_2d.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0)), 0.6f);
        }
    }

    public override void hide()
    {
        base.hide();
        Program.ShiftUIenabled(Program.I().ui_main_3d, true);
    }

    public override void show()
    {
        base.show();
        Program.ShiftUIenabled(Program.I().ui_main_3d, false);
    }

    public static GameObject createWindow(Servant servant, GameObject mod)
    {
        var re = servant.create
        (
            mod,
            Program.I().camera_main_2d.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height * 1.5f, 600)),
            new Vector3(0, 0, 0),
            false,
            Program.I().ui_main_2d
        );
        UIHelper.InterGameObject(re);
        return re;
    }
}