# beanCPU
## 基於 C# 的 16-bit 輕量化 CPU 模擬架構

### 📌 專案介紹
beanCPU 是一個結合了 **暫存器 (Register-based)** 與 **堆疊 (Stack-oriented)** 混合邏輯的 16 位元模擬器專案。本專案提供了一套自定義的指令集 (ISA)、支援預處理功能的組譯器 (Assembler)，以及一個具備點陣字體渲染與即時記憶體監控介面的模擬器 (Emulator)。

---

### 🛠 技術規格
* **字長 (Word Size)**: 16-bit
* **定址空間**: 64 KB (0x0000 - 0xFFFF)
* **通用暫存器**: 8 個 (R0 - R7)
* **特殊暫存器**: 
    * `PC` (Program Counter)
    * `SP` (Stack Pointer, 初始值 0xFFFF)
* **位元組順序**: 大端序 (Big Endian)
* **開發環境**: C# (.NET / WinForms)

---

### 🕹 beanCPU 指令集詳解 (ISA)
指令採用 `(Opcode << 8) | RegisterIndex` 的格式封裝。

| 分類 | 指令範例 | 功能說明 |
| :--- | :--- | :--- |
| **資料傳送** | `MOV Rn, Imm` | **立即數賦值**：將數值 `Imm` 載入暫存器 `Rn`。 |
| | `MOV Rd, Rs` | **語法糖**：底層展開為 `SUB Rd, Rd` 與 `ADD Rd, Rs` 實現暫存器複製。 |
| | `LD Rn, Addr` | **載入**：將記憶體位址 `Addr` 處的值讀取至 `Rn`。 |
| | `ST Rn, Addr` | **儲存**：將 `Rn` 的值寫入記憶體位址 `Addr`。支援 **MMIO**（如 `0xFF00`）。 |
| **算術運算** | `ADD Rd, Rs` | **加法**：`reg[d] = reg[d] + reg[s]`。 |
| | `SUB Rd, Rs` | **減法**：`reg[d] = reg[d] - reg[s]`。 |
| | `MUL Rd, Rs` | **乘法**：`reg[d] = reg[d] * reg[s]`。 |
| | `DIV Rd, Rs` | **除法**：`reg[d] = reg[d] / reg[s]`。除數為 0 時結果設為 `0xFFFF`。 |
| **流程控制** | `JMP Addr` | **無條件跳轉**：將 `PC` 指向 `Addr`。 |
| | `JZ Rn, Addr` | **若為零跳轉**：當 `reg[n] == 0` 時跳轉至 `Addr`。 |
| | `JNZ Rn, Addr` | **若非零跳轉**：當 `reg[n] != 0` 時跳轉至 `Addr`。 |
| | `JE R1, R2, Lbl`| **相等跳轉**：若 `reg[1] == reg[2]` 則跳轉至標籤。 |
| | `JNE/JG/JL` | **條件比較**：支援「不等於」、「大於」、「小於」之比較跳轉。 |
| | `CALL Addr` | **子程式呼叫**：壓入返回位址並跳轉。 |
| | `RET` | **返回**：從堆疊彈出位址並恢復 `PC`。 |
| **堆疊操作** | `PUSH Rn` | 將 `reg[n]` 的值壓入堆疊。 |
| | `POP Rn` | 從堆疊彈出值存入 `reg[n]`。 |
| **系統服務** | `PRINT_STR Adr`| 從位址印出字串，直到遇見 `0x00`。 |
| | `INT Vector` | **中斷**：觸發系統服務（如 `0x10` 刷新螢幕、`0x16` 讀取鍵盤）。 |
| | `BRK` | **停止**：終止 CPU 執行。 |
| **組譯指引** | `.FILL Val` | 在當前位置填充一個 16 位元數值（支援標籤與數值）。 |

---

### 🖥 記憶體映射 (Memory Map)
* **0x0000 - 0xFEFF**: 程式碼與數據區
* **0xFF00**: 字符輸出埠 (Write to Monitor)
* **0xFF10**: 鍵盤輸入緩存 (Key Char)
* **0xFF11**: 鍵盤狀態位 (1: 按下 / 0: 放開)

---

### 🚀 快速上手
1.  **編譯程式碼**:
    使用 `Assembler.exe <source.asm> [output.bin]`。
2.  **執行模擬器**:
    執行 `AsmEmuShort.exe <code.bin> [tick_delay]`。
    * 使用 `Left/Right Arrow` 切換記憶體分頁監控。
3.  **預處理功能**:
    組譯器支援 `USING` 與 `USINGH` 檔案包含功能，以及 `DEFINE` 宏定義。

---

### ⚠️ 注意事項
* **系統限制**: 僅支援 Windows 執行環境（基於 WinForms）。
* **MOV 副作用**: 由於 `MOV Rd, Rs` 使用 `SUB` 實現，會影響潛在的狀態旗標。

---

### ⚖️ 許可協議
本專案採用 **MIT 協議**。您可以自由使用、修改並分發，但請保留原作者的創意聲明與版權標記。
