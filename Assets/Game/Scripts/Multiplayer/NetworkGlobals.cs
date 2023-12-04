using Fusion;
using System.Collections.Generic;
using UnityEngine;

public enum MPCheckState
{
    none,
    prepeareStartGameWait,
    handCompletedWait
}


public class NetworkGlobals : NetworkBehaviour
{
    [UnitySerializeField]
    [Networked]
    [Capacity(7)]
    public NetworkLinkedList<NetworkPlayer> orderedNetworkPlayers => default;

    //[Networked][Capacity(104)]
    //public NetworkLinkedList<byte> deckCardsByte => default;

    //[Networked]
    //public byte cardDealerInd { get; set; }

    [SerializeField] private MPCheckState mpCheckState;

    public List<int> prepeareStartGameChecks = new List<int>();
    public List<int> handCompletedCheck = new List<int>();

    public bool gotDeck;

    private GameControl gameControl;

    public override void Spawned()
    {
        gameControl = FindObjectOfType<GameControl>();
        gameControl.networkGlobals = this;


        Setup();
    }

    public void Setup()
    {
        // Oturma düzenini belirle

        //gameControl.myNetworkPlayer.completedHandCardsByte.Clear();

        if (gameControl.networkHandler.isHost)
        {
            // ResetCheckValues
            prepeareStartGameChecks.Clear();
            handCompletedCheck.Clear();
            //

            // Aktorlerin oturma düzeni
            NetworkPlayer[] networkPlayers = FindObjectsOfType<NetworkPlayer>();
            orderedNetworkPlayers.Clear();

            for (int i = 0; i < networkPlayers.Length; i++)
            {
                if (networkPlayers[i].localPlayer)
                    orderedNetworkPlayers.Add(networkPlayers[i]);
            }
            for (int i = 0; i < networkPlayers.Length; i++)
            {
                if (!networkPlayers[i].localPlayer)
                    orderedNetworkPlayers.Add(networkPlayers[i]);
            }
            //--------

            gameControl.SetMultiplayerActors();

            // Decki ayarla

            gameControl.CreateDeck();
            gameControl.ShuffleDeck();

            //------------

            UpdateNetworkDeckCards();

            // Oyuncuların kartlarını belirle
            gameControl.DealCardsToMultiplayerActors();

            gameControl.actorControls[0].player = true;
            for (int i = 1; i < orderedNetworkPlayers.Count; i++)
            {
                orderedNetworkPlayers[i].RPC_PlayInd((byte)i);
            }

            // Dağıtıcıyı belirle
            gameControl.ChooseRandomCardDealer();
            //cardDealerInd = (byte)gameControl.cardDealerInd;
            RPC_CardDealerInd((byte)gameControl.cardDealerInd);

            gameControl.SortOrderOfPlayActors();
            //

            //
            mpCheckState = MPCheckState.prepeareStartGameWait;

            //RPC_PrepeareStartGame();
            //
        }
    }

    private void Update()
    {
        if (mpCheckState == MPCheckState.none)
            return;

        switch (mpCheckState)
        {
            case MPCheckState.prepeareStartGameWait:

                if (prepeareStartGameChecks.Count == gameControl.actorControls.Count - 1) // host hariç
                {
                    mpCheckState = MPCheckState.handCompletedWait;
                    RPC_PrepeareStartGame();
                }

                break;

            case MPCheckState.handCompletedWait:

                if (handCompletedCheck.Count == gameControl.actorControls.Count - 1) // host hariç
                {
                    mpCheckState = MPCheckState.none;

                    if (gameControl.gameCounter == gameControl.gameLimit)
                        FindObjectOfType<HandCompletedPanel>(true).mainMenuButton.SetActive(true);
                    else
                        FindObjectOfType<HandCompletedPanel>(true).nextButton.SetActive(true);
                }

                break;
        }
    }

    public void UpdateNetworkDeckCards()
    {
        //deckCardsByte.Clear();
        //for (int i = 0; i < gameControl.deck.Count; i++)
        //{
        //    deckCardsByte.Add((byte)NetworkCardConverter.CardToInt(gameControl.deck[i]));
        //}

        RPC_Deck(NetworkCardConverter.CardsToString(gameControl.deck));
    }

    public void UpdateNetworkDeckCardsFromThrowed()
    {
        RPC_DeckFromThrowed(NetworkCardConverter.CardsToString(gameControl.deck));
    }


    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_PrepeareStartGame()
    {
        GameManager.Instance.gameStat = GameManager.GameStat.game;

        if (!gameControl.networkHandler.isHost)
        {
            //FillDeck();

            //gameControl.cardDealerInd = (int)cardDealerInd;

            gameControl.SetMultiplayerActors();

            gameControl.DealCardsToMultiplayerActors();

            gameControl.SortOrderOfPlayActors();
        }

        gameControl.PrepeareStartGame();
        gameControl.lobbyUIManager.gameObject.SetActive(false);
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_Deck(string deckString)
    {
        if (!gameControl.networkHandler.isHost)
        {
            gameControl.deck = NetworkCardConverter.StringToCards(deckString);
            gotDeck = true;
        }
    }


    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_CardDealerInd(byte ind)
    {
        if (!gameControl.networkHandler.isHost)
        {
            gameControl.cardDealerInd = (int)ind;

            // Prepeare Start Game Check
            if (gameControl.myNetworkPlayer.gotPlayInd && gotDeck)// && orderedNetworkPlayers.Count > 0)
                RPC_PrepeareStartGameCheck((byte)gameControl.myNetworkPlayer.playInd);
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_DeckFromThrowed(string deckString)
    {
        if (!gameControl.networkHandler.isHost)
        {
            //FillDeck();
            gameControl.deck = NetworkCardConverter.StringToCards(deckString);
        }

        gameControl.DeckFromThrowed();
    }
    //void FillDeck()
    //{
    //    gameControl.deck.Clear();
    //    for (int i = 0; i < deckCardsByte.Count; i++)
    //        gameControl.deck.Add(NetworkCardConverter.IntToCard((int)deckCardsByte[i]));
    //}









    // Check RPCs
    [Rpc(sources: RpcSources.Proxies, targets: RpcTargets.StateAuthority)]
    public void RPC_PrepeareStartGameCheck(byte playerInd)
    {
        prepeareStartGameChecks.Add((int)playerInd);
    }

    [Rpc(sources: RpcSources.Proxies, targets: RpcTargets.StateAuthority)]
    public void RPC_HandCompletedCheck(byte playerInd)
    {
        handCompletedCheck.Add((int)playerInd);
    }
}