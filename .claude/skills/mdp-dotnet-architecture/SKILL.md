---
name: mdp-dotnet-architecture
description: .NET 專案的目錄與分層結構（Workspace／Architecture）、程式碼撰寫慣例（Namespace／Class／Field／Constructor／Properties／Method）、以及類別設計規範（Context／Repository／Entity／Dependency Injection）的操作版規則與檢查清單；規劃 .NET 專案架構、撰寫或審查 .NET 程式碼、新增 Entity 時套用。
metadata:
  generated: 2026-09-06T21:46:04+08:00
---

## 使用時機

規劃 .NET 專案的目錄與分層結構、撰寫或審查 Namespace／Class／Field／
Constructor／Properties／Method 這類程式碼撰寫慣例、設計或新增
Context／Repository／Entity、或設定 Dependency Injection 時，套用本規範。

## 01. Workspace 設計規範

- `src/`：所有原始碼專案放這裡。
- `docs/`：架構規則、設計文件這類跟程式碼相關但不參與建置的文件放這裡。
- `tests/`：測試專案放這裡，跟 `src/` 同一層。
- `README.md`：專案說明，放 repo 根目錄。
- `LICENSE`：授權條款，放 repo 根目錄。
- `.gitignore`：Git 版本控制排除清單，放 repo 根目錄。
- repo 根目錄只留上述這類跟建置無關的東西，不放任何 `.sln`／專案檔案。

## 02. Architecture 設計規範

- `{Domain}`：Domain 層，類別庫專案。提供 Entity、Repository 介面
  與 Context，定義業務核心邏輯與規格，不依賴任何框架。
- `{Domain}.Accesses`：Access 層，類別庫專案。提供 Repository 介面
  的實作，負責實際資料存取（資料庫、記憶體等）。
- `{Domain}.WebApp`：Host 層，ASP.NET MVC 專案。提供網頁使用者
  介面。
- `{Domain}.BlazorApp`：Host 層，Blazor 專案。提供互動式網頁應用
  的使用者介面。
- `{Domain}.ConsoleApp`：Host 層，Console 專案。提供命令列工具
  （例如批次匯入、排程工作），無使用者介面。
- 分層相依：Domain 層不相依其他任何層；Access 層相依 Domain 層；
  Host 層相依 Domain 層＋Access 層。
- 同一個 `{Domain}` 可以同時有多個 Host 層專案（例如網站＋批次匯入用的
  Console 工具），都依賴同一個 `{Domain}.Accesses`／`{Domain}`。
- `.sln` 檔名對應 repo，跟所有專案同一層，不放 repo 根目錄。
- `.sln` 可以包含多組 `{Domain}`／`{Domain}.Accesses`／Host 層專案；各組
  `{Domain}` 之間彼此獨立、不共用 Repository 介面或 Context。

## 03. Namespace 設計規範

- 每個專案內所有檔案，namespace 一律等於專案名稱本身，不因資料夾而加後綴
  （例如 `{Domain}.WebApp` 裡不管檔案放在哪個資料夾，namespace 都是
  `{Domain}.WebApp`）。
- 一律用 file-scoped 寫法（`namespace X;`，不用大括號區塊）。
- `using` 放在檔案最上面，跟 `namespace` 之間空一行（C# 官方範本順序，
  不要反過來）；沒有 `using` 的檔案直接從 `namespace` 開始。
- 資料夾要不要拆，依專案性質決定：`{Domain}`（Domain）、
  `{Domain}.Accesses`（資料存取實作）不拆資料夾，檔案直接放專案根
  目錄，靠 namespace 就足夠辨識；`{Domain}.WebApp`（Host 層）維持 MVC
  慣例資料夾（`Controllers/`／`Models/`／`Views/`）。不論拆不拆，資料夾都
  只是物理上分類檔案，不影響 namespace。

## 04. Class 設計規範

- 類別（含介面）內成員照以下順序分類，只列出實際有內容的分類：

  ```
  // Singleton
  // Imports
  // Constants
  // Enumeration
  // Fields
  // Constructors
  // Properties
  // Methods
  // Operators
  // Handlers
  // Events
  ```

- 每個分類標頭下第一個成員緊接著寫、不空行；同分類內成員之間空一行；
  不同分類之間空兩行。
- `Imports`（`using`）不算進分類清單、不加標頭。
- 類別內部呼叫自己（含繼承來）的方法或屬性，一律不加 `this.` 前綴。
  Controller 繼承自 `Controller` 的成員（`ModelState`、`HttpContext`、
  `View(...)`、`RedirectToAction(...)`）也一律不加。
- 成員排序規則只管類別／介面內部排序，`Program.cs` 這種 top-level
  statements 進入點檔案不適用。

## 05. Field 設計規範

- 命名：`_` + 參數命名（注入型別去掉介面字首 `I`，第一個字母轉小寫）。
- `// Fields` 分類內，lock 物件排最前面，其餘欄位接在後面。

## 06. Constructor 設計規範

- 適用於所有類別，不限於某一層。
- 參數命名：注入型別去掉介面字首 `I`（如果是介面），第一個字母轉小寫。
- `// Default`：合約檢查後、把參數存進欄位的賦值陳述式，前面加這個
  標籤，跟 `// Contracts` 空一行、跟賦值本身不空行。
- 一律先用私有欄位承接注入物件，不要繞過欄位直接賦值給屬性或到處傳遞。
- 合約檢查放方法本體最前面，用 `// Contracts` 標籤，格式規則跟方法共用；
  建構子最常見的是參照型別參數的 not-null 檢查。

## 07. Properties 設計規範

- 公開屬性命名：注入型別去掉介面字首 `I`，維持 PascalCase（不轉小寫）。
- 一律用完整的 `get` / `return` 寫法，不用 `=>` expression-bodied（自動
  屬性 `{ get; set; }` 不受影響）。
- `get` 如果整個只有一行（只有一個 `return`），不加 `// Return`，整個
  `get` 縮成一行寫，比展開成三行好讀。

## 08. Method 設計規範

- 方法本體避免深巢狀，拆成平鋪區塊，每個區塊前面加一個單字小標籤，
  第一個標籤前不空行、後面每個標籤前空一行；標籤只提示角色，不要把邏輯
  寫進註解裡。優先從下表挑字：

  | 標籤 | 用途 |
  |---|---|
  | `// Contracts` | 方法／建構子最前面的參數合約檢查 |
  | `// Default` | 建構子專用，把參數存進欄位的預設賦值 |
  | `// Search` | 查詢資料（例如 Repository 的 `FindById`／`FindAll`） |
  | `// Execute` | 執行核心動作（Repository 的 `Add`／`Update`／`Remove`，或 Entity 狀態轉換） |
  | `// Return` | 方法裡代表「正常結果」的最後一個 `return` |
  | `// Lock` | 進入 `lock` 區塊做執行緒同步 |
  | `// Create` | 建立一個接下來要用的物件實例（例如 `IDbContextFactory` 建立 `DbContext`） |
  | `// Variables`／`// Define`／`// Require`／`// Arguments`／`// Notify`／`// Raise` | 尚無實例，先保留定義，遇到對應場景再套用 |

- `// Contracts` 放在方法（或建構子）本體最前面，檢查參數合約。
- 參照型別參數不能為 `null`：`ArgumentNullException.ThrowIfNull(參數)`。
- 字串參數不能為 `null`／空白：`ArgumentException.ThrowIfNullOrWhiteSpace(參數)`。
- 值型別參數（`int`、`bool`、`DateTime`）不需要檢查。
- MVC 驗證判斷（`ModelState.IsValid`、路由 id 跟表單 id 是否一致）也算合約
  檢查，放進同一個 `// Contracts` 底下。
- `// Contracts` 內每個檢查獨立一行，不用 `||`／`&&` 合併；檢查之間不空行，
  跟後面邏輯空一行。
- `// Return`：方法裡真正交付結果的最後一個 `return` 才標；guard
  clause 式的提前 `return`（例如 `if ({entity} is null) return NotFound();`）
  不用標。回傳型別是 `void` 的方法（例如 `Update`／`Remove` 找不到就丟
  例外）沒有交付結果，不需要 `// Return`。
- guard clause 一律省略大括號、寫成一行（不只限於 `// Contracts`，
  方法本體任何「不合法／找不到就提前 return 或丟例外」都比照辦理）。
- 只有一個步驟的方法（例如 `FindById`）不需要額外標籤，`// Return` 本身
  就夠。

## 09. Context 設計規範

- `{Domain}Context` 是 Domain 的入口物件：把所有 Repository 介面透過建構子
  注入進來，用唯讀屬性對外提供。
- `{Domain}Context` 每個 `{Domain}` 只有一個，不隨 Entity 數量增加而變多：
  同一個 `{Domain}` 新增 Entity 時，是在既有的 Context 上多加一個
  Repository 屬性，不是另外開一個新的 Context；一個 `.sln` 有多個
  `{Domain}` 時，每個 `{Domain}` 各自有自己的 Context，彼此不共用。
- 外部呼叫端（Controller 等）一律注入 `{Domain}Context` 來使用 Repository，
  不直接注入個別 Repository 介面。

## 10. Repository 設計規範

- 命名：介面 `I{Entity}Repository`；非持久化實作 `Mock{Entity}{介面名}`；
  真實資料庫實作（尚未使用，先預留）`{技術}{Entity}{介面名}`（`{技術}`
  例如資料存取技術為 Entity Framework 時取 `Ef`）。
- 方法順序固定：新增 → 修改 → 刪除 → 查單筆 → 查全部 → 查全部（有條件）。
- 刪除方法一律叫 `Remove`，不叫 `Delete`。
- 查詢命名：`Find` 開頭查單筆（回傳 `{Entity}?`）；`FindAll` 開頭查
  多筆（回傳 `IReadOnlyList<{Entity}>`）。
- 方法順序與命名樣板：

```csharp
void Add({Entity} entity);

void Update({Entity} entity);

void Remove(Guid {entity}Id);

{Entity}? FindByXX(Guid {entity}Id);

IReadOnlyList<{Entity}> FindAll();

IReadOnlyList<{Entity}> FindAllByXX();
```

- `Add` 不用額外檢查主鍵是否已存在。
- `Add` 把 `CreateTime`／`UpdateTime` 都重新蓋上 `DateTime.UtcNow`，不信任
  物件建構當下的預設值。
- `Update` 維持 `CreateTime` 既有值不覆寫，只把 `UpdateTime` 重新蓋上，不
  信任呼叫端傳進來的值。
- 失敗語意依方法類型分兩種，不混用：Query 方法（`Find` 開頭）用回傳值
  本身表達「找不到」（`null`／空集合）；Command 方法（`Add`／`Update`／
  `Remove`）一律回傳 `void`，找不到對應資料就直接丟例外
  （`KeyNotFoundException`）。
- 例外要在哪一層被攔截、轉換成什麼樣的 HTTP 回應，先不在這份文件規範，
  留待之後訂錯誤處理規則時再一併決定。
- Repository 只放存取資料的方法（CRUD＋查詢），不放業務邏輯／狀態轉換
  （例如「切換完成狀態」）。不要在 Repository 開
  `Toggle{XX}(Guid {entity}Id)` 這種直接用 id 操作、把邏輯藏在 Repository
  裡的方法。
- 真實資料庫實作（`Ef{Entity}Repository`）建構子注入
  `IDbContextFactory<{Domain}DbContext>`，每個方法內用 `using (...) { }`
  （不用 `using var`）建立短命 `DbContext`，區塊結束就 `Dispose`。

## 11. Entity 設計規範

- 主鍵：屬性命名 `{Entity}Id`（不要單純的 `Id`），型別 `Guid`，用
  `Guid.CreateVersion7()` 產生（不是 `Guid.NewGuid()`），不用 `int` 流水
  號；由 Entity 屬性預設值在建立當下產生，Repository 不再自己產生。
- 主鍵變數／參數統一命名 `{entity}Id`，貫穿 Repository、Controller action
  參數、MVC 路由樣板（`{{entity}Id?}`，不是泛用 `{id?}`）、View 的
  `asp-route-{entity}Id`，不留通用的 `id`。
- 稽核時間戳記：`CreateTime`（不是 `CreatedAt`）、`UpdateTime`，型別
  `DateTime`，一律存 UTC，預設值 `DateTime.UtcNow`（不是 `DateTime.Now`）；
  這條是全專案通則，任何時間戳記都比照辦理，需要顯示當地時間才在畫面層
  轉換。
- Entity 狀態轉換方法（例如切換某個布林狀態的方法）不用自己碰
  `UpdateTime`，經過 Repository 的 `Update` 就一定會蓋到。
- 充血模型（Rich Domain Model）：跟 Entity 自身狀態有關的業務邏輯，一律
  寫成 Entity 上的方法，不要寫在 Repository 或 Controller 裡。
- 呼叫端標準流程：Repository 查出 Entity → 呼叫 Entity 方法改變狀態 →
  Repository 存回去。
- 業務欄位（非主鍵、非稽核時間戳記）一律用
  `System.ComponentModel.DataAnnotations` 屬性保護輸入合法性，依型別與
  業務規則挑選（例如 `[Required]`、`[StringLength]`、`[EmailAddress]`），
  不能只靠 Controller 的 `ModelState.IsValid` 裸檢查，也不用自己在 Entity
  方法或 Controller 裡手寫等價的 if 判斷。
- 不可為空白的字串屬性，要加上 `[Required(ErrorMessage = "不可以為空白")]`；
  只加 `[StringLength]` 不會擋空字串或 `null`，兩者是互補而非互斥的規則，
  「不可為空白」跟「長度上限」要分開標註。
- `ErrorMessage` 一律用「參數驗證」角度撰寫：陳述「這個屬性違反了什麼
  規則」（`不可以為空白`、`長度不可超過 N 字`），不要用引導使用者
  操作的祈使句（不要 `請輸入 XXX`）。跟 Method／Constructor `// Contracts`
  的合約檢查（`ArgumentException` 系列）用同一種語氣，全專案「合約違規」
  一律陳述規則本身，不指示下一步動作。

## 12. Dependency Injection 設計規範

- Domain 層（`{Domain}Context`）與 Access 層（Repository 實作，不論
  Mock 或真實資料庫）在 DI 容器裡一律用 Singleton 生命週期註冊。
- 所有 DI 註冊都寫在 Host 層專案的進入點檔案（例如
  `{Domain}.WebApp/Program.cs`）。
- 註冊順序：先註冊 Repository 介面對實作，再註冊 `{Domain}Context`——
  跟建構子相依方向（Context 依賴 Repository）一致。
- 非持久化實作：`AddSingleton<I{Entity}Repository, Mock{Entity}Repository>()`。
- 真實資料庫實作：`AddDbContextFactory<{Domain}DbContext>()` 搭配
  `AddSingleton<I{Entity}Repository, Ef{Entity}Repository>()`。
- `{Domain}Context`：`AddSingleton<{Domain}Context>()`。
- Host 層本身的生命週期（例如 MVC Controller 每個請求一個實例）由
  ASP.NET Core 框架管理，不在這裡的規範範圍內。

## 新增 Entity 檢查清單

1. 在 `{Domain}` 專案裡建立 `{Entity}` 類別：
   - 主鍵屬性 `{Entity}Id`，型別 `Guid`，預設值 `Guid.CreateVersion7()`。
   - 稽核時間戳記 `CreateTime`／`UpdateTime`，型別 `DateTime`，預設值
     `DateTime.UtcNow`。
   - 業務欄位加上對應的 `DataAnnotations`（`[Required]`／`[StringLength]`／
     `[EmailAddress]` 等）；不可為空白的字串屬性另加
     `[Required(ErrorMessage = "不可以為空白")]`；`ErrorMessage` 用陳述式
     語氣，不用祈使句。
   - 跟 Entity 自身狀態有關的業務邏輯寫成 Entity 上的方法（充血模型），
     狀態轉換方法不用自己蓋 `UpdateTime`。
2. 在 `{Domain}` 專案裡建立 `I{Entity}Repository` 介面：
   - 方法順序 `Add` → `Update` → `Remove` → `FindByXX` → `FindAll` →
     `FindAllByXX`，命名依樣板；刪除方法叫 `Remove`。
   - 查單筆 `Find` 開頭回傳 `{Entity}?`；查多筆 `FindAll` 開頭回傳
     `IReadOnlyList<{Entity}>`。
   - 只放存取資料的方法，不放業務邏輯／狀態轉換方法。
3. 在 `{Domain}.Accesses` 專案裡建立 `Mock{Entity}Repository`（非持久化
   實作）：
   - 成員排序（Fields／Constructors／Methods），lock 物件排 `// Fields`
     最前面。
   - `Add` 蓋 `CreateTime`／`UpdateTime`；`Update` 只蓋 `UpdateTime`；
     Command 方法找不到資料就丟 `KeyNotFoundException`。
   - 之後若要接真實資料庫，比照命名為 `Ef{Entity}Repository`，建構子
     注入 `IDbContextFactory<{Domain}DbContext>`，方法內用
     `using (...) { }` 建立短命 `DbContext`。
4. 在既有 `{Domain}Context` 上多加一個 `I{Entity}Repository` 唯讀屬性，
   不另外開新的 Context。
5. 在 Host 層 `Program.cs` 註冊 DI：先註冊
   `AddSingleton<I{Entity}Repository, Mock{Entity}Repository>()`
   （或搭配 `AddDbContextFactory` 註冊 `Ef{Entity}Repository`），
   `{Domain}Context` 的 Singleton 註冊全 Domain 共用一次，不用為新
   Entity 重複註冊。
6. 呼叫端（Controller 等）一律注入 `{Domain}Context` 使用新
   Repository，不直接注入 `I{Entity}Repository`。
7. 主鍵變數／參數統一命名 `{entity}Id`，貫穿 Repository、Controller
   action 參數、MVC 路由樣板（`{{entity}Id?}`）、View 的
   `asp-route-{entity}Id`。
8. 新增的類別／方法比照全域撰寫慣例：namespace 等於專案名稱、
   file-scoped 寫法、Class 成員分類排序、Field／Constructor／
   Properties／Method 各節格式規則。

## 自我檢查清單（寫完程式碼後逐條核對）

- [ ] `src/`／`docs/`／`tests/` 三個資料夾用途正確，repo 根目錄只有
      `README.md`／`LICENSE`／`.gitignore`，沒有 `.sln`／專案檔案。
- [ ] `.sln` 跟所有專案同一層（`src/` 底下），沒有放在 repo 根目錄。
- [ ] 分層相依方向正確：Domain 不依賴其他層；Access 依賴 Domain；
      Host 依賴 Domain＋Access；沒有循環依賴。
- [ ] 同一組 `{Domain}` 內，各 Host 層專案都依賴同一個
      `{Domain}.Accesses`／`{Domain}`；不同組 `{Domain}` 之間沒有共用
      Repository 介面或 Context。
- [ ] 每個檔案的 namespace 等於專案名稱本身，沒有因資料夾而加後綴。
- [ ] namespace 用 file-scoped 寫法；`using` 在檔案最上面，跟
      `namespace` 之間空一行。
- [ ] `{Domain}`／`{Domain}.Accesses` 沒有拆資料夾；`{Domain}.WebApp`
      維持 `Controllers/`／`Models/`／`Views/` 慣例資料夾。
- [ ] 類別（含介面）成員照 Singleton／Imports／Constants／
      Enumeration／Fields／Constructors／Properties／Methods／
      Operators／Handlers／Events 順序排列，只列出實際有內容的分類。
- [ ] 分類標頭下第一個成員不空行、同分類成員間空一行、不同分類間
      空兩行；`Imports` 沒有加標頭。
- [ ] 類別內部呼叫自己（含繼承來，例如 Controller 基底類別）的方法或
      屬性，沒有加 `this.` 前綴。
- [ ] `Program.cs` 這類 top-level statements 進入點檔案沒有被硬套用
      成員排序規則。
- [ ] 欄位命名為 `_` + 參數命名（介面型別去掉 `I`、第一個字母轉小寫）。
- [ ] `// Fields` 分類內 lock 物件排最前面。
- [ ] 建構子參數命名：介面型別去掉 `I`、第一個字母轉小寫。
- [ ] 建構子內 `// Contracts` 檢查後接 `// Default` 賦值，`// Default`
      跟 `// Contracts` 空一行、跟賦值本身不空行。
- [ ] 建構子一律先用私有欄位承接注入物件，沒有繞過欄位直接賦值給
      屬性或到處傳遞。
- [ ] 公開屬性命名：介面型別去掉 `I`，維持 PascalCase。
- [ ] 屬性用完整 `get`／`return` 寫法，沒有用 `=>` expression-bodied
      （自動屬性除外）；只有一個 `return` 的 `get` 縮成一行、沒加
      `// Return`。
- [ ] 方法本體依角色拆成平鋪區塊，每區塊前加單字標籤（優先從既有
      詞彙表挑字），第一個標籤前不空行、之後每個標籤前空一行，標籤
      沒有把邏輯寫進註解裡。
- [ ] `// Contracts` 放在方法／建構子最前面；參照型別參數用
      `ArgumentNullException.ThrowIfNull`；字串參數用
      `ArgumentException.ThrowIfNullOrWhiteSpace`；值型別參數沒有
      多餘檢查。
- [ ] MVC 驗證判斷（`ModelState.IsValid`、路由 id 與表單 id 一致性）
      放進同一個 `// Contracts` 底下。
- [ ] `// Contracts` 內每個檢查獨立一行，沒有用 `||`／`&&` 合併；
      檢查之間不空行、跟後續邏輯空一行。
- [ ] `// Return` 只標「正常結果」的最後一個 `return`；guard clause
      式提前 `return` 沒有標；回傳 `void` 的方法沒有加 `// Return`。
- [ ] guard clause 一律省略大括號、寫成一行。
- [ ] 只有一個步驟的方法沒有加多餘標籤，只用 `// Return`。
- [ ] `{Domain}Context` 把所有 Repository 介面透過建構子注入、用唯讀
      屬性對外提供；每個 `{Domain}` 只有一個 Context，新增 Entity 是
      加屬性而不是開新 Context。
- [ ] 呼叫端（Controller 等）一律注入 `{Domain}Context`，沒有直接注入
      個別 Repository 介面。
- [ ] Repository 介面／實作命名正確：`I{Entity}Repository`、
      `Mock{Entity}{介面名}`、`{技術}{Entity}{介面名}`。
- [ ] Repository 方法順序為新增→修改→刪除→查單筆→查全部→查全部
      （有條件）；刪除方法叫 `Remove`，不叫 `Delete`；查單筆
      `Find` 開頭回傳 `{Entity}?`，查多筆 `FindAll` 開頭回傳
      `IReadOnlyList<{Entity}>`。
- [ ] `Add` 沒有額外檢查主鍵是否已存在，且重新蓋上
      `CreateTime`／`UpdateTime`；`Update` 保留既有 `CreateTime`、只
      重新蓋 `UpdateTime`。
- [ ] Query 方法用回傳值本身表達「找不到」；Command 方法回傳
      `void`，找不到對應資料就丟 `KeyNotFoundException`。
- [ ] Repository 只放 CRUD＋查詢方法，沒有把業務邏輯／狀態轉換
      （例如 `Toggle{XX}(Guid {entity}Id)`）藏在 Repository 裡。
- [ ] 真實資料庫實作建構子注入 `IDbContextFactory<{Domain}DbContext>`，
      方法內用 `using (...) { }`（不是 `using var`）建立短命
      `DbContext`。
- [ ] Entity 主鍵屬性命名 `{Entity}Id`、型別 `Guid`、用
      `Guid.CreateVersion7()` 產生，不是 `Guid.NewGuid()` 或 `int`
      流水號，且由屬性預設值產生、Repository 沒有另外產生。
- [ ] 主鍵變數／參數統一命名 `{entity}Id`，Repository、Controller
      action 參數、MVC 路由樣板、View 的 `asp-route-{entity}Id` 都沒
      有殘留通用的 `id`。
- [ ] 稽核時間戳記命名 `CreateTime`／`UpdateTime`（不是
      `CreatedAt`），型別 `DateTime`，預設值 `DateTime.UtcNow`（不是
      `DateTime.Now`），全專案其他時間戳記也比照辦理。
- [ ] Entity 狀態轉換方法沒有自己碰 `UpdateTime`。
- [ ] 跟 Entity 自身狀態有關的業務邏輯寫在 Entity 方法上（充血模型），
      沒有寫在 Repository 或 Controller 裡；呼叫端依「查出→呼叫方法
      →存回去」流程操作。
- [ ] 業務欄位都用 `DataAnnotations` 保護，沒有只靠
      `ModelState.IsValid` 裸檢查，也沒有手寫等價 if 判斷。
- [ ] 不可為空白的字串屬性同時有 `[Required(ErrorMessage = "不可以
      為空白")]` 與長度限制（如 `[StringLength]`），兩者沒有被省略
      其中一個。
- [ ] 所有 `ErrorMessage`（含 `// Contracts` 的 `ArgumentException`
      訊息）都用陳述規則的語氣，沒有出現「請輸入 XXX」這類祈使句。
- [ ] Domain／Access 層（`{Domain}Context`、Repository 實作）在 DI
      容器裡都用 Singleton 註冊。
- [ ] 所有 DI 註冊都寫在 Host 層進入點檔案（例如
      `{Domain}.WebApp/Program.cs`）。
- [ ] DI 註冊順序先 Repository 介面對實作、再 `{Domain}Context`，跟
      建構子相依方向一致。
