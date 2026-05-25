using UnityEngine;

namespace SpeedrunMod.Utils;

internal static class PlayerUtil
{
    internal static PlayerMove ResolvePlayerMove()
    {
        GameObject playerRoot = GlobalTag.player;
        if (playerRoot != null)
        {
            PlayerMove onRoot = playerRoot.GetComponent<PlayerMove>() ?? playerRoot.GetComponentInChildren<PlayerMove>(true);
            if (onRoot != null)
            {
                return onRoot;
            }
        }

        return Object.FindObjectOfType<PlayerMove>();
    }

    internal static bool IsOnPlayerHierarchy(Collider collider, PlayerMove playerMove = null)
    {
        if (collider == null)
        {
            return false;
        }

        playerMove ??= ResolvePlayerMove();
        return playerMove != null && collider.transform.IsChildOf(playerMove.transform);
    }
}
