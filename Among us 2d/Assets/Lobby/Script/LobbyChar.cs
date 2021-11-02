﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public class LobbyChar : MonoBehaviourPunCallbacks, IPunObservable
{
    private Animator animator;

    static public bool isMoveable;
    public float speed = 2f;

    private SpriteRenderer spriteRender;

    public EPlayerColor playerColor;

    [SerializeField]
    private Text nicknameText;
    private string nickname;

    public void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        // 플레이어 색상 설정
        spriteRender = GetComponent<SpriteRenderer>();
        spriteRender.material.SetColor("_PlayerColor", PlayerColor.GetColor(playerColor));

        isMoveable = true;
        animator = GetComponent<Animator>();

        if (photonView.IsMine)
        {
            Camera cam = Camera.main;
            cam.transform.SetParent(transform);
            cam.transform.localPosition = new Vector3(0f, 0f, -10f);
            cam.orthographicSize = 2.5f;
        }

        // 플레이어 닉네임 설정
        photonView.RPC("SetNickName", RpcTarget.AllBuffered);

        // LobbyManger RPC
        if (LobbyManger.Instance != null)
        {
            LobbyManger.Instance.GetComponent<PhotonView>().RPC("UpdatePlayerCount", RpcTarget.All);
            LobbyManger.Instance.GetComponent<PhotonView>().RPC("SetStartButton", RpcTarget.All);
        }
    }

    // 씬 이동 후 호출
    private void OnLevelWasLoaded()
    {
        isMoveable = true;
        
        if (photonView.IsMine)
        {
            Destroy(transform.Find("Main Camera").gameObject);
            Camera cam = Camera.main;
            cam.transform.SetParent(transform);
            cam.transform.localPosition = new Vector3(0f, 0f, -10f);
            cam.orthographicSize = 2.5f;
        }
    }

    private void OnDestroy()
    {
        /*if (LobbyManger.Instance != null)
        {
            LobbyManger.Instance.GetComponent<PhotonView>().RPC("UpdatePlayerCount", RpcTarget.All);
            LobbyManger.Instance.GetComponent<PhotonView>().RPC("SetStartButton", RpcTarget.All);
        }*/
    }

    void Update()
    {
        // UI 버튼을 클릭시 캐릭터 움직임 x
        if ((Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject()))
        {
            isMoveable = false;
        }

        if (photonView.IsMine && isMoveable)
        {
            bool isMove = false;
            if (Setting.controlType == EcontrolType.KeyboardMouse)
            {
                Vector3 dir = Vector3.ClampMagnitude(new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f), 1f);
                if (dir.x < 0f) transform.localScale = new Vector3(-0.5f, 0.5f, 1f);
                else if (dir.x > 0f) transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                transform.position += dir * speed * Time.deltaTime;
                isMove = dir.magnitude != 0f;
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    Vector3 dir = (Input.mousePosition - new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f)).normalized;
                    if (dir.x < 0f) transform.localScale = new Vector3(-0.5f, 0.5f, 1f);
                    else if (dir.x > 0f) transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                    transform.position += dir * speed * Time.deltaTime;
                    isMove = dir.magnitude != 0f;
                }
            }
            // 애니메이터 파라미터
            animator.SetBool("IsMove", isMove);
        }

        if (transform.localScale.x < 0)
        {
            nicknameText.transform.localScale = new Vector3(-0.3f, 0.3f, 1f);
        }
        else if (transform.localScale.x > 0)
        {
            nicknameText.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
        }
    }

    [PunRPC]
    public void SetNickName()
    {
        nickname = photonView.Owner.NickName;
        nicknameText.text = nickname;
    }

    [PunRPC]
    public void setColor(EPlayerColor color)
    {
        playerColor = color;
        if (spriteRender == null)
        {
            spriteRender = GetComponent<SpriteRenderer>();
        }
        spriteRender.material.SetColor("_PlayerColor", PlayerColor.GetColor(playerColor));
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(playerColor);
        }
        else
        {
            playerColor = (EPlayerColor)stream.ReceiveNext();
        }
    }
}
