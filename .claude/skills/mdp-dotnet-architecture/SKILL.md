---
name: mdp-dotnet-architecture
description: .NET 專案通用設計規範（目錄分層、Namespace、Class 成員排序、Constructor/Method 慣例、Context／Repository／Entity／DI 規則）。撰寫或修改任何 .NET 專案的 .cs 檔案（含 Domain／Access／Host 層）前必讀，確保產出符合本規範。
user-invocable: true
metadata:
  created: "2026-09-05 22:48:06 +0800"
---

# .NET 設計規範（AI 執行版）

規則一律用 `{Domain}`、`{Entity}` 佔位符描述，不綁定具體專案或名稱。

## 使用時機

- 在任何 .NET 專案的 `src/` 底下新增或修改 `.cs` 檔案之前，先核對下面對
  應章節的規則。
- 新增一個 Entity 時，依序做完「新增 Entity 檢查清單」的每一步，缺一步
  就回頭補。
- 寫完後跑一次「自我檢查清單」，逐條核對，不合就修到合為止。

## 01. Workspace

- `src/`：所有原始碼專案放這裡。
- `docs/`：架構規則、設計文件這類跟程式碼相關但不參與建置的文件放這裡。
- `tests/`：測試專案放這裡，跟 `src/` 同一層。
- `README.md`：專案說明，放 repo 根目錄。
- `LICENSE`：授權條款，放 repo 根目錄。
- `.gitignore`：Git 版本控制排除清單，放 repo 根目錄。
- repo 根目錄只留上述這類跟建置無關的東西，不放任何 `.sln`／專案檔案。

## 02. Architecture（分層）

- `{Domain}`：Domain 層，類別庫專案。提供 Entity、Repository 介面與
  Context，定義業務核心邏輯與規格，不依賴任何框架。
- `{Domain}.Accesses`：Access 層，類別庫專案。提供 Repository 介面的實
  作，負責實際資料存取（資料庫、記憶體等）。
- `{Domain}.WebApp`：Host 層，ASP.NET MVC 專案。提供網頁使用者介面。
- `{Domain}.BlazorApp`：Host 層，Blazor 專案。提供互動式網頁應用的使用
  者介面。
- `{Domain}.ConsoleApp`：Host 層，Console 專案。提供命令列工具（例如批
  次匯入、排程工作），無使用者介面。
- 分層相依：Domain 層不相依其他任何層；Access 層相依 Domain 層；Host
  層相依 Domain 層＋Access 層。
- 同一個 `{Domain}` 可以同時有多個 Host 層專案（例如網站＋批次匯入用的
  Console 工具），都依賴同一個 `{Domain}.Accesses`／`{Domain}`。
- `.sln` 檔名對應 repo，跟所有專案同一層，不放 repo 根目錄。
- `.sln` 可以包含多組 `{Domain}`／`{Domain}.Accesses`／Host 層專案；各組
  `{Domain}` 之間彼此獨立、不共用 Repository 介面或 Context。

## 03. Namespace

- 每個專案內所有檔案，namespace 一律等於專案名稱本身，不因資料夾而加後
  綴（例如 `{Domain}.WebApp` 裡不管檔案放在哪個資料夾，namespace 都是
  `{Domain}.WebApp`）。
- 一律用 file-scoped 寫法（`namespace X;`，不用大括號區塊）。
- `using` 放在檔案最上面，跟 `namespace` 之間空一行（C# 官方範本順序，
  不要反過來）；沒有 `using` 的檔案直接從 `namespace` 開始。
- 資料夾要不要拆，依專案性質決定：`{Domain}`（Domain）、
  `{Domain}.Accesses`（資料存取實作）不拆資料夾，檔案直接放專案根目
  錄，靠 namespace 就足夠辨識；`{Domain}.WebApp`（Host 層）維持 MVC 慣
  例資料夾（`Controllers/`／`Models/`／`Views/`）。不論拆不拆，資料夾都
  只是物理上分類檔案，不影響 namespace。

## 04. Class（成員排序）

類別（含介面）內成員照以下順序分類，只列出實際有內容的分類：

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

- 每個分類標頭下第一個成員緊接著寫、不空行；同分類內成員之間空一行；不
  同分類之間空兩行。
- `Imports`（`using`）不算進分類清單、不加標頭。
- 類別內部呼叫自己（含繼承來）的方法或屬性，一律不加 `this.` 前綴。
  Controller 繼承自 `Controller` 的成員（`ModelState`、`HttpContext`、
  `View(...)`、`RedirectToAction(...)`）也一律不加。
- 成員排序規則只管類別／介面內部排序，`Program.cs` 這種 top-level
  statements 進入點檔案不適用。

## 05. Field

- 命名：`_` + 參數命名（注入型別去掉介面字首 `I`，第一個字母轉小寫）。
- `// Fields` 分類內，lock 物件排最前面，其餘欄位接在後面。

## 06. Constructor

適用於所有類別，不限於某一層。

- 參數命名：注入型別去掉介面字首 `I`（如果是介面），第一個字母轉小寫。
- `// Default`：合約檢查後、把參數存進欄位的賦值陳述式，前面加這個標
  籤，跟 `// Contracts` 空一行、跟賦值本身不空行。
- 一律先用私有欄位承接注入物件，不要繞過欄位直接賦值給屬性或到處傳遞。
- 合約檢查放方法本體最前面，用 `// Contracts` 標籤，格式規則跟方法共
  用；建構子最常見的是參照型別參數的 not-null 檢查。

## 07. Properties

- 公開屬性命名：注入型別去掉介面字首 `I`，維持 PascalCase（不轉小
  寫）。
- 一律用完整的 `get` / `return` 寫法，不用 `=>` expression-bodied（自動
  屬性 `{ get; set; }` 不受影響）。
- `get` 如果整個只有一行（只有一個 `return`），不加 `// Return`，整個
  `get` 縮成一行寫，比展開成三行好讀。

## 08. Method

方法本體避免深巢狀，拆成平鋪區塊，每個區塊前面加一個單字小標籤，第一個
標籤前不空行、後面每個標籤前空一行；標籤只提示角色，不要把邏輯寫進註解
裡。優先從下表挑字：

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
- 字串參數不能為 `null`／空白：
  `ArgumentException.ThrowIfNullOrWhiteSpace(參數)`。
- 值型別參數（`int`、`bool`、`DateTime`）不需要檢查。
- MVC 驗證判斷（`ModelState.IsValid`、路由 id 跟表單 id 是否一致）也算合
  約檢查，放進同一個 `// Contracts` 底下。
- `// Contracts` 內每個檢查獨立一行，不用 `||`／`&&` 合併；檢查之間不空
  行，跟後面邏輯空一行。
- `// Return`：方法裡真正交付結果的最後一個 `return` 才標；guard clause
  式的提前 `return`（例如 `if ({entity} is null) return NotFound();`）
  不用標。回傳型別是 `void` 的方法（例如 `Update`／`Remove` 找不到就丟
  例外）沒有交付結果，不需要 `// Return`。
- guard clause 一律省略大括號、寫成一行（不只限於 `// Contracts`，方法
  本體任何「不合法／找不到就提前 return 或丟例外」都比照辦理）。
- 只有一個步驟的方法（例如 `FindById`）不需要額外標籤，`// Return` 本身
  就夠。

## 09. Context

- `{Domain}Context` 是 Domain 的入口物件：把所有 Repository 介面透過建
  構子注入進來，用唯讀屬性對外提供。
- `{Domain}Context` 每個 `{Domain}` 只有一個，不隨 Entity 數量增加而變
  多：同一個 `{Domain}` 新增 Entity 時，是在既有的 Context 上多加一個
  Repository 屬性，不是另外開一個新的 Context；一個 `.sln` 有多個
  `{Domain}` 時，每個 `{Domain}` 各自有自己的 Context，彼此不共用。
- 外部呼叫端（Controller 等）一律注入 `{Domain}Context` 來使用
  Repository，不直接注入個別 Repository 介面。

## 10. Repository

- 命名：介面 `I{Entity}Repository`；非持久化實作
  `Mock{Entity}{介面名}`；真實資料庫實作（尚未使用，先預留）
  `{技術}{Entity}{介面名}`（`{技術}` 例如資料存取技術為 Entity Framework
  時取 `Ef`）。
- 方法順序固定：新增 → 修改 → 刪除 → 查單筆 → 查全部 → 查全部（有條
  件）。
- 刪除方法一律叫 `Remove`，不叫 `Delete`。
- 查詢命名：`Find` 開頭查單筆（回傳 `{Entity}?`）；`FindAll` 開頭查多筆
  （回傳 `IReadOnlyList<{Entity}>`）。
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
- `Add` 把 `CreateTime`／`UpdateTime` 都重新蓋上 `DateTime.UtcNow`，不信
  任物件建構當下的預設值。
- `Update` 維持 `CreateTime` 既有值不覆寫，只把 `UpdateTime` 重新蓋上，
  不信任呼叫端傳進來的值。
- 失敗語意依方法類型分兩種，不混用：Query 方法（`Find` 開頭）用回傳值
  本身表達「找不到」（`null`／空集合）；Command 方法（`Add`／`Update`／
  `Remove`）一律回傳 `void`，找不到對應資料就直接丟例外
  （`KeyNotFoundException`）。
- 例外要在哪一層被攔截、轉換成什麼樣的 HTTP 回應，先不在這份文件規範，
  留待之後訂錯誤處理規則時再一併決定。
- Repository 只放存取資料的方法（CRUD＋查詢），不放業務邏輯／狀態轉換
  （例如「切換完成狀態」）。不要在 Repository 開
  `Toggle{XX}(Guid {entity}Id)` 這種直接用 id 操作、把邏輯藏在
  Repository 裡的方法。
- 真實資料庫實作（`Ef{Entity}Repository`）建構子注入
  `IDbContextFactory<{Domain}DbContext>`，每個方法內用 `using (...) { }`
  （不用 `using var`）建立短命 `DbContext`，區塊結束就 `Dispose`。

## 11. Entity

- 主鍵：屬性命名 `{Entity}Id`（不要單純的 `Id`），型別 `Guid`，用
  `Guid.CreateVersion7()` 產生（不是 `Guid.NewGuid()`），不用 `int` 流水
  號；由 Entity 屬性預設值在建立當下產生，Repository 不再自己產生。
- 主鍵變數／參數統一命名 `{entity}Id`，貫穿 Repository、Controller
  action 參數、MVC 路由樣板（`{{entity}Id?}`，不是泛用 `{id?}`）、View
  的 `asp-route-{entity}Id`，不留通用的 `id`。
- 稽核時間戳記：`CreateTime`（不是 `CreatedAt`）、`UpdateTime`，型別
  `DateTime`，一律存 UTC，預設值 `DateTime.UtcNow`（不是
  `DateTime.Now`）；這條是全專案通則，任何時間戳記都比照辦理，需要顯示
  當地時間才在畫面層轉換。
- Entity 狀態轉換方法（例如切換某個布林狀態的方法）不用自己碰
  `UpdateTime`，經過 Repository 的 `Update` 就一定會蓋到。
- 充血模型（Rich Domain Model）：跟 Entity 自身狀態有關的業務邏輯，一律
  寫成 Entity 上的方法，不要寫在 Repository 或 Controller 裡。
- 呼叫端標準流程：Repository 查出 Entity → 呼叫 Entity 方法改變狀態 →
  Repository 存回去。
- 業務欄位（非主鍵、非稽核時間戳記）一律用
  `System.ComponentModel.DataAnnotations` 屬性保護輸入合法性，依型別與
  業務規則挑選（例如 `[Required]`、`[StringLength]`、
  `[EmailAddress]`），不能只靠 Controller 的 `ModelState.IsValid` 裸檢
  查，也不用自己在 Entity 方法或 Controller 裡手寫等價的 if 判斷。
- 不可為空白的字串屬性，要加上 `[Required(ErrorMessage = "不可以為空白")]`；
  只加 `[StringLength]` 不會擋空字串或 `null`，兩者是互補而非互斥的規則，
  「不可為空白」跟「長度上限」要分開標註。
- `ErrorMessage` 一律用「參數驗證」角度撰寫：陳述「這個屬性違反了什麼規
  則」（`不可以為空白`、`長度不可超過 N 字`），不要用引導使用者操作的祈
  使句（不要 `請輸入 XXX`）。跟 Method／Constructor `// Contracts` 的合
  約檢查（`ArgumentException` 系列）用同一種語氣，全專案「合約違規」一
  律陳述規則本身，不指示下一步動作。

## 12. Dependency Injection

- Domain 層（`{Domain}Context`）與 Access 層（Repository 實作，不論
  Mock 或真實資料庫）在 DI 容器裡一律用 Singleton 生命週期註冊。
- 所有 DI 註冊都寫在 Host 層專案的進入點檔案（例如
  `{Domain}.WebApp/Program.cs`）。
- 註冊順序：先註冊 Repository 介面對實作，再註冊 `{Domain}Context`——跟
  建構子相依方向（Context 依賴 Repository）一致。
- 非持久化實作：
  `AddSingleton<I{Entity}Repository, Mock{Entity}Repository>()`。
- 真實資料庫實作：`AddDbContextFactory<{Domain}DbContext>()` 搭配
  `AddSingleton<I{Entity}Repository, Ef{Entity}Repository>()`。
- `{Domain}Context`：`AddSingleton<{Domain}Context>()`。
- Host 層本身的生命週期（例如 MVC Controller 每個請求一個實例）由
  ASP.NET Core 框架管理，不在這裡的規範範圍內。

## 新增 Entity 檢查清單（以 `{Domain}` = Domain、`{Entity}` = Entity 為例，對照 §9～§12）

1. `src/{Domain}/{Entity}.cs`：Entity，主鍵 `{Entity}Id`（`Guid`／
   `CreateVersion7()`）＋ `CreateTime`／`UpdateTime`＋業務欄位（用
   `DataAnnotations` 保護，`ErrorMessage` 陳述違反的規則），狀態轉換邏
   輯（若有）寫成方法。
2. `src/{Domain}/I{Entity}Repository.cs`：Repository 介面，方法順序照
   §10 樣板（`Add` → `Update` → `Remove` → `FindBy...` → `FindAll` →
   `FindAllBy...`）。
3. `src/{Domain}/{Domain}Context.cs`：加一個 `I{Entity}Repository` 唯讀
   屬性＋建構子參數，不開新 Context。
4. `src/{Domain}.Accesses/Mock{Entity}Repository.cs`：非持久化實作，
   lock／Search／Execute 分區塊，失敗語意照 §10（Query 回傳值表達找不
   到，Command 丟 `KeyNotFoundException`）。
5. `src/{Domain}.WebApp/Program.cs`：
   `AddSingleton<I{Entity}Repository, Mock{Entity}Repository>()`，放在
   Repository 註冊區塊、`{Domain}Context` 註冊之前；生命週期一律
   Singleton。
6. `src/{Domain}.WebApp/Controllers/{Entity}sController.cs`：CRUD
   action，注入 `{Domain}Context`（不直接注入 `I{Entity}Repository`），
   方法標籤照 §8，路由參數用 `{entity}Id`。
7. Views（若需要 UI）：路由樣板與 `asp-route-{entity}Id` 對應
   `{Entity}Id`。

## 自我檢查清單（寫完程式碼後逐條核對）

清單顆粒度跟到每一條規則，逐項打勾；只有命名鏈（§5～§7）這種本來就是
同一件事的不同面向才合併成一條。

**§1 Workspace**

- [ ] `src/` 有放所有原始碼專案。
- [ ] `docs/` 有放架構規則／設計文件這類跟程式碼相關但不參與建置的文件。
- [ ] `tests/` 有放測試專案，跟 `src/` 同一層。
- [ ] `README.md` 放在 repo 根目錄。
- [ ] `LICENSE` 放在 repo 根目錄。
- [ ] `.gitignore` 放在 repo 根目錄。
- [ ] repo 根目錄沒有 `.sln`／專案檔案，只留跟建置無關的東西。

**§2 Architecture**

- [ ] `{Domain}` 是類別庫專案，只提供 Entity／Repository 介面／Context，
      不依賴任何框架。
- [ ] `{Domain}.Accesses` 是類別庫專案，只提供 Repository 介面的實作。
- [ ] `{Domain}.WebApp`（若有）是 ASP.NET MVC 專案。
- [ ] `{Domain}.BlazorApp`（若有）是 Blazor 專案。
- [ ] `{Domain}.ConsoleApp`（若有）是 Console 專案，無使用者介面。
- [ ] 分層相依方向正確：Domain 不相依任何層；Access 相依 Domain；Host
      相依 Domain＋Access。
- [ ] 同一個 `{Domain}` 下多個 Host 層專案都依賴同一個
      `{Domain}.Accesses`／`{Domain}`，沒有各自另開一份。
- [ ] `.sln` 跟所有專案同一層，不在 repo 根目錄。
- [ ] 多組 `{Domain}` 彼此獨立，沒有共用 Repository 介面或 Context。

**§3 Namespace**

- [ ] Namespace 一律等於專案名稱本身，不因資料夾而加後綴。
- [ ] 一律 file-scoped 寫法。
- [ ] `using` 放最上面，跟 `namespace` 間空一行，順序沒有反過來。
- [ ] `{Domain}`／`{Domain}.Accesses` 沒拆資料夾；`{Domain}.WebApp` 維
      持 MVC 慣例資料夾。

**§4 Class 成員排序**

- [ ] 成員照 Singleton／Imports／Constants／Enumeration／Fields／
      Constructors／Properties／Methods／Operators／Handlers／Events
      順序排列，只列出實際有內容的分類。
- [ ] 分類標頭下第一個成員不空行，同分類成員間空一行，不同分類間空兩
      行。
- [ ] `Imports` 沒有另外加標頭、沒算進分類清單。
- [ ] 類別內部呼叫自己（含繼承來）的方法／屬性都沒有加 `this.` 前綴。
- [ ] 這條排序規則沒有套用到 `Program.cs` 這種 top-level statements 檔
      案。

**§5～§7 命名鏈與 Field／Constructor／Properties**

- [ ] 欄位／參數／屬性命名鏈（`I{X}` → `{x}` 參數 → `_{x}` 欄位 →
      `{X}` 屬性）一致。
- [ ] `// Fields` 分類內 lock 物件排最前面。
- [ ] 建構子：`// Contracts` 檢查 not-null → 空一行 → `// Default` 賦
      值進欄位；先用私有欄位承接注入物件，沒有繞過欄位直接賦值給屬性。
- [ ] 屬性一律用完整 `get`／`return` 寫法（自動屬性除外）；單行 `get`
      沒有加 `// Return`。

**§8 Method**

- [ ] 方法本體用平鋪區塊＋單字標籤，沒有深巢狀，標籤沒有把邏輯寫進註
      解裡；標籤有從既定詞彙表挑，沒有自創跟既有詞彙重疊的新詞。
- [ ] `// Contracts` 放在方法（或建構子）本體最前面。
- [ ] 參照型別參數有 `ArgumentNullException.ThrowIfNull` 檢查。
- [ ] 字串參數有 `ArgumentException.ThrowIfNullOrWhiteSpace` 檢查。
- [ ] 值型別參數沒有多餘檢查。
- [ ] MVC 驗證判斷（`ModelState.IsValid` 等）有放進同一個 `// Contracts`
      底下。
- [ ] `// Contracts` 內每個檢查獨立一行，沒用 `||`／`&&` 合併。
- [ ] `// Return` 只標最後交付結果的 return；guard clause 式提前
      return、`void` 方法都沒有誤標。
- [ ] guard clause 省略大括號、寫成一行；只有一步驟的方法沒有多加標
      籤。

**§9 Context**

- [ ] `{Domain}Context` 把所有 Repository 介面透過建構子注入，用唯讀
      屬性對外提供。
- [ ] 沒有為新 Entity 另開 Context，是在既有 Context 上多加屬性。
- [ ] 外部呼叫端注入 `{Domain}Context`，沒有直接注入個別 Repository 介
      面。

**§10 Repository**

- [ ] 介面／實作命名符合 `I{Entity}Repository`／`Mock{Entity}...`／
      `{技術}{Entity}...` 規則。
- [ ] 方法順序：新增 → 修改 → 刪除 → 查單筆 → 查全部 → 查全部（有條
      件）。
- [ ] 刪除方法叫 `Remove`，不是 `Delete`。
- [ ] 查詢方法用 `Find`／`FindAll` 開頭，回傳型別正確（`{Entity}?`／
      `IReadOnlyList<{Entity}>`）。
- [ ] 方法簽章跟 §10 樣板一致。
- [ ] `Add` 沒有多餘檢查主鍵是否已存在。
- [ ] `Add` 有把 `CreateTime`／`UpdateTime` 都蓋上 `DateTime.UtcNow`。
- [ ] `Update` 有維持 `CreateTime`、只蓋 `UpdateTime`。
- [ ] 失敗語意沒有混用：Query 用回傳值表達找不到，Command 丟
      `KeyNotFoundException`。
- [ ] 沒有自己發明例外攔截／HTTP 回應轉換邏輯（這部分留待之後規則）。
- [ ] Repository 沒有放業務邏輯／狀態轉換方法（沒有 `Toggle{XX}` 這類
      方法）。
- [ ] 真實資料庫實作有注入 `IDbContextFactory`，每個方法用
      `using (...) { }` 建立／`Dispose` 短命 `DbContext`。

**§11 Entity**

- [ ] 主鍵屬性叫 `{Entity}Id`（不是 `Id`），型別 `Guid`，用
      `Guid.CreateVersion7()` 產生。
- [ ] 主鍵變數／參數統一用 `{entity}Id`（Repository／Controller／路由／
      View 都一致），沒有留通用的 `id`。
- [ ] 稽核時間戳記叫 `CreateTime`／`UpdateTime`（不是 `CreatedAt`），型
      別 `DateTime`，一律存 UTC。
- [ ] Entity 狀態轉換方法沒有自己碰 `UpdateTime`。
- [ ] 跟 Entity 自身狀態有關的業務邏輯寫成 Entity 方法，沒有寫在
      Repository 或 Controller 裡。
- [ ] 呼叫端流程：查出 Entity → 呼叫 Entity 方法 → 存回去，沒有跳過
      Entity 直接改欄位。
- [ ] 業務欄位都有 `DataAnnotations` 保護，沒有只靠
      `ModelState.IsValid` 裸檢查或手寫等價 if 判斷。
- [ ] 不可為空白的字串屬性都有
      `[Required(ErrorMessage = "不可以為空白")]`，沒有只靠
      `[StringLength]` 頂替。
- [ ] `ErrorMessage` 是陳述式（`不可為空`／`不可超過…`），沒有祈使句
      （`請輸入…`）。

**§12 Dependency Injection**

- [ ] Domain／Access 層在 DI 容器都用 Singleton 生命週期註冊。
- [ ] 所有 DI 註冊都寫在 Host 層 `Program.cs`。
- [ ] 註冊順序：先 Repository 介面對實作，再 `{Domain}Context`。
- [ ] 非持久化實作用
      `AddSingleton<I{Entity}Repository, Mock{Entity}Repository>()`。
- [ ] 真實資料庫實作用 `AddDbContextFactory<{Domain}DbContext>()` 搭配
      `AddSingleton<I{Entity}Repository, Ef{Entity}Repository>()`。
- [ ] `{Domain}Context` 用 `AddSingleton<{Domain}Context>()` 註冊。
- [ ] 沒有去管 Host 層本身的生命週期（那是 ASP.NET Core 框架管的）。
