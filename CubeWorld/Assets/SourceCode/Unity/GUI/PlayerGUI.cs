using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CubeWorld.Tiles;
using CubeWorld.World.Objects;

public class PlayerGUI : MonoBehaviour 
{
    public int lastFps = 0;

    private int frames;
    private float lastTime;

    public PlayerUnity playerUnity;

    public enum State
    {
        NORMAL,
        INVENTORY
    }

    public State ActiveState
    {
        get { return this.state; }
        set
        {
            if (value != state)
            {
                activeGUIState.OnDeactivated();
                this.state = value;
                this.activeGUIState = availableStates[state];
                this.activeGUIState.OnActivated();
            }
        }
    }

    private State state;
    private GUIState activeGUIState;
    private Dictionary<State, GUIState> availableStates = new Dictionary<State, GUIState>();

    void Start()
    {
        lastTime = Time.realtimeSinceStartup;

        availableStates[State.NORMAL] = new GUIStatePlayerNormal(this);
        availableStates[State.INVENTORY] = new GUIStatePlayerInventory(this);

        state = State.NORMAL;
        activeGUIState = availableStates[State.NORMAL];
        activeGUIState.OnActivated();
    }

    void Update()
    {
        UpdateFPS();

        if (playerUnity.gameManagerUnity.State == GameManagerUnity.GameManagerUnityState.GAME)
            activeGUIState.ProcessKeys();
    }

    private void UpdateFPS()
    {
        frames++;

        if (Time.realtimeSinceStartup - lastTime >= 1.0f)
        {
            lastFps = (int) ((float)frames / (Time.realtimeSinceStartup - lastTime));
            lastTime = Time.realtimeSinceStartup;
            frames = 0;
        }
    }

    public void EnterInventory()
    {
        playerUnity.gameManagerUnity.ReleaseCursor();
        ActiveState = State.INVENTORY;
    }

    public void ExitInventory()
    {
        playerUnity.gameManagerUnity.LockCursor();
        ActiveState = State.NORMAL;
    }

    void OnGUI() 
    {
        playerUnity.DrawUnderWaterTexture();

        if (playerUnity.gameManagerUnity.State == GameManagerUnity.GameManagerUnityState.GAME ||
            playerUnity.gameManagerUnity.State == GameManagerUnity.GameManagerUnityState.PAUSE)
            activeGUIState.Draw();
	}
}
