using System.Net;
using UnityEngine;
using System.Collections;

public class BallSyncClient : MonoBehaviour
{
    UDPService UDP;
    Vector3 targetPosition;
    Vector3 serverPosition;
    Vector3 serverDirection;
    float interpolationSpeed=5.0f;
    float serverSpeed = 1f;
    Vector3 lastServerPosition;
    float interpolationDuration = 0.1f; // ici on va choisir la durée pendant la quelle on veut que le la balle coté client et la position de la balle envoyée par le serveur se synchronise. 
    bool isInterpolating = false;

    void Awake() {
      if (Globals.IsServer) {
        enabled = false;
      }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UDP = FindFirstObjectByType<UDPService>();
        transform.position = Vector3.zero;
        serverPosition = transform.position;
        serverDirection = Vector3.right;
        serverSpeed = 1f;

        UDP.OnMessageReceived += (string message, IPEndPoint sender) => {

            if (!message.StartsWith("UPDATE")) { return; }

            string[] tokens = message.Split('|');
          if (tokens.Length >= 8) {
                float x, y, z;
                float dx, dy, dz;
                float spd;

                if (float.TryParse(tokens[1], out x) && float.TryParse(tokens[2], out y) && float.TryParse(tokens[3], out z) &&
                    float.TryParse(tokens[4], out dx) && float.TryParse(tokens[5], out dy) && float.TryParse(tokens[6], out dz) &&
                    float.TryParse(tokens[7], out spd)) {

                    lastServerPosition = serverPosition;
                    serverPosition = new Vector3(x, y, z);
                    serverDirection = new Vector3(dx, dy, dz);
                    serverSpeed = spd;

                    // on modifie la position pour que la transifition soit smooth et qu'en gros on ait pas de saut de balle dans le futur. 
                    //l'idée c'est d'appliquer une correction petit a petit jusqu'a ce que lespositions soit synchronisées
                    if (gameObject.activeInHierarchy) {
                        StopAllCoroutines();
                        StartCoroutine(SmoothCorrection(transform.position, serverPosition));
                    } else {
                        transform.position = serverPosition;
                    }
                }
            }

        };
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInterpolating) {
            transform.position += serverDirection * serverSpeed * Time.deltaTime;
        }
    }
     IEnumerator SmoothCorrection(Vector3 startPos, Vector3 endPos) {
        isInterpolating = true;
        float elapsed = 0f;
        while (elapsed < interpolationDuration) {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / interpolationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
        isInterpolating = false;
    }
}
