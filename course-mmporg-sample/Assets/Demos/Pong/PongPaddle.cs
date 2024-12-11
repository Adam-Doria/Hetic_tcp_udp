using UnityEngine;

public enum PongPlayer {
    None = 0,
    PlayerLeft = 1,
    PlayerRight = 2
}

[System.Serializable]
public class PaddleState {
    public Vector3 Position;
}

public class PongPaddle : MonoBehaviour
{ 
    public PongPlayer Player = PongPlayer.PlayerLeft;
    public float Speed = 1;
    public float MinY = -4;
    public float MaxY = 4;

}
