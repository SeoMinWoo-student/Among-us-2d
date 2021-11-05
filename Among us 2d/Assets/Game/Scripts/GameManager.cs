﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{
    // 싱글톤
    public static GameManager Instance
    {
        get
        {
            if(m_instance == null)
            {
                m_instance = FindObjectOfType<GameManager>();
            }

            return m_instance;
        }
    }

    public static GameManager m_instance;   // 싱글톤 변수

    // 임포스터 수
    public int imposters;
    public int playerCount;    // 현재 플레이어 수

    public List<GameObject> PlayerList = new List<GameObject>();    // 플레이어들의 리스트

    [SerializeField]
    private Transform GameCharSpawnPoz;

    private void Awake()
    {
        // 싱글톤
        if (Instance != this)
        {
            Destroy(gameObject);
        }

        // 현재 room의 커스텀프로퍼티를 가져오기
        Hashtable CP = PhotonNetwork.CurrentRoom.CustomProperties;

        // room에 저장되어 있는 임포스터 수
        imposters = (int)CP["imposter"];
        playerCount = PhotonNetwork.CurrentRoom.PlayerCount;    // 현재 플레이어 수 값 가져오기
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            StartCoroutine("GameReady");        // GameReady 코루틴 시작
        }
    }

    // 플레이어들 리스트 추가
    public void AddPlayers(GameObject crew)
    {
        if (!PlayerList.Contains(crew))
        {
            PlayerList.Add(crew);           // 플레이어 추가
        }
    }

    public IEnumerator GameReady()
    {
        // 플레이어 수가 같아질 때 까지 update 한다.
        while (PlayerList.Count != playerCount)
        {
            yield return null;
        }

        ChooseImposters();      // 임포스터 정하는 함수
        SpawnGameChar();        // 플레이어들의 스폰 위치

        yield return new WaitForSeconds(0.5f);

        photonView.RPC("RpcLoading",RpcTarget.All);         // 모든 클라이언트의 로딩 시작
    }

    [PunRPC]
    private void RpcLoading()
    {
        StartCoroutine(LoadingUI.instance.Loading());       // LoadingUI 코루틴 시작
    }

    // 임포스터 선택 기능
    public void ChooseImposters()
    {
        // 임포스터 정하기
        int imp = imposters;
        while (imp > 0)
        {
            int ran = Random.Range(0, playerCount);
            if (PlayerList[ran].GetComponent<LobbyChar>().playerType == PlayerType.Crew)
            {
                PlayerList[ran].GetComponent<PhotonView>().RPC("SetPlayerType", RpcTarget.All, PlayerType.imposter);
                imp--;
            }
        }
    }

    // 플레이어들 스폰 기능
    public void SpawnGameChar()
    {
        int distance = 1;
        Vector3 spawnPos;

        for (int i = 0; i < playerCount; i++)
        {
            float angle = (2f * Mathf.PI) / playerCount;      // 각도 생성
            angle *= i;
            spawnPos = GameCharSpawnPoz.position + (new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * distance);
            PlayerList[i].GetComponent<PhotonView>().RPC("setPoz", RpcTarget.All, spawnPos);
        }
    }

    [ContextMenu("정보")]
    void Info()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("==============방 정보==============");
            Debug.Log("임포스터 수는 : " + imposters);
            print("현재 방 인원수 : " + PhotonNetwork.CurrentRoom.PlayerCount);
            print("현재 방 최대인원수 : " + PhotonNetwork.CurrentRoom.MaxPlayers);

            string playerStr = "방에 있는 플레이어 목록 : ";
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                playerStr += PhotonNetwork.PlayerList[i].NickName + ", ";
            }
            print(playerStr);
        }
    }
}
