using UnityEngine;
using Fusion;
using GNW2.GameManager;
using GNW2.Player;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
public class GameHandler : NetworkBehaviour
{
    public static GameHandler Instance;

    [SerializeField] private GameObject selectionUI;
    [SerializeField] private GameObject WinUI;
    [SerializeField] private GameObject LostUI;

    [SerializeField] private Button Rock;
    [SerializeField] private Button Paper;
    [SerializeField] private Button Scissor;

    private List<PlayerTurn> playerTurn = new();


    struct PlayerTurn
    {
        public PlayerRef player;
        public int PlayerSelection;
    }


    bool hasShowedUI;

    public bool HasGameStarted;


    public override void Spawned()
    {
        base.Spawned();
        if (Instance == null)
        {
            Instance = this;
        }

        if (Rock != null)
        {
            Rock.onClick.AddListener(SelectRock);
        }

        if (Paper != null)
        {
            Paper.onClick.AddListener(SelectPaper);
        }
        
        if(Scissor != null)
        {
            Scissor.onClick.AddListener(SelectScissor);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (Object.HasStateAuthority)
        {

            if (GameManager.Instance.activePlayers.Count >= 2)
                HasGameStarted = true;
        }
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowUI()
    {
        selectionUI.SetActive(true);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_HideUI()
    {
        selectionUI.SetActive(false);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SendTurn(int type, PlayerRef player)
    {
        playerTurn.Add(new PlayerTurn
        {
            PlayerSelection = type,
            player = player
        });
        if(playerTurn.Count == 2)
        {
            Evaluate();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowResults([RpcTarget] PlayerRef player, NetworkBool isWin)
    {
        if (isWin)
        {
            WinUI.SetActive(true);
        }
        else
        {
            LostUI.SetActive(false);
        }
    }

    private void SelectRock()
    {

        RPC_SendTurn(0);
    }

    private void SelectPaper()
    {
RPC_SendTurn(1);
    }

    private void SelectScissor()
    {
        RPC_SendTurn(2);
    }
    
    
    private void Evaluate()
    {
        var p1result = playerTurn[0];
        var p2result = playerTurn[1];

        switch (p1result.PlayerSelection)
        {
            case 0:
                if (p2result.PlayerSelection == 1)
                {
                    RPC_ShowResults(p1result.player, false);
                    RPC_ShowResults(p2result.player, true);
                }
                else
                {
                    RPC_ShowResults(p1result.player, false);
                    RPC_ShowResults(p2result.player, false);
                }
                break;
            case 1:
                if (p2result.PlayerSelection == 2)
                {
                    RPC_ShowResults(p1result.player, false);
                    RPC_ShowResults(p2result.player, true);
                }
                else
                {
                    RPC_ShowResults(p1result.player, false);
                    RPC_ShowResults(p2result.player, false);
                }
                break;
            case 2:
                if (p2result.PlayerSelection == 0)
                {
                    RPC_ShowResults(p1result.player, false);
                    RPC_ShowResults(p2result.player, true);
                }
                else
                {
                    RPC_ShowResults(p1result.player, false);
                    RPC_ShowResults(p2result.player, false);
                }
                break;
            default:
                break;
        }
        

    }


}
