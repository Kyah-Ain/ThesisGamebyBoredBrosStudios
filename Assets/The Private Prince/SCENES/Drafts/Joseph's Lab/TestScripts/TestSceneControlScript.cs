using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TestSceneControlScript : MonoBehaviour
{
    [Header("UI")]
    public Image fader;

    public static TestSceneControlScript instance;

    private GameObject player;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            Debug.LogError("Player not found! Make sure the Player GameObject is tagged 'Player'.");
    }

    public static void TransitionPlayer(Vector3 pos)
    {
        Debug.Log($"=== TRANSITION START ===");
        Debug.Log($"Instance exists: {instance != null}");
        Debug.Log($"Player reference in instance: {instance.player != null}");
        Debug.Log($"Target position received: {pos}");

        Debug.Log("TransitionPlayer called with pos: " + pos);

        if (instance == null)
        {
            Debug.LogError("TestSceneControlScript instance is NULL");
            return;
        }

        instance.StartCoroutine(instance.Transition(pos));
    }

    private IEnumerator Transition(Vector3 pos)
    {
        // Fade to black
        fader.gameObject.SetActive(true);

        for (float f = 0; f < 1; f += Time.deltaTime / 0.25f)
        {
            fader.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, f));
            yield return null;
        }

        Debug.Log($"=== TELEPORT DEBUG ===");
        Debug.Log($"Player found: {player != null}");

        if (player != null)
        {
            CharacterController controller = player.GetComponentInChildren<CharacterController>();
            Debug.Log($"CharacterController found: {controller != null}");
            Debug.Log($"Teleporting from: {player.transform.position} to: {pos}");

            if (controller != null)
            {
                controller.enabled = false;
                player.transform.position = pos + Vector3.up * 0.1f;
                controller.enabled = true;
            }
            else
            {
                player.transform.position = pos;
            }
        }
        else
        {
            Debug.LogError("Player reference is NULL in TestSceneControlScript!");
        }

        // Optional pause while screen is black
        yield return new WaitForSeconds(1f);

        // Fade back in
        for (float f = 0; f < 1; f += Time.deltaTime / 0.25f)
        {
            fader.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, f));
            yield return null;
        }

        fader.gameObject.SetActive(false);
    }
}
