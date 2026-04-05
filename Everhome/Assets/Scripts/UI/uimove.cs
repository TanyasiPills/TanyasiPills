using System.Collections;
using UnityEngine;

public class uimove : MonoBehaviour
{
    public Transform main;
    public Transform settings;
    public Transform lobby;

    public float settingsOffset = 0f;
    public float lobbyOffset = 0f;

    bool InSettings = false;
    bool InLobby = false;

    public void Settings()
    {
        StartCoroutine(MoveToSettings());
    }

    private IEnumerator MoveToSettings()
    {
        Vector3 displacement = new Vector3(settingsOffset * (InSettings ? -1 : 1), 0, 0);

        Vector3 settingsStart = settings.localPosition;
        Vector3 settingsTarget = settingsStart + displacement;

        Vector3 mainStart = main.localPosition;
        Vector3 mainTarget = mainStart + displacement;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 2f;

            settings.localPosition = Vector3.Lerp(settingsStart, settingsTarget, t);
            main.localPosition = Vector3.Lerp(mainStart, mainTarget, t);

            yield return null;
        }

        InSettings = !InSettings;
    }

    public void Lobby()
    {
        StartCoroutine(MoveToLobby());
    }

    private IEnumerator MoveToLobby()
    {
        Vector3 displacement = new Vector3(0, lobbyOffset * (InLobby ? -1 : 1), 0);

        Vector3 lobbyStart = lobby.localPosition;
        Vector3 lobbyTarget = lobbyStart + displacement;

        Vector3 mainStart = main.localPosition;
        Vector3 mainTarget = mainStart + displacement;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 2f;

            lobby.localPosition = Vector3.Lerp(lobbyStart, lobbyTarget, t);
            main.localPosition = Vector3.Lerp(mainStart, mainTarget, t);

            yield return null;
        }

        InLobby = !InLobby;
    }
}
