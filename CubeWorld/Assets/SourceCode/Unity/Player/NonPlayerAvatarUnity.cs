using UnityEngine;
using System.Collections;
using CubeWorld.Tiles;
using CubeWorld.World.Objects;

public class NonPlayerAvatarUnity : AvatarUnity
{
    private bool firstUpdate = true;

    public override void Update()
    {
        base.Update();

        if (gameManagerUnity.State == GameManagerUnity.GameManagerUnityState.GAME ||
            gameManagerUnity.State == GameManagerUnity.GameManagerUnityState.PAUSE)
            UpdateAvatarPosition();
    }

    private Vector3 lastRotation;

    private void UpdateAvatarPosition()
    {
        if (firstUpdate ||
            transform.position != GraphicsUnity.CubeWorldVector3ToVector3(avatar.position) ||
            lastRotation != GraphicsUnity.CubeWorldVector3ToVector3(avatar.rotation))
        {
            firstUpdate = false;
            transform.position = GraphicsUnity.CubeWorldVector3ToVector3(avatar.position);
            transform.localRotation = Quaternion.Euler(0, avatar.rotation.y, 0);

            lastRotation = GraphicsUnity.CubeWorldVector3ToVector3(avatar.rotation);
        }
    }
}
