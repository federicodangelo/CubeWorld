using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuSystem
{
    static public GUISkin skin;

    public delegate void OnPressedDelegate();

    static private int cantidadBotones;

    static private int focusedButton = 0;

    static private bool ignoreAxis;
    static private bool ignoreButton;

    static private List<OnPressedDelegate> delegates = new List<OnPressedDelegate>();

    static public float vAxis = 0.0f;
    static public bool actionButtonDown = false;
    static public bool useKeyboard = true;

    static public void ResetFocus()
    {
        focusedButton = 0;
    }

    static public void BeginMenu(string text)
    {
        cantidadBotones = 0;
        delegates.Clear();

        //Screen.showCursor = false;
        GUI.skin = skin;
        GUI.BeginGroup(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 200, 400, 400));

        GUI.Box(new Rect(0, 0, 400, 400), text);
    }

    static public void Button(string text, OnPressedDelegate onPressed)
    {
        delegates.Add(onPressed);

        GUI.SetNextControlName("Boton" + cantidadBotones.ToString());

        if (GUI.Button(new Rect(10, 40 + 30 * 2 * cantidadBotones, 380, 30), text))
            onPressed();

        cantidadBotones++;
    }

    static public string TextField(string text)
    {
        GUI.SetNextControlName("Boton" + cantidadBotones.ToString());

        text = GUI.TextField(new Rect(10, 40 + 30 * 2 * cantidadBotones, 380, 30), text);

        cantidadBotones++;

        return text;
    }

    static public void LastButton(string text, OnPressedDelegate onPressed)
    {
        delegates.Add(onPressed);

        GUI.SetNextControlName("Boton" + cantidadBotones.ToString());

        if (GUI.Button(new Rect(10, 400 - 70, 380, 30), text))
            onPressed();

        cantidadBotones++;
    }

    static public void EndMenu()
    {
        GUI.EndGroup();

        if (useKeyboard)
        {
            if (cantidadBotones > 0 && GUIUtility.hotControl == 0)
            {
                if (!ignoreAxis && vAxis != 0)
                {
                    if (vAxis > 0)
                        focusedButton--;
                    else if (vAxis < 0)
                        focusedButton++;

                    ignoreAxis = true;
                }

                if (vAxis == 0)
                    ignoreAxis = false;

                if (focusedButton < 0)
                    focusedButton = cantidadBotones - 1;

                if (focusedButton >= cantidadBotones)
                    focusedButton = 0;

                GUI.FocusControl("Boton" + focusedButton.ToString());

                if (!ignoreButton && actionButtonDown)
                {
                    ignoreButton = true;
                    delegates[focusedButton]();
                }
                else if (!actionButtonDown)
                {
                    ignoreButton = false;
                }
            }
        }
    }
}
