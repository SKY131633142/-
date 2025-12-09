using UnityEngine;

public class SubmarineController : MonoBehaviour
{
    // === 可以在 Inspector 面板調整的參數 ===
    
    [Header("移動與速度參數")]
    public float moveSpeed = 5f;        // 前進/後退/上下移動速度
    public float rotationSpeed = 90f;   // 左右轉向速度 (度/秒)
    
    [Header("傾斜參數")]
    public float maxPitchAngle = 15f;    // 最大傾斜角度 (X軸，單位：度)
    public float pitchSpeed = 5f;        // 傾斜速度 (控制傾斜反應快慢)
    public float levelSpeed = 2f;        // 恢復水平的速度
    
    [Header("高度限制參數")]
    public float MAX_HEIGHT = 2f;    // 上限 Y = 2
    public float MIN_HEIGHT = -38f;  // 下限 Y = -38
    
    
    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        // 1. 獲取輸入 (W/S/A/D 和 E/Q)
        float forwardInput = Input.GetAxis("Vertical");      // W/S (前後)
        float turnInput = Input.GetAxis("Horizontal");       // A/D (左右轉向)
        
        float verticalInput = 0f;
        if (Input.GetKey(KeyCode.E)) // E 鍵上浮
        {
            verticalInput = 1f;
        }
        else if (Input.GetKey(KeyCode.Q)) // Q 鍵下潛
        {
            verticalInput = -1f;
        }

        // --- A. 處理水平轉向 (Yaw) ---
        if (turnInput != 0)
        {
            // 繞 Y 軸旋轉
            transform.Rotate(Vector3.up, turnInput * rotationSpeed * Time.deltaTime, Space.Self);
        }

        // --- B. 處理前後移動 (X/Z 平面) ---
        transform.Translate(Vector3.forward * forwardInput * moveSpeed * Time.deltaTime, Space.Self); 

        // --- C. 處理上下浮沉 (Y 軸與高度限制) ---
        
        // 計算 Y 軸的位移量
        float yMovement = verticalInput * moveSpeed * Time.deltaTime;
        
        // 計算潛在的新 Y 座標
        float newY = transform.position.y + yMovement;
        
        // 鎖定 Y 座標在設定的上下限之間
        newY = Mathf.Clamp(newY, MIN_HEIGHT, MAX_HEIGHT);
        
        // 將潛水艇的 Y 座標設置為限制後的值
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);


        // --- D. 處理俯仰傾斜 (Pitching) ---

        // 1. 計算目標傾斜角度 (目標傾斜角 = 上下輸入 * 最大傾斜角)
        // 上浮目標為 -maxPitchAngle (抬頭)，下潛目標為 +maxPitchAngle (低頭)
        float targetPitch = -verticalInput * maxPitchAngle;
        
        // 2. 獲取當前旋轉量
        Quaternion currentRotation = transform.rotation;
        
        // 3. 獲取當前的 Euler 角度，並鎖定 Y 軸 (Yaw) 的值不變，只調整 X 軸 (Pitch)
        Vector3 currentEuler = currentRotation.eulerAngles;
        
        // 4. 使用 Mathf.LerpAngle 或 Quaternion.Slerp 緩慢朝目標角度傾斜
        if (verticalInput != 0)
        {
            // 正在輸入上下方向，快速傾斜到目標角度
            currentEuler.x = Mathf.LerpAngle(currentEuler.x, targetPitch, pitchSpeed * Time.deltaTime);
        }
        else
        {
            // 沒有上下輸入，緩慢恢復到 0 度水平狀態
            currentEuler.x = Mathf.LerpAngle(currentEuler.x, 0f, levelSpeed * Time.deltaTime);
        }
        
        // 5. 將調整後的歐拉角轉換回四元數，並應用於潛水艇
        transform.rotation = Quaternion.Euler(currentEuler);
    }
}