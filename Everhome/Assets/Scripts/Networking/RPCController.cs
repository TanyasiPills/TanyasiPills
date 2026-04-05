using Unity.Netcode;
using UnityEngine;

public class RPCController : NetworkBehaviour
{
    AudioPlayer player;

    void Start()
    {
        GameObject manager = GameObject.FindGameObjectWithTag("Manager");
        player = manager.GetComponent<AudioPlayer>();
        GameObject.FindGameObjectWithTag("Recorder").GetComponent<AudioRecord>().rpc = this;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SendVoiceToServerRpc(byte[] data, int length, NetworkObjectReference refy)
    {
        SendVoiceToEveryoneRpc(data, length, refy);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
    private void SendVoiceToEveryoneRpc(byte[] data, int length, NetworkObjectReference refy)
    {
        if (refy.TryGet(out NetworkObject netObj))
        {
            if (netObj.OwnerClientId == NetworkManager.Singleton.LocalClientId) return;

            player.PushSample(refy, data, length);
        }
    }
}
