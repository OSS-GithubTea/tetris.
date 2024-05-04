﻿using UnityEngine;

public class Stage : MonoBehaviour
{
    [Header("Editor Objects")]
    public GameObject tilePrefab;
    public Transform backgroundNode;
    public Transform boardNode;
    public Transform tetrominoNode;
    public Transform tetrominoqueue;
    public Transform next;
    public Transform hold;
    public GameObject gameoverPanel;
    int changenum = 0;
    int holdnum = 0;
    int holdnum2 = 0;
    int holdnum3 = 0;
    bool dohold = false;
    int[] numbers = {0, 1, 2, 3, 4, 5, 6}; 
    int[] nextnumbers = {0, 1, 2, 3};

    [Header("Game Settings")]
    [Range(4, 40)]
    public int boardWidth = 40;
    [Range(5, 20)]
    public int boardHeight = 20;
    public float fallCycle = 1.0f;

    private int halfWidth;
    private int halfHeight;

    private float nextFallTime;

    private void Start()
    {
      
        gameoverPanel.SetActive(false);

        halfWidth = Mathf.RoundToInt(boardWidth * 0.5f);
        halfHeight = Mathf.RoundToInt(boardHeight * 0.5f);

        nextFallTime = Time.time + fallCycle;

        CreateBackground();

        for (int i = 0; i < boardHeight; ++i)
        {
            var col = new GameObject((boardHeight - i - 1).ToString());
            col.transform.position = new Vector3(0, halfHeight - i, 0);
            col.transform.parent = boardNode;
        }

        numsupple();

        for (int i = 0; i < 4; ++i)
        {
            numaddd();
        }


;       
        CreateTetromino();
        
        



    }

    void Update()
    {



        if (gameoverPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
        
            Vector3 moveDir = Vector3.zero;
            bool isRotate = false;

             if (Input.GetKeyDown(KeyCode.A))
            {
                if(dohold == false){
                    minohold();
                }
                
            }
           
            
        
            


            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
        // 왼쪽으로 이동
            
                moveDir.x = -1;
            }

            


            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                moveDir.x = 1;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                isRotate = true;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                moveDir.y = -1;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                while (MoveTetromino(Vector3.down, false))
                {
                }
            }

            // 아래로 떨어지는 경우는 강제로 이동시킵니다.
            if (Time.time > nextFallTime)
            {
                nextFallTime = Time.time + fallCycle;
                moveDir = Vector3.down;
                isRotate = false;
            }

            if (moveDir != Vector3.zero || isRotate)
            {
                MoveTetromino(moveDir, isRotate);
            }
        
        
    }

    bool MoveTetromino(Vector3 moveDir, bool isRotate)
    {
        Vector3 oldPos = tetrominoNode.transform.position;
        Quaternion oldRot = tetrominoNode.transform.rotation;

        tetrominoNode.transform.position += moveDir;
        if (isRotate)
        {
            tetrominoNode.transform.rotation *= Quaternion.Euler(0, 0, 90);
        }

        if (!CanMoveTo(tetrominoNode))
        {
            tetrominoNode.transform.position = oldPos;
            tetrominoNode.transform.rotation = oldRot;

            if ((int)moveDir.y == -1 && (int)moveDir.x == 0 && isRotate == false)
            {
                AddToBoard(tetrominoNode);
                CheckBoardColumn();
                CreateTetromino();
                dohold = false;
                if (!CanMoveTo(tetrominoNode))
                {
                    gameoverPanel.SetActive(true);
                }
            }

            return false;
        }

        return true;
    }

    // 테트로미노를 보드에 추가
    void AddToBoard(Transform root)
    {
        while (root.childCount > 0)
        {
            var node = root.GetChild(0);

            int x = Mathf.RoundToInt(node.transform.position.x + halfWidth);
            int y = Mathf.RoundToInt(node.transform.position.y + halfHeight - 1);

            node.parent = boardNode.Find(y.ToString());
            node.name = x.ToString();
        }
    }

    // 보드에 완성된 행이 있으면 삭제
    void CheckBoardColumn()
    {
        bool isCleared = false;

        // 완성된 행 == 행의 자식 갯수가 가로 크기
        foreach (Transform column in boardNode)
        {
            if (column.childCount == boardWidth)
            {
                foreach (Transform tile in column)
                {
                    Destroy(tile.gameObject);
                }

                column.DetachChildren();
                isCleared = true;
            }
        }

        // 비어 있는 행이 존재하면 아래로 당기기
        if (isCleared)
        {
            for (int i = 1; i < boardNode.childCount; ++i)
            {
                var column = boardNode.Find(i.ToString());

                // 이미 비어 있는 행은 무시
                if (column.childCount == 0)
                    continue;

                int emptyCol = 0;
                int j = i - 1;
                while (j >= 0)
                {
                    if (boardNode.Find(j.ToString()).childCount == 0)
                    {
                        emptyCol++;
                    }
                    j--;
                }

                if (emptyCol > 0)
                {
                    var targetColumn = boardNode.Find((i - emptyCol).ToString());

                    while (column.childCount > 0)
                    {
                        Transform tile = column.GetChild(0);
                        tile.parent = targetColumn;
                        tile.transform.position += new Vector3(0, -emptyCol, 0);
                    }
                    column.DetachChildren();
                }
            }
        }
    }

    // 이동 가능한지 체크
    bool CanMoveTo(Transform root)
    {
        for (int i = 0; i < root.childCount; ++i)
        {
            var node = root.GetChild(i);
            int x = Mathf.RoundToInt(node.transform.position.x + halfWidth);
            int y = Mathf.RoundToInt(node.transform.position.y + halfHeight - 1);

            if (x < 0 || x > boardWidth - 1)
                return false;

            if (y < 0)
                return false;

            var column = boardNode.Find(y.ToString());

            if (column != null && column.Find(x.ToString()) != null)
                return false;
        }

        return true;
    }

    // 타일 생성
    Tile CreateTile(Transform parent, Vector2 position, Color color, int order = 1)
    {
        var go = Instantiate(tilePrefab);
        go.transform.parent = parent;
        go.transform.localPosition = position;

        var tile = go.GetComponent<Tile>();
        tile.color = color;
        tile.sortingOrder = order;

        return tile;
    }

    // 배경 타일을 생성
    void CreateBackground()
    {
        Color color = Color.gray;

        // 타일 보드
        color.a = 0.5f;
        for (int x = -halfWidth; x < halfWidth; ++x)
        {
            for (int y = halfHeight; y > -halfHeight; --y)
            {
                CreateTile(backgroundNode, new Vector2(x, y), color, 0);
            }
        }

        //홀드와 넥스트 타일 생성
        for (int x = halfWidth+1; x < halfWidth+5; ++x)
        {
            for (int y = halfHeight-1; y > -halfHeight; --y)
            {
                CreateTile(backgroundNode, new Vector2(x, y), color, 0);
            }
        }

        for (int x = -halfWidth-2; x > -halfWidth-6; --x)
        {
            for (int y = halfHeight-1; y > halfHeight-5; --y)
            {
                CreateTile(backgroundNode, new Vector2(x, y), color, 0);
            }
        }

        // 좌우 테두리
        color.a = 1.0f;
        for (int y = halfHeight; y > -halfHeight; --y)
        {
            CreateTile(backgroundNode, new Vector2(-halfWidth - 1, y), color, 0);
            CreateTile(backgroundNode, new Vector2(halfWidth, y), color, 0);
            CreateTile(backgroundNode, new Vector2(halfWidth+5, y), color, 0); //우측 바깥
        }

        // 아래 테두리
        for (int x = -halfWidth - 1; x <= halfWidth+5; ++x)
        {
            CreateTile(backgroundNode, new Vector2(x, -halfHeight), color, 0);
        }

         for (int x = -halfWidth-2; x > -halfWidth-7; --x) //좌측 바깥 상하
        {
            CreateTile(backgroundNode, new Vector2(x, halfHeight), color, 0);
            CreateTile(backgroundNode, new Vector2(x, halfHeight-5), color, 0);
        }

        for (int x = halfWidth+1; x < halfWidth+5; ++x)//우측 바깥 가운데
        {
            for(int y = 0; y<4; y++){
                CreateTile(backgroundNode, new Vector2(x, halfHeight-y*5), color, 0);
            }
        }

        for (int y = halfHeight-1; y > halfHeight-5; --y)
        {
            CreateTile(backgroundNode, new Vector2(-halfWidth-6, y), color, 0);
        }
    
    
    
    
    }


    void numsupple(){
        
        for (int i = 7 - 1; i > 0; i--) {
        int j = Random.Range(0, 7); // 0부터 i까지의 무작위 인덱스 선택
        int temp = numbers[i]; // 현재 요소를 temp에 저장
        numbers[i] = numbers[j]; // 현재 요소에 무작위로 선택한 요소의 값을 할당
        numbers[j] = temp; // 무작위로 선택한 요소에 temp의 값을 할당
        }
    }

    void killnum(){
            // next의 자식 오브젝트를 반복합니다.
for (int i = 0; i < next.childCount; i++)
{
    // 현재 자식 오브젝트를 가져옵니다.
    Transform child = next.GetChild(i);

    // 현재 자식 오브젝트의 자식 오브젝트를 제거합니다.
    foreach (Transform grandChild in child)
    {
        Destroy(grandChild.gameObject);
    }
}

        
    }

    void minohold() {
        dohold = true;
        if(hold.childCount > 0){
            for(int i = 0; i<4; i++){
                Destroy(hold.GetChild(i).gameObject);
            }
            int maxax = holdnum;
            Color32 color = Color.white;

            if(maxax == 3){
                hold.position = new Vector2(-8.5f, halfHeight-2.5f);
            } 
            else if(maxax == 0) {
                hold.position = new Vector2(-8.5f, halfHeight-3.5f);
            }
            else {
                hold.position = new Vector2(-9f, halfHeight-3);
            }
        
            MakeMino(hold, maxax, color);
            for(int i = 0; i<4; i++){
                Destroy(tetrominoNode.GetChild(i).gameObject);
            }

            maxax = holdnum2;
            color = Color.white;
            tetrominoNode.rotation = Quaternion.identity;

            if(maxax == 3){
                tetrominoNode.position = new Vector2(-0.5f, halfHeight+0.5f);
            } 
            else if(maxax == 0) {
                tetrominoNode.position = new Vector2(-0.5f, halfHeight-0.5f);
            }

            else {
                tetrominoNode.position = new Vector2(-1, halfHeight);
            }
            MakeMino(tetrominoNode, maxax, color);
            holdnum3 = holdnum2;
            holdnum2 = holdnum;
            holdnum = holdnum3;
 
        } else {
            int maxax = holdnum;
            holdnum2 = holdnum;
            Color32 color = Color.white;

            if(maxax == 3){
                hold.position = new Vector2(-8.5f, halfHeight-2.5f);
            } 
            else if(maxax == 0) {
            hold.position = new Vector2(-8.5f, halfHeight-3.5f);
            }
        
            else {
                hold.position = new Vector2(-9f, halfHeight-3);
            }
        
            MakeMino(hold, maxax, color);
            for(int i = 0; i<4; i++){
                Destroy(tetrominoNode.GetChild(i).gameObject);
            }

            CreateTetromino();
        }
    }
    

    void numaddd() {
        
        nextnumbers[3] = nextnumbers[2];
        nextnumbers[2] = nextnumbers[1];
        nextnumbers[1] = nextnumbers[0];
        nextnumbers[0] = numbers[changenum];

        changenum++;
        if(changenum > 6){
            changenum = 0;
            numsupple();
        }
    }

    void MakeMino(Transform parent, int index, Color color){
        switch (index)
        {
            // I : 하늘색
            case 0:
                color = new Color32(115, 251, 253, 255);
                CreateTile(parent, new Vector2(-1.5f, 0.5f), color);
                CreateTile(parent, new Vector2(-0.5f, 0.5f), color);
                CreateTile(parent, new Vector2(0.5f, 0.5f), color);
                CreateTile(parent, new Vector2(1.5f, 0.5f), color);
                break;

            // J : 파란색
            case 1:
                color = new Color32(0, 33, 245, 255);
                CreateTile(parent, new Vector2(-1f, 0.0f), color);
                CreateTile(parent, new Vector2(0f, 0.0f), color);
                CreateTile(parent, new Vector2(1f, 0.0f), color);
                CreateTile(parent, new Vector2(-1f, 1.0f), color);
                break;

            // L : 귤색
            case 2:
                color = new Color32(243, 168, 59, 255);
                CreateTile(parent, new Vector2(-1f, 0.0f), color);
                CreateTile(parent, new Vector2(0f, 0.0f), color);
                CreateTile(parent, new Vector2(1f, 0.0f), color);
                CreateTile(parent, new Vector2(1f, 1.0f), color);
                break;

            // O : 노란색
            case 3:
                color = new Color32(255, 253, 84, 255);
                CreateTile(parent, new Vector2(-0.5f, -0.5f), color);
                CreateTile(parent, new Vector2(0.5f, -0.5f), color);
                CreateTile(parent, new Vector2(-0.5f, 0.5f), color);
                CreateTile(parent, new Vector2(0.5f, 0.5f), color);
                break;

            // S : 녹색
            case 4:
                color = new Color32(117, 250, 76, 255);
                CreateTile(parent, new Vector2(-1f, 0f), color);
                CreateTile(parent, new Vector2(0f, 0f), color);
                CreateTile(parent, new Vector2(0f, 1f), color);
                CreateTile(parent, new Vector2(1f, 1f), color);
                break;

            // T : 자주색
            case 5:
                color = new Color32(155, 47, 246, 255);
                CreateTile(parent, new Vector2(-1f, 0f), color);
                CreateTile(parent, new Vector2(0f, 0f), color);
                CreateTile(parent, new Vector2(1f, 0f), color);
                CreateTile(parent, new Vector2(0f, 1f), color);
                break;

            // Z : 빨간색
            case 6:
                color = new Color32(235, 51, 35, 255);
                CreateTile(parent, new Vector2(-1f, 1f), color);
                CreateTile(parent, new Vector2(0f, 1f), color);
                CreateTile(parent, new Vector2(0f, 0f), color);
                CreateTile(parent, new Vector2(1f, 0f), color);
                break;
        }

    }
   
    void CreateNext2(){
        for(int i = 0; i<4; i++){
            
            
           
            CreateNext(i);
            while (tetrominoqueue.childCount > 0) {

                var node = tetrominoqueue.GetChild(0);

                int x = Mathf.RoundToInt(node.transform.position.x);

                node.parent = next.Find(i.ToString());
                node.name = x.ToString();
            }

        }
            
    }

    void CreateNext(int j)
    {
        int maxax = nextnumbers[j];
        Color32 color = Color.white;
        if(maxax == 0){
            tetrominoqueue.position = new Vector2(halfWidth+2.5f, -halfHeight + 1.5f + j*5);
        }
        else if(maxax ==3){
            tetrominoqueue.position = new Vector2(halfWidth+2.5f, -halfHeight + 2.5f + j*5);
        }
        else{
            tetrominoqueue.position = new Vector2(halfWidth+2, -halfHeight + 2 + j*5);
        }
        
        MakeMino(tetrominoqueue, maxax, color);
    }

    

    // 테트로미노 생성
    void CreateTetromino()
    {   
        
       
        
        int maxax = nextnumbers[3];
        Color32 color = Color.white;
        tetrominoNode.rotation = Quaternion.identity;

        if(maxax == 3){
            tetrominoNode.position = new Vector2(-0.5f, halfHeight+0.5f);
        } 
        else if(maxax == 0) {
            tetrominoNode.position = new Vector2(-0.5f, halfHeight-0.5f);
        }
        
        else {
            tetrominoNode.position = new Vector2(-1, halfHeight);
            
        }
        holdnum = nextnumbers[3];
        numaddd();
        killnum();
        CreateNext2();
        MakeMino(tetrominoNode, maxax, color);
    }
}