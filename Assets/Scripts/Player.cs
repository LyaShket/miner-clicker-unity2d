﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    //===========================
    // [SerializeField] & public
    //===========================
    [Header("Player Characteristics")]
    [SerializeField] float horizontalSpeed = 20f;
    [SerializeField] float jumpForce = 180f;

    public static float miningDelay = 2.4f;
    public static float climbingSpeed = 4f;
    public static float minPower = 1f;
    public static float maxPower = 3f;
    public static float minAutoPower = 1f;
    public static float maxAutoPower = 3f;

    [Header("UI")]
    [SerializeField] Text moneyText;
    [SerializeField] Text depthText;

    [Header("Others")]
    [SerializeField] GameObject stairsStart;
    [SerializeField] GameObject stairs;

    //=========
    // private
    //=========
    private Rigidbody2D rb;

    // states
    private bool isMining = false;
    private bool isOnStairs = false;
    private bool isOnStartStairs = false;
    private bool isClimbing = false;

    private float xSpeed;

    //=======================
    // player move and doing
    //=======================
    // left move
    public void OnDownLeft()
	{
        StartCoroutine("MoveLeft");
    }
    public void OnUpLeft()
	{
        StopCoroutine("MoveLeft");
        rb.velocity = new Vector2(0, rb.velocity.y);
    }
    IEnumerator MoveLeft()
	{
        while (true)
        {
            rb.AddForce(Vector2.left * xSpeed);
            yield return new WaitForEndOfFrame();
        }
	}
    // right move
    public void OnDownRight()
	{
        StartCoroutine("MoveRight");
    }
    public void OnUpRight()
	{
        StopCoroutine("MoveRight");
        rb.velocity = new Vector2(0, rb.velocity.y);
    }
    IEnumerator MoveRight()
	{
        while (true)
		{
            rb.AddForce(Vector2.right * xSpeed);
            yield return new WaitForEndOfFrame();
        }
	}
    // jump
    public void OnDownJump()
	{
        if (rb.velocity.y == 0 && !isMining && !isClimbing)
            rb.AddForce(new Vector2(0, jumpForce));
    }
    // climb
    public void OnDownClimb()
    {
        isClimbing = !isClimbing;
        if (!isOnStairs && isClimbing)
        {
            isClimbing = false;
        }
        if (isMining == true)
        {
            isMining = false;
            StopCoroutine("AutoMine");
        }
    }
    // automine
    public void OnDownAutomine()
	{
        if (isMining)
        {
            isMining = false;
            StopCoroutine("AutoMine");
        }
        else
        {
            isMining = true;
            isClimbing = false;
            StartCoroutine("AutoMine");
        }
    }
    //========
    // stairs
    //========
    public void OnStairsStay()
    {
        isOnStairs = true;
    }
    public void OnStairsExit()
    {
        if (!isMining)
            isOnStairs = false;
    }
    public void OnStartStairsStay()
    {
        isOnStartStairs = true;
        if (!isMining && isOnStairs && isClimbing)
        {
            rb.MovePosition(rb.position + new Vector2(-0.64f, 0.64f));
            isClimbing = false;
        }
    }
    public void OnStartStairsExit()
    {
        isOnStartStairs = false;
    }
    private void Start()
	{
        rb = GetComponent<Rigidbody2D>();
        LoadData();
    }
    private void Update()
    {
        // get & set depth on UI panel
        int depth = Utils.GetDepth(gameObject);
        depthText.text = $"Depth: {depth}";
        
        // if is climbing
        if (!isMining && isOnStairs && isClimbing)
        {
            rb.MovePosition(rb.position + Vector2.up * climbingSpeed * Time.deltaTime);
            rb.velocity = Vector2.zero;
        }
        // if is not climbing
        else if (!isMining)
        {
            // if player is jumping, horizontal speed will be reduced in 4 times
            xSpeed = rb.velocity.y == 0 ? horizontalSpeed : horizontalSpeed / 4f;

            // left move (Left Arrow)
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                OnDownLeft();
            if (Input.GetKeyUp(KeyCode.LeftArrow))
                OnUpLeft(); ;

            // right move (Right Arrow)
            if (Input.GetKeyDown(KeyCode.RightArrow))
                OnDownRight();
            if (Input.GetKeyUp(KeyCode.RightArrow))
                OnUpRight();

            // jump (Up Arrow)
            if (Input.GetKeyDown(KeyCode.UpArrow))
                OnDownJump();
        }

        // limit velocity
        if (rb.velocity.x > 2f)
            rb.velocity = new Vector2(2f, rb.velocity.y);
        else if (rb.velocity.x < -2f)
            rb.velocity = new Vector2(-2f, rb.velocity.y);
        if (rb.velocity.y < -8f)
            rb.velocity = new Vector2(rb.velocity.x, -8f);

        // mine (Left Click or M)
        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.M))
            StartCoroutine("Mine");

        // automine (Space)
        if (Input.GetKeyDown(KeyCode.Space))
            OnDownAutomine();

        // climbe (C)
        if (Input.GetKeyDown(KeyCode.C))
            OnDownClimb();
        
        // developer mode (D)
        if (Input.GetKeyDown(KeyCode.D))
        {
            miningDelay = 1f;
            climbingSpeed = 20f;
            minPower = 2000f;
            maxPower = 4000f;
            minAutoPower = 2000f;
            maxAutoPower = 4000f;
        }
    }
    private GameObject GetBlockUnderPlayer()
    {
        Vector2 raycastOrigin = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, -Vector2.up, 0.5f); // block's height is 0.5f

        // if no block is under player
        if (hit.collider == null)
            return null;

        return hit.collider.gameObject;
    }
    private void HitBlock(GameObject block)
	{
        Block blockComponent = block.GetComponent<Block>();
        float damage = (float)System.Math.Round(Random.Range(minPower, maxPower), 1);
        float strength = blockComponent.Hit(damage);

        if (strength <= 0)
        {
            // calucate stairs position
            Vector3 stairsPos = block.transform.position;
            stairsPos.z = 1;

            // calculate player position
            Vector3 playerPos = block.transform.position;
            playerPos.y -= 0.02f; // player's height is 0.46f, block's is 0.5f, (0.5f - 0.46f) / 2 = 0.02f

            // update money (add money for block)
            GameManager.UpdateMoney(blockComponent.money);

            // destroy block
            blockComponent.BreakBlock();

            // teleport player to new player position
            transform.position = playerPos;

            // create and save stairs
            GameObject stairsInst = Instantiate(stairs, stairsPos, Quaternion.identity, block.transform.parent);
            Chunk blockCollider = block.transform.parent.transform.parent.GetComponent<Chunk>();
            blockCollider.SaveStairs(stairsInst);
        }
    }
    private void AlignPlayerWithGrid()
	{
        Vector2 playerPos = transform.position;
        playerPos.x /= 0.64f;
        playerPos.x = (float)System.Math.Round(playerPos.x, System.MidpointRounding.AwayFromZero);
        playerPos.x *= 0.64f;
        playerPos.y -= 0.02f;
        transform.position = playerPos;
    }
    IEnumerator Mine()
    {
        // if player is mining
        if (isMining & (isOnStairs || isOnStartStairs))
        {
            GameObject block = GetBlockUnderPlayer();
            if (block != null)
                HitBlock(block);
        }
        yield return new WaitForEndOfFrame();
    }
    IEnumerator AutoMine()
	{
        // while player is not staying
        while (rb.velocity != Vector2.zero)
        {
            yield return new WaitForEndOfFrame();
        }
        
        AlignPlayerWithGrid();
        yield return new WaitForEndOfFrame();

        // if player is not on stairs
        if (!isOnStairs && !isOnStartStairs)
        {
            GameObject block = GetBlockUnderPlayer();

            // calculate start stairs position
            Vector3 stairsPos = block.transform.position;
            stairsPos.y += 0.64f;
            stairsPos.z = 1;

            // get parents
            GameObject blocks = block.transform.parent.gameObject; // Blocks is block's parent
            GameObject chunk = blocks.transform.parent.gameObject; // chunk is Block's parent

            // create and save start stairs
            GameObject startStairsInst = Instantiate(stairsStart, stairsPos, Quaternion.identity, blocks.transform);
            Chunk blockCollider = chunk.GetComponent<Chunk>();
            blockCollider.SaveStartStairs(startStairsInst);
        }

        // automining cycle
        while (true)
        {
            yield return new WaitForSeconds(miningDelay);
            StartCoroutine("Mine");
        }
    }
    private void LoadData()
    {
        // position
        transform.position = Load.GetVec3("player", transform.position);

        // isMining
        if (PlayerPrefs.HasKey("isMining"))
            isMining = PlayerPrefs.GetInt("isMining") == 1;

        // isOnStairs
        if (PlayerPrefs.HasKey("isOnStairs"))
            isOnStairs = PlayerPrefs.GetInt("isOnStairs") == 1;

        // isOnStartStairs
        if (PlayerPrefs.HasKey("isOnStartStairs"))
            isOnStartStairs = PlayerPrefs.GetInt("isOnStartStairs") == 1;

        // isClimbing
        if (PlayerPrefs.HasKey("isClimbing"))
            isClimbing = PlayerPrefs.GetInt("isClimbing") == 1;
        if (isMining && (isOnStairs || isOnStartStairs))
        {
            isMining = true;
            isClimbing = false;
            StartCoroutine("AutoMine");
        }
    }
    private void SaveData()
    {
        // position
        Save.SetVec3("player", transform.position);
        
        // isMining
        if (isMining)
            PlayerPrefs.SetInt("isMining", 1);
        else
            PlayerPrefs.SetInt("isMining", 0);

        // isOnStairs
        if (isOnStairs)
            PlayerPrefs.SetInt("isOnStairs", 1);
        else
            PlayerPrefs.SetInt("isOnStairs", 0);

        // isOnStartStairs
        if (isOnStartStairs)
            PlayerPrefs.SetInt("isOnStartStairs", 1);
        else
            PlayerPrefs.SetInt("isOnStartStairs", 0);
        
        // isClimbing
        if (isClimbing)
            PlayerPrefs.SetInt("isClimbing", 1);
        else
            PlayerPrefs.SetInt("isClimbing", 0);

        // money
        PlayerPrefs.SetInt("money", SaveScript.money);
    }
	private void OnApplicationPause(bool pause)
	{
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            SaveData();
    }
	private void OnApplicationQuit()
	{
        SaveData();
    }
}
