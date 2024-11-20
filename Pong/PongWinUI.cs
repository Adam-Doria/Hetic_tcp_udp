using UnityEngine;
using UnityEngine.SceneManagement;

public class PongWinUI : MonoBehaviour
{
  PongNetworkManager networkManager;

  void Start()
  {
    networkManager = PongNetworkManager.Instance;
  }

  public void OnReplay()
  {
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
  }
}
