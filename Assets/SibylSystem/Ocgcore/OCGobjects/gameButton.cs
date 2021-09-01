using UnityEngine;

public class gameButton : OCGobject
{
    public gameCard cookieCard;

    public string cookieString;
    public GameObject gameObjectEvent;

    public string hint;

    public bool notCookie;

    public int response;

    public superButtonType type;

    public gameButton(int response, string hint, superButtonType type)
    {
        this.response = response;

        this.hint = hint;

        this.type = type;
    }

    public void show(Vector3 v)
    {
        if (gameObject == null)
        {
            gameObject = create(Program.I().new_ui_superButton, Program.camera_main_2d.ScreenToWorldPoint(v),
                Vector3.zero, false, Program.ui_main_2d);
            gameObjectEvent = UIHelper.getRealEventGameObject(gameObject);
            UIHelper.registEvent(gameObject, clicked);
            gameObject.GetComponent<iconSetForButton>().setTexture(type);
            gameObject.GetComponent<iconSetForButton>().setText(hint);
            gameObject.transform.localScale = Vector3.zero;
            iTween.ScaleTo(gameObject,
                new Vector3(0.7f * Screen.height / 700f, 0.7f * Screen.height / 700f, 0.7f * Screen.height / 700f),
                0.2f);
        }

        gameObject.transform.position = Program.camera_main_2d.ScreenToWorldPoint(v);
    }

    private void clicked()
    {
        Program.I().ocgcore.ES_gameButtonClicked(this);
    }

    public void hide()
    {
        destroy(gameObject, 0.2f, true, true);
        gameObject = null;
        gameObjectEvent = null;
    }
}