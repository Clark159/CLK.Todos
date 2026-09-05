---
name: mdp-dotnet-architecture
description: .NET 專案通用設計規範（目錄分層、Namespace、Class 成員排序、Constructor/Method 慣例、Context／Repository／Entity／DI 規則）。撰寫或修改任何 .NET 專案的 .cs 檔案（含 Domain／Access／Host 層）前必讀，確保產出符合本規範。
user-invocable: true
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

- `src/`：所有原始碼專案放這裡，`.sln` 跟所有專案同一層，不放 repo 根目
  錄；測試專案放 `tests/`，跟 `src/` 同一層。
- 一個 `.sln` 可以包含多個 `{Domain}`（多個 Bounded Context）：`.sln` 檔名
  對應 repo 本身，不綁定任何一個 `{Domain}`。
- `docs/`：架構規則、設計文件這類跟程式碼相關但不參與建置的文件放這裡。
- `README.md`、`.gitignore` 放 repo 根目錄。
- repo 根目錄只留上述這類跟建置無關的東西，不放任何 `.sln`／專案檔案。

## 02. Architecture（分層）

- `{Domain}`：Domain 層，類別庫專案。提供 Entity、Repository 介面與
  Context，定義業務核心邏輯與規格，不依賴任何框架。
- `{Domain}.Accesses`：Access 層，類別庫專案。提供 Repository 介面的實
  作，負責實際資料存取（資料庫、記憶體等）。
- `{Domain}.WebApp`：Host 層，ASP.NET MVC 專案，提供網頁使用者介面。
- `{Domain}.BlazorApp`：Host 層，Blazor 專案，提供互動式網頁應用的使用者
  介面。
- `{Domain}.ConsoleApp`：Host 層，Console 專案，提供命令列工具（例如批次
  匯入、排程工作），無使用者介面。
- 分層相依：Domain 層不相依其他任何層；Access 層相依 Domain 層；Host 層
  相依 Domain 層＋Access 層。
- 同一個 `{Domain}` 可以同時有多個 Host 層專案，都依賴同一個
  `{Domain}.Accesses`／`{Domain}`。
- 一個 `.sln` 可以同時有多組 `{Domain}`／`{Domain}.Accesses`／Host 層專
  案，各組 `{Domain}` 之間彼此獨立、不共用 Repository 介面或 Context。

## 03. Namespace

- 每個專案內所有檔案，namespace 一律等於專案名稱本身，不因資料夾而加後
  綴。
- 一律用 file-scoped 寫法（`namespace X;`，不用大括號區塊）。
- `using` 放在檔案最上面，跟 `namespace` 之間空一行（C# 官方範本順序）；
  沒有 `using` 的檔案直接從 `namespace` 開始。
- 資料夾要不要拆，依專案性質決定：`{Domain}`、`{Domain}.Accesses` 不拆資
  料夾，檔案直接放專案根目錄；`{Domain}.WebApp`（Host 層）維持 MVC 慣例
  資料夾（`Controllers/`／`Models/`／`Views/`）。不論拆不拆，資料夾都只
  是物理分類，不影響 namespace。

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
  `get` 縮成一行寫。

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
  式的提前 `return`（例如找不到資料就提前回應）不用標。回傳型別是
  `void` 的方法（例如找不到就丟例外）沒有交付結果，不需要 `// Return`。
- guard clause 一律省略大括號、寫成一行（不只限於 `// Contracts`，方法
  本體任何「不合法／找不到就提前 return 或丟例外」都比照辦理）。
- 只有一個步驟的方法（例如單筆查詢）不需要額外標籤，`// Return` 本身就
  夠。

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
  `{技術}{Entity}{介面名}`（例如 EF Core 實作）。
- 方法順序固定：新增 → 修改 → 刪除 → 查單筆 → 查全部 → 查全部（有條
  件）。
- 刪除方法一律叫 `Remove`，不叫 `Delete`。
- 查詢命名：`Find` 開頭查單筆（回傳 `{Entity}?`）；`FindAll` 開頭查多筆
  （回傳 `IReadOnlyList<{Entity}>`）。

```csharp
void Add({Entity} entity);

void Update({Entity} entity);

void Remove(Guid {entity}Id);

{Entity}? FindByXX(Guid {entity}Id);

IReadOnlyList<{Entity}> FindAll();

IReadOnlyList<{Entity}> FindAllByXX();
```

- `Add` 不用額外檢查主鍵是否已存在（外部提供的 GUIDv7 碰撞機率低到可忽
  略）。
- `Add` 把 `CreateTime`／`UpdateTime` 都重新蓋上 `DateTime.UtcNow`，不信
  任物件建構當下的預設值。
- `Update` 維持 `CreateTime` 既有值不覆寫，只把 `UpdateTime` 重新蓋上，
  不信任呼叫端傳進來的值。
- 失敗語意依方法類型分兩種，不混用：Query 方法（`Find` 開頭）用回傳值
  本身表達「找不到」（`null`／空集合）；Command 方法（`Add`／`Update`／
  `Remove`）一律回傳 `void`，找不到對應資料就直接丟例外
  （`KeyNotFoundException`）——Command 呼叫前理應已經用 `FindById` 確認
  過資料存在，這裡若仍找不到代表資料在兩次操作之間被異動，屬於例外狀
  況。
- Repository 只放存取資料的方法（CRUD＋查詢），不放業務邏輯／狀態轉換。
  不要在 Repository 開 `Toggle{XX}(Guid {entity}Id)` 這種直接用 id 操
  作、把邏輯藏在 Repository 裡的方法。
- 真實資料庫實作建構子注入 `IDbContextFactory<{Domain}DbContext>`，每個
  方法內用 `using (...) { }`（不用 `using var`）建立短命 `DbContext`，
  區塊結束就 `Dispose`——因為 Repository 實作全部走 Singleton，
  `DbContext` 本身不是 thread-safe、不能跟著 Singleton 活整個生命週期。

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
- Entity 狀態轉換方法不用自己碰 `UpdateTime`，經過 Repository 的
  `Update` 就一定會蓋到。
- 充血模型（Rich Domain Model）：跟 Entity 自身狀態有關的業務邏輯，一律
  寫成 Entity 上的方法，不要寫在 Repository 或 Controller 裡——避免邏輯
  散落各處、同一規則被實作兩三次。
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
  使句（不要 `請輸入 XXX`）——跟 Method／Constructor `// Contracts` 的合
  約檢查（`ArgumentException` 系列）用同一種語氣，全專案「合約違規」一
  律陳述規則本身，不指示下一步動作（引導文案屬於 UI 層，該由畫面層另外
  處理）。

```csharp
public class {Entity}
{
    public Guid {Entity}Id { get; set; } = Guid.CreateVersion7();

    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;
}
```

## 12. Dependency Injection

- Domain 層（`{Domain}Context`）與 Access 層（Repository 實作，不論
  Mock 或真實資料庫）在 DI 容器裡一律用 Singleton 生命週期註冊——兩層都
  不持有跟單一 HTTP 請求綁定的狀態，用 Singleton 避免每個請求都重新建立
  一整條相依鏈。
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

- [ ] Namespace file-scoped，且等於專案名稱（§3）。
- [ ] 類別成員分類順序、空行規則正確；不必要的 `this.` 都拿掉（§4）。
- [ ] 欄位／參數／屬性命名鏈（`I{X}` → `{x}` 參數 → `_{x}` 欄位 →
      `{X}` 屬性）一致（§5～§7）。
- [ ] 建構子：`// Contracts` 檢查 not-null → 空一行 → `// Default` 賦值
      進欄位（§6）。
- [ ] 方法內區塊標籤正確、guard clause 省略大括號、`// Return` 只標最
      後交付結果的 return（§8）。
- [ ] Repository 方法順序＋命名（`Find`／`FindAll`）＋失敗語意（Query
      回傳值／Command 丟例外）都符合 §10。
- [ ] Entity 主鍵型別／命名／預設值、稽核時間戳記命名／型別／UTC 都符
      合 §11。
- [ ] 業務欄位都有 `DataAnnotations` 保護，`ErrorMessage` 是陳述式
      （`不可為空`／`不可超過…`），沒有祈使句（`請輸入…`）（§11）。
- [ ] 不可為空白的字串屬性都有 `[Required(ErrorMessage = "不可以為空白")]`，
      沒有只靠 `[StringLength]` 頂替（§11）。
- [ ] `{Domain}Context` 沒有為新 Entity 另開 Context（§9）。
- [ ] DI 註冊都在 `Program.cs`，順序＋生命週期（Singleton）符合 §12。
- [ ] Controller／路由／View 用 `{entity}Id`，不是泛用 `id`（§11）。
