using UnityEngine;

public enum PongBallState {
  Playing = 0,
  PlayerLeftWin = 1,
  PlayerRightWin = 2,
}

public class PongBall : MonoBehaviour
{
    ServerManager ServerMan;
    public float Speed = 1;
    private int scoreLeft = 0;
    private int scoreRight = 0;
    Vector3 Direction;
    PongBallState _State = PongBallState.Playing;
    
    public PongBallState State {
      get {
        return _State;
      }
    } 

    void Awake() {
      if (!Globals.IsServer) {
        enabled = false;
      }

    }

    void Start() {
      ServerMan = FindObjectOfType<ServerManager>();
      InitBall();
    }
    void InitBall() {
      transform.position = Vector3.zero;
      Direction = new Vector3(
          Random.Range(0.5f, 1),
          Random.Range(-0.5f, 0.5f),
          0
        );
        Direction.x *= Mathf.Sign(Random.Range(-100, 100));
        Direction.Normalize();
    }

    void Update() {
      if (State != PongBallState.Playing) {
        return;
      }

      transform.position = transform.position + (Direction * Speed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision c) {
      switch (c.collider.name) {
        case "BoundTop":
        case "BoundBottom":
          Direction.y = -Direction.y;
          SendBallPositionUpdate();
          break;

        case "PaddleLeft":
        case "PaddleRight":
          Direction.x = -Direction.x;
          SendBallPositionUpdate();
          break;

        case "BoundLeft":
        scoreRight++;
        UpdateScore();
        SendBallPositionUpdate();
          break;

        case "BoundRight":
        scoreLeft++;
        UpdateScore();
        SendBallPositionUpdate();
          break;
        

      }
    }
    public void SetState(PongBallState newState) {
        _State = newState;
    }
   void UpdateScore() {
        Debug.Log($"[SERVER] Sending SCORE_UPDATE message: {scoreLeft}|{scoreRight}");
        ServerMan.BroadcastUDPMessage($"SCORE_UPDATE|{scoreLeft}|{scoreRight}");

        if (scoreLeft >= 5) {
            EndGame(PongPlayer.PlayerLeft);
        } else if (scoreRight >= 5) {
            EndGame(PongPlayer.PlayerRight);
        } else {       
        InitBall();
        }
    }

    void EndGame(PongPlayer winner) {
        _State = (winner == PongPlayer.PlayerLeft) ? PongBallState.PlayerLeftWin : PongBallState.PlayerRightWin;
        ServerMan.BroadcastUDPMessage($"GAME_OVER|{winner}");
        Speed = 0;
        Direction = Vector3.zero;
    }

     public Vector3 GetDirection() {
        return Direction;
    }
    public void SendBallPositionUpdate() {
      Vector3 position = transform.position;
      ServerMan.BroadcastUDPMessage($"UPDATE|{position.x}|{position.y}|{position.z}|{Direction.x}|{Direction.y}|{Direction.z}|{Speed}");
    }

 
}
