#region using
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using BackEnd.Tcp;
using UnityEngine.SceneManagement;
using System.Text;
using UnityEngine.UI;

#endregion
public class EventManager : MonoBehaviour
{
    public static EventManager Instance = null;

    [Header("InGameServer")]
    MatchInGameRoomInfo _roomInfo; //인게임에서 방 정보를 전달하기위해 선언해둔 변수


    #region FindObjectArea
    GroundBuyScript _theGBS;
    CardManager theCardManager;
    DiceSystem theDice;
    TileManager theTM;

    #endregion


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = FindObjectOfType(typeof(EventManager)) as EventManager;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        _theGBS = FindObjectOfType<GroundBuyScript>();
        theDice = FindObjectOfType<DiceSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        Backend.Match.OnMatchMakingRoomLeave = (MatchMakingGamerInfoInRoomEventArgs args) =>
        {
            //Todo
        };

        //매칭신청(인게임서버접속 시작)
        Backend.Match.OnMatchMakingResponse = (MatchMakingResponseEventArgs args) =>
        {
            if(args.ErrInfo == ErrorCode.Success){
                _roomInfo = args.RoomInfo; //추후에 roomToken을 써야되기 때문에 따로 저장
                    Backend.Match.JoinGameServer(args.RoomInfo.m_inGameServerEndPoint.m_address,
                    args.RoomInfo.m_inGameServerEndPoint.m_port,
                    false, out ErrorInfo errorInfo);
            }
        };

        Backend.Match.OnSessionJoinInServer += (args) =>
        { //인게임서버에 접속 성공했을 떄 호출되는 이벤트 이 이벤트가 호출되어야 서버에 접속성공한것.
            if (args.ErrInfo == ErrorInfo.Success)
            {
                Backend.Match.JoinGameRoom(this._roomInfo.m_inGameRoomToken); //OnMatchMakingResponse에서 전달받은 RoomToken을 여기로 전달.
                GameManager.Instance.nowPlayer.sessionId = args.Session.SessionId;
            }
        };

        Backend.Match.OnMatchInGameAccess = (MatchInGameSessionEventArgs args) =>
        { 
            //유저가 입장 시 호출됨
            if (args.ErrInfo == ErrorCode.Success)
            {
                AudioManager.Instance.Stop("Title_Sound");
                SceneManager.LoadScene("MainScene");
            }
        };

        Backend.Match.OnMatchInGameStart = () =>
        {
            //게임시작 이벤트 브로드캐스팅 준비 완료
            UIManager.Instance.SetUI();
        };

        Backend.Match.OnMatchRelay = (MatchRelayEventArgs args) =>
        {
            byte[] data = args.BinaryUserData;
            ParsingData pData = JsonUtility.FromJson<ParsingData>(Encoding.Default.GetString(data));
            switch (pData.type)
            {
                case ParsingType.TurnCardSet: //게임 시작 시 두 클라이언트 간 턴 선택하는 카드의 랜덤번호를 맞춰준다.
                    TurnCardSet tsData = JsonUtility.FromJson<TurnCardSet>(pData.data);
                    if (tsData.randomNum == 0)
                    {
                        GameManager.Instance.turnCards[0].GetComponent<ButtonManager>().turnNum = 1;
                        GameManager.Instance.turnCards[1].GetComponent<ButtonManager>().turnNum = 0;
                    }
                    else
                    {
                        GameManager.Instance.turnCards[0].GetComponent<ButtonManager>().turnNum = 0;
                        GameManager.Instance.turnCards[1].GetComponent<ButtonManager>().turnNum = 1;
                    }

                    break;

                case ParsingType.Turn: //턴 선택 분기
                    print("turn case");
                    TurnCard tData = JsonUtility.FromJson<TurnCard>(pData.data);
                    GameManager.Instance.playerCount.Add(1);
                    GameManager.Instance.turnCards[tData.turncardIdx].SetActive(false);
                    if (GameManager.Instance.playerCount.Count > 1)
                    {
                        GameManager.Instance.turnCardParent.SetActive(false);
                    }
                    break;

                case ParsingType.Dice: //주사위 데이터
                    print("dice type");
                    if (theDice == null) theDice = FindObjectOfType<DiceSystem>();

                    StartCoroutine(theDice.RollDiceCoroutine());
                    DiceData dData = JsonUtility.FromJson<DiceData>(pData.data);
                    GameManager.Instance.diceNum = dData.diceNum;
                    theDice.diceFlag = true;
                    break;

                case ParsingType.NextTurn: //다음턴으로 넘기기
                    GameManager.Instance.NextTurnFunc(); //이 함수로
                    GameManager.Instance.UIFlag = false;
                    break;

                case ParsingType.GroundBuy: //땅 구매
                    if (GameManager.Instance.myCharactor.myTurn)
                    {
                        GameManager.Instance.myCharactor.groundCount += 1;
                        GameManager.Instance.myCharactor.playerMoney -= 50;
                        GameManager.Instance.nowPlayer.nowTile.price = 50;
                    }
                    else
                    {
                        //상대방이 땅을 구매했을 때, 상대방 땅 색깔로 구매되었다는걸 알려줘야함.
                        GameManager.Instance.myCharactor.againstPlayer.nowTile.ownPlayer
                            = GameManager.Instance.myCharactor.againstPlayer.playerId;
                        GameManager.Instance.myCharactor.againstPlayer.groundCount += 1;
                        GameManager.Instance.myCharactor.againstPlayer.playerMoney -= 50;
                        GameManager.Instance.nowPlayer.nowTile.price = 50;
                    }
                    GameManager.Instance.SetFloatingText(GameManager.Instance.nowPlayer, 50, false);
                    break;

                case ParsingType.BuildingBuy: //건물건설
                    if (GameManager.Instance.myCharactor.myTurn)
                    {
                        // //모든 작업 완료 후 턴 넘기기
                        GameManager.Instance.NextTurnFunc();
                        GameManager.Instance.UIFlag = false;

                    }
                    else
                    {
                        BuildingData bdata = JsonUtility.FromJson<BuildingData>(pData.data);

                        GameManager.Instance.myCharactor.againstPlayer.nowTile.building =
                            GameManager.Instance.buildings[bdata.buildingNum];

                        GameManager.Instance.myCharactor.againstPlayer.buildingCount += 1;
                        GameManager.Instance.myCharactor.againstPlayer.playerMoney -= 50; //건물 건설비용
                        GameManager.Instance.myCharactor.againstPlayer.nowTile.price =
                        GameManager.Instance.buildings[bdata.buildingNum].toll;
                        GameManager.Instance.SetFloatingText(GameManager.Instance.nowPlayer, 50, false);
                        GameManager.Instance.NextTurnFunc();
                        GameManager.Instance.UIFlag = false;
                    }
                    break;

                case ParsingType.Teleport:
                    TeleportData tpData = JsonUtility.FromJson<TeleportData>(pData.data);

                    GameManager.Instance.nowPlayer.tpFlag = tpData.tpFlag; //전달받은 값을 현재 턴의 플레이어에 할당.
                    GameManager.Instance.nowPlayer.tpTile = GameObject.Find(tpData.tpTileNum); //땅이 계속 0으로만 들어감.

                    GameManager.Instance.seletedTile = null;

                    //이후 턴 넘기기.
                    GameManager.Instance.NextTurnFunc();
                    GameManager.Instance.UIFlag = false;
                    break;

                case ParsingType.Card:
                    CardData cardData = JsonUtility.FromJson<CardData>(pData.data);
                    GameManager.Instance.nowPlayer.cards.Add(cardData.card);
                    break;

                //건물파괴 타일 선택시(타일선택)
                case ParsingType.TileSelect:
                    TileSelectData tileSelectData = JsonUtility.FromJson<TileSelectData>(pData.data);
                    GameManager.Instance.seletedTile = GameObject.Find(tileSelectData.tilename);
                    break;

                //건물파괴
                case ParsingType.Extortion:
                    ExtortionData extortionData = JsonUtility.FromJson<ExtortionData>(pData.data);
                    Color tileColor = GameManager.Instance.seletedTile.GetComponent<Tile>().signImg.GetComponent<SpriteRenderer>().color;
                    StartCoroutine(ExtortionAlphaCoroutine(tileColor, extortionData.playerId));
                    break;

                case ParsingType.CardClick:
                    CardClickData cData = JsonUtility.FromJson<CardClickData>(pData.data);

                    switch (cData.cardNum)
                    {
                        case 1: //고속이동
                            GameManager.Instance.nowPlayer.highSpeedFlag = true;
                            break;

                        case 2://투명도둑
                            GameManager.Instance.nowPlayer.invisibleFlag = true;
                            break;

                        case 3://거대화 꼬꼬
                            GameManager.Instance.nowPlayer.biggerFlag = true;
                            break;

                        case 4: //소형알
                            GameManager.Instance.nowPlayer.lowerDiceFlag = true;
                            break;

                        case 5: //대형알
                            GameManager.Instance.nowPlayer.higherDiceFlag = true;
                            break;

                        // 6번 통행료 면제는 사용카드가 아니라 패시브 카드라서 패스.

                        case 7: //레이저빔
                            GameManager.Instance.nowPlayer.laserFlag = true;
                            break;
                    }
                    break;

                case ParsingType.CardListAdd:
                    CardData cardData1 = JsonUtility.FromJson<CardData>(pData.data);

                    var _card = Instantiate(GameManager.Instance.nowPlayer.cardPrefab,
                        Vector3.zero, Quaternion.identity, GameManager.Instance.nowPlayer.cardParent);
                    _card.transform.localPosition = new Vector3(0f, 0f, 0f);

                    GameManager.Instance.nowPlayer.cards.Add(cardData1.card); //카드를 뽑았다면 현재 플레이어 카드리스트에 값 추가.
                    break;

                case ParsingType.CardDestory:
                    CardDestroyData destroyData = JsonUtility.FromJson<CardDestroyData>(pData.data);
                    Destroy(destroyData.destoryCard);
                    Destroy(GameManager.Instance.nowPlayer.cardParent.GetChild(0).gameObject);
                    GameManager.Instance.nowPlayer.cards.Remove(GameManager.Instance.nowPlayer.cards.Find(card => card.cardCode == destroyData.cardCode));
                    break;

                case ParsingType.InvisibleThief: //카드 투명도둑
                    GameManager.Instance.invisibleCardNum = UnityEngine.Random.Range(0,
                        GameManager.Instance.nowPlayer.againstPlayer.cards.Count);
                    break;

                case ParsingType.ExemptionFlag: //상대방 땅에 걸린경우
                    StartCoroutine(ExemptionCoroutine());
                    break;

                case ParsingType.ExemptionFlagSet:
                    GameManager.Instance.nowPlayer.exemptionFlag = true;
                    break;

                case ParsingType.Visit:
                    VisitData visitData = JsonUtility.FromJson<VisitData>(pData.data);
                    switch (visitData.caseNum)
                    {
                        case 0: //농장
                            GameManager.Instance.nowPlayer.playerMoney += visitData.money;
                            GameManager.Instance.SetFloatingText(GameManager.Instance.nowPlayer, visitData.money, true);
                            GameManager.Instance.NextTurnFunc();
                            break;

                        case 1: //재단
                            StartCoroutine(TempleCoroutine()); //재단 활성화 되었을때 파티클 재생위해 코루틴으로 수정
                            break;

                    }

                    break;

                case ParsingType.ArriveTile: //양계장에 도착할 경우
                    StartCoroutine(ArriveCoroutine(pData)); //플로팅 텍스트 때문에 코루틴으로 뺌
                    break;

                case ParsingType.Olympic:
                    StartCoroutine(GameManager.Instance.OlympicMethod(GameManager.Instance.nowPlayer.playerId, GameManager.Instance.nowPlayer.VirtualCamera));
                    GameManager.Instance.NextTurnFunc();
                    break;

                case ParsingType.Laser:
                    LaserData laserData = JsonUtility.FromJson<LaserData>(pData.data);
                    GameManager.Instance.seletedTile = GameObject.Find(laserData.laserTileNum);
                    theCardManager = GameObject.Find("CardManager").GetComponent<CardManager>();
                    StartCoroutine(theCardManager.LaserCoroutine());
                    break;
            }
        };

        //게임 종료(정상적: 게임에서 게임오버 함수 호출, 비정상적 : 플레이어가 나감)
        Backend.Match.OnMatchResult = (MatchResultEventArgs args) =>
        {
            GameManager.Instance.gameOverUI.SetActive(true);
            //게임오버는 따로 처리하는거 없이 나가기 버튼만
        };

        //게임 중, 플레이어가 연결 끊김.
        Backend.Match.OnSessionOffline = (MatchInGameSessionEventArgs args) =>
        {
            if (args.ErrInfo == ErrorCode.NetworkOffline)
            {
                //연결 끊긴 방에서 나가기 위한 UI 출력
                UIManager.Instance.SetErrorUI();
            }
        };
    }


    //건물강탈 코루틴
    IEnumerator ExtortionAlphaCoroutine(Color tileColor, int playerId)
    {
        AudioManager.Instance.Play("Extortion_Sound");
        //여기부터 둘 다 처리되어야 하는 부분.

        // 타일의 Alpha 값을 서서히 0으로 줄임
        while (tileColor.a > 0f)
        {
            tileColor.a -= 0.02f;
            GameManager.Instance.seletedTile.GetComponent<Tile>().signImg.GetComponent<SpriteRenderer>().color = tileColor;
            yield return new WaitForSeconds(0.02f);
        }

        // ownPlayer를 바꿔서 땅의 소유주를 바꿔주고, signImg도 동시에 변하게함
        GameManager.Instance.seletedTile.GetComponent<Tile>().ownPlayer = playerId;

        // 타일의 Alpha 값을 서서히 1로 올림
        while (tileColor.a < 1f)
        {
            tileColor.a += 0.02f;
            GameManager.Instance.seletedTile.GetComponent<Tile>().signImg.GetComponent<SpriteRenderer>().color = tileColor;
            yield return new WaitForSeconds(0.02f);
        }

        GameManager.Instance.seletedTile = null;

        GameManager.Instance.NextTurnFunc();
        GameManager.Instance.UIFlag = false;
    }

    //양계장 코루틴
    IEnumerator ArriveCoroutine(ParsingData pData)
    {
        ArriveTileData arriveTileData = JsonUtility.FromJson<ArriveTileData>(pData.data);
        int totalMoney = 0;

        //타일 체크
        for (int i = 0; i < TileManager.Instance.tiles.Length; i++)
        {
            if (TileManager.Instance.tiles[i].ownPlayer == arriveTileData.playerId && TileManager.Instance.tiles[i].building.type == 0) totalMoney += 100;
        }
        GameManager.Instance.nowPlayer.playerMoney += totalMoney;
        yield return new WaitForSeconds(0.5f);
        if (totalMoney > 0)
        {
            GameManager.Instance.SetFloatingText(GameManager.Instance.nowPlayer, totalMoney, true);
        }
        GameManager.Instance.NextTurnFunc();
        GameManager.Instance.UIFlag = false;
    }

    //재단 코루틴
    IEnumerator TempleCoroutine()
    {
        AudioManager.Instance.Play("Olympics_Sound");
        GameManager.Instance.nowPlayer.nowTile.price *= 2;
        GameManager.Instance.nowPlayer.nowTile.transform.Find("Pos").GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        GameManager.Instance.nowPlayer.nowTile.transform.Find("Pos").GetChild(0).gameObject.SetActive(false);
        GameManager.Instance.NextTurnFunc();
    }

    //통행료 지불 코루틴(내 움직임이 끝날때까지 기다렸다가 징수하기 위해 코루틴 사용)
    IEnumerator ExemptionCoroutine()
    {
        yield return new WaitUntil(() => GameManager.Instance.nowPlayer.finishMoving == true); //무빙이 끝났다면 통행료 징수

        if (!GameManager.Instance.nowPlayer.exemptionFlag)
        {
            GameManager.Instance.nowPlayer.playerMoney -= GameManager.Instance.nowPlayer.nowTile.price;
            GameManager.Instance.SetFloatingText(GameManager.Instance.nowPlayer, GameManager.Instance.nowPlayer.nowTile.price, false);
            GameManager.Instance.nowPlayer.againstPlayer.playerMoney += GameManager.Instance.nowPlayer.nowTile.price;
            GameManager.Instance.SetFloatingText(GameManager.Instance.nowPlayer.againstPlayer, GameManager.Instance.nowPlayer.nowTile.price, true);

            GameManager.Instance.NextTurnFunc(); //통신으로 처리하지 않는 이유는 통신을 거치면 돈이 빠져나가기전에 두 클라이언트 모두 턴이 넘어가기 때문에
                                                 //완벽히 처리해주고 각 클라이언트의 턴을 넘기기 위함.
        }
        // 통행료 면제 카드가 있다면 통행료 징수를 하지 않음
        else
        {
            StartCoroutine(GameManager.Instance.ParticleFunc());
        }
    }
}
