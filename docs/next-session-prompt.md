# 接續 Prompt：CLK.Todos 架構規則整理

這份文件不是架構規則本身（規則都在 `architecture.md`），是給明天接續工作
用的交接筆記。打開新的 Claude Code session 時，可以把「開場 Prompt」那段直接
貼上去，讓我快速抓回上下文。

## ⚠️ 先處理：今天的變更還沒 commit

今天這一整輪規則調整（第 8～16 節，從 `#region Contracts` 排版到
`FindById`/`FindAll` 改名）都還停留在 working tree，還沒 commit + push。
明天接續前，先確認要不要補一個 commit 把今天的進度存起來，不然開新
session 時 `git status` 會看到一大包未提交的變更。

## 開場 Prompt（明天可以直接貼這段）

> 這是 CLK.Todos 練習專案的接續工作。請先讀
> `.claude/skills/mdp-dotnet-architecture/architecture.md`
> 了解目前已經定案的架構規則（目前到第 16 節），以及 `docs/next-session-prompt.md`
> 這份交接筆記。我要繼續用「我下規則、你套用到全部相關檔案並驗證、更新
> architecture.md」這個節奏調整架構慣例，之後每次改完都要 build 驗證、
> 重跑網站做端對端測試（新增/編輯/切換/刪除），不要只改完就結束。跟之前一樣，
> 每次先標註這是不是 Agentic AI Coding，全程中文對話。

## 今天做了什麼（依時間順序）

1. 建立獨立的 `CLK.Todos` Domain 專案（相依乾淨），把 Entity、Repository
   介面搬進去
2. sln 從根目錄搬進 `src/`
3. 建立 `CLK.Todos.Accesses` 專案放 Repository 實作，`InMemoryTodoStore`
   改名 `MockTodoRepository`
4. 新增 `TodoContext` 作為 Domain 入口物件，所有 Repository 透過它的屬性存取
5. Namespace 收斂：整個專案的 namespace 一律等於專案名稱，不因資料夾分層
   多加後綴；改用帶大括號的區塊寫法（不是 file-scoped）
6. `CLK.Todos`、`CLK.Todos.Accesses` 不拆資料夾；`CLK.Todos.WebApp`
   （原 `CLK.Todos.Web`）維持 MVC 慣例資料夾；建立「應用程式進入點專案
   一律 `{Solution}.{類型}App`」命名慣例
7. 建構子注入命名慣例：參數/欄位/屬性名稱都從注入型別反推
8. 類別成員排序慣例（Fields → Constructors → Properties → Methods 等）
9. 合約檢查慣例：`#region Contracts`、`ArgumentNullException.ThrowIfNull`、
   每個檢查獨立一行、不用 `||`/`&&` 合併
10. 關閉全專案 Nullable 參考型別檢查
11. Repository 方法命名與排序：`Add → Update → Remove → FindById → FindAll`，
    `Delete` 改名 `Remove`
12. 充血模型：`ToggleDone` 邏輯搬進 `Todo` 實體本身，Repository 不放業務邏輯
13. `// Default`、`// Return` 註解慣例（建構子賦值、方法正常結果的 return）
14. `!` 取反運算子規則細分：邏輯判斷不能用，翻轉布林值可以用
15. 呼叫自己的方法/屬性要加 `this.`（欄位、Controller 裡 `return` 直接呼叫
    方法的地方例外）
16. 拿掉 `// Imports` 標頭、`using` 跟 `namespace` 間只留一行空白
17. 方法內部依邏輯步驟加小標籤註解（例如 `// FindById`、`// Update`）
18. 清掉所有 `<summary>` XML 文件註解（一次性清理，非架構規則）
19. `GetById`/`GetAll` 改名為 `FindById`/`FindAll`

## 目前狀態

- 三個專案：`CLK.Todos`（Domain）、`CLK.Todos.Accesses`（實作）、
  `CLK.Todos.WebApp`（MVC 網站）
- `.claude/skills/mdp-dotnet-architecture/architecture.md` 是目前最
  完整的規則來源（已跟 `SKILL.md` 放同一個 Skill 資料夾，不再放
  `docs/` 底下）
- Build 0 警告 0 錯誤，Todo CRUD 全功能（新增/查詢/編輯/刪除/切換完成）
  都端對端測試過
- 最後一次 commit：`bcaae86`（今天這輪改動都還沒進版控）

## 接下來可以做的方向（可選）

- ~~把 `docs/architecture-notes.md` 正式轉寫成 `.claude/skills/` 底下的
  Skill 檔案，讓其他團隊能直接套用這套規則~~（已完成：`SKILL.md` 跟
  `architecture.md` 現在同放在 `.claude/skills/mdp-dotnet-architecture/`）
- 繼續補規則的空白（例如字串合約檢查 `ThrowIfNullOrWhiteSpace` 目前還沒有
  實例可套用、`ConsoleApp` 命名慣例也還沒真的用過）
- review 一次 `architecture.md` 有沒有殘留的過時範例（例如第 8 節
  `TodoContext` 範例還沒補上 `#region Contracts`/`// Default`）
