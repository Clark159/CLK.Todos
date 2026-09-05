# .NET 設計規範

這份文件涵蓋 .NET 專案的目錄與分層結構（Workspace／Architecture）、程式碼
撰寫慣例（Namespace／Class／Constructor／Method），以及 Domain 層的設計規範
（Context／Repository／Entity）。

> 每一節統一用「規則 → 範例 → 說明」整理。規則一律用 `{Domain}`、
> `{Entity}` 佔位符描述，不綁定具體名稱。範例具體標註 CLK.Todos 專案裡
> `Todo` 這個 Entity 示範規則的樣子，是規則的一個實例，不是規則本身。

## 目錄

1. [Workspace 設計規範](#1-workspace-設計規範)
2. [Architecture 設計規範](#2-architecture-設計規範)
3. [Namespace 設計規範](#3-namespace-設計規範)
4. [Class 設計規範](#4-class-設計規範)
5. [Field 設計規範](#5-field-設計規範)
6. [Constructor 設計規範](#6-constructor-設計規範)
7. [Properties 設計規範](#7-properties-設計規範)
8. [Method 設計規範](#8-method-設計規範)
9. [Context 設計規範](#9-context-設計規範)
10. [Repository 設計規範](#10-repository-設計規範)
11. [Entity 設計規範](#11-entity-設計規範)
12. [Dependency Injection 設計規範](#12-dependency-injection-設計規範)

## 1. Workspace 設計規範

**規則**

- `src/`：所有原始碼專案放這裡，`.sln` 跟所有專案同一層，不放 repo
  根目錄。之後新增測試專案，一律放 `tests/`，跟 `src/` 同一層。
- 一個 `.sln` 可以包含多個 `{Domain}`（多個 Bounded Context）：`.sln`
  檔名對應 repo 本身，不綁定任何一個 `{Domain}`。
- `docs/`：架構規則、設計文件這類跟程式碼相關但不參與建置的文件放這裡
  （例如這份 `architecture-notes.md`）。
- `README.md`：專案說明，放 repo 根目錄。
- `.gitignore`：Git 版本控制排除清單，放 repo 根目錄。
- repo 根目錄只留上述這類跟建置無關的東西，不放任何 `.sln`／專案檔案。

**範例**

- repo 根目錄配置（本專案：CLK.Todos，目前只有 `Todo` 一個 `{Domain}`）

```
CLK.Todos/                   repo 根目錄
├── README.md
├── .gitignore
├── docs/
│   └── architecture-notes.md
└── src/
    ├── CLK.Todos.sln
    ├── CLK.Todos/
    ├── CLK.Todos.Accesses/
    └── CLK.Todos.WebApp/
```

**說明**

依用途分類，避免根目錄堆滿雜項檔案。

## 2. Architecture 設計規範

**規則**

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
- 一個 `.sln` 可以同時有多組 `{Domain}`／`{Domain}.Accesses`／Host 層
  專案，各組 `{Domain}` 之間彼此獨立、不共用 Repository 介面或 Context。

**範例**

- `src/` 分層對應（本專案：CLK.Todos）

```
src/
├── CLK.Todos.sln
├── CLK.Todos/              Domain 層
├── CLK.Todos.Accesses/     Access 層
└── CLK.Todos.WebApp/       Host 層
```

**說明**

分層相依單向、無循環參照，抽換任一層不影響下層；Domain 不依賴框架，可被
多個 Host 層專案重複使用；介面與實作分離，換實作不動 Domain。

## 3. Namespace 設計規範

**規則**

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

**範例**

- file-scoped namespace 搭配 `using`

```csharp
using System.ComponentModel.DataAnnotations;

namespace CLK.Todos;

public class Todo
{
    // ...
}
```

**說明**

namespace 等於專案名稱，一個 `using` 就能取用整個專案型別，不受資料夾搬移
影響；file-scoped 寫法減少縮排；`using`／`namespace` 順序符合官方慣例，
避免跟 IDE、`dotnet format` 打架。`{Domain}.WebApp` 維持 MVC 慣例資料夾是
因為 Razor 慣例路由依賴這個結構，屬於框架限制，不是這裡自訂的規則。

## 4. Class 設計規範

**規則**

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

**範例**

- `TodoContext` 的成員排序

```csharp
public class TodoContext
{
    // Fields
    private readonly ITodoRepository _todoRepository;


    // Constructors
    public TodoContext(ITodoRepository todoRepository)
    {
        _todoRepository = todoRepository;
    }


    // Properties
    public ITodoRepository TodoRepository
    {
        get
        {
            return _todoRepository;
        }
    }
}
```

**說明**

固定成員順序讓任何類別檔案都一致好讀；分類標頭方便快速定位（例如找
`// Fields`）；省略 `this.` 減少視覺雜訊——欄位已經靠 `_` 前綴、參數靠
命名跟成員區分，不需要 `this.` 才能辨識。

## 5. Field 設計規範

**規則**

- 命名：`_` + 參數命名（注入型別去掉介面字首 `I`，第一個字母轉小寫）。
- `// Fields` 分類內，lock 物件排最前面，其餘欄位接在後面。

**範例**

- `MockTodoRepository` 的 lock 物件排最前面

```csharp
public class MockTodoRepository : ITodoRepository
{
    // Fields
    private readonly object _lock = new();

    private readonly List<Todo> _todos = new();
}
```

**說明**

欄位命名可直接從注入型別反推，不用另外設計命名規則；`private readonly`
避免建構後被意外改動；lock 物件排最前面，一眼就能看出這個類別有執行緒
安全考量。

## 6. Constructor 設計規範

適用於所有類別，不限於某一層。

**規則**

- 參數命名：注入型別去掉介面字首 `I`（如果是介面），第一個字母轉小寫。
- `// Default`：合約檢查後、把參數存進欄位的賦值陳述式，前面加這個
  標籤，跟 `// Contracts` 空一行、跟賦值本身不空行。
- 一律先用私有欄位承接注入物件，不要繞過欄位直接賦值給屬性或到處傳遞。
- 合約檢查放方法本體最前面，用 `// Contracts` 標籤，格式規則跟方法共用；
  建構子最常見的是參照型別參數的 not-null 檢查。

**範例**

- `TodoContext` → 參數 `todoContext` → 欄位 `_todoContext`
- `ITodoRepository` → 參數 `todoRepository` → 欄位 `_todoRepository` →
  屬性 `TodoRepository`

```csharp
public class TodoContext
{
    private readonly ITodoRepository _todoRepository;

    public TodoContext(ITodoRepository todoRepository)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(todoRepository);

        // Default
        _todoRepository = todoRepository;
    }

    public ITodoRepository TodoRepository
    {
        get { return _todoRepository; }
    }
}
```

**說明**

`// Default` 讓建構子清楚分成合約檢查跟賦值兩階段。

## 7. Properties 設計規範

**規則**

- 公開屬性命名：注入型別去掉介面字首 `I`，維持 PascalCase（不轉小寫）。
- 一律用完整的 `get` / `return` 寫法，不用 `=>` expression-bodied（自動
  屬性 `{ get; set; }` 不受影響）。
- `get` 如果整個只有一行（只有一個 `return`），不加 `// Return`，整個
  `get` 縮成一行寫，比展開成三行好讀。

**範例**

- 單行屬性寫法

```csharp
public ITodoRepository TodoRepository
{
    get { return _todoRepository; }
}
```

**說明**

屬性命名可直接從注入型別反推，跟建構子／欄位的命名鏈一致；完整
`get`／`return` 寫法在需要展開邏輯時保持一致風格，單行屬性縮成一行則避免
不必要的視覺膨脹。

## 8. Method 設計規範

**規則**

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
  clause 式的提前 `return`（例如 `if (todo is null) return NotFound();`）
  不用標。回傳型別是 `void` 的方法（例如 `Update`／`Remove` 找不到就丟
  例外）沒有交付結果，不需要 `// Return`。
- guard clause 一律省略大括號、寫成一行（不只限於 `// Contracts`，
  方法本體任何「不合法／找不到就提前 return 或丟例外」都比照辦理）。
- 只有一個步驟的方法（例如 `FindById`）不需要額外標籤，`// Return` 本身
  就夠。

**範例**

- `MockTodoRepository.Update`（查詢用 `// Search`、寫入用 `// Execute`；
  找不到就丟例外，方法回傳 `void`）

```csharp
public void Update(Todo todo)
{
    // Contracts
    ArgumentNullException.ThrowIfNull(todo);

    // Lock
    lock (_lock)
    {
        // Search
        var entity = _todos.FirstOrDefault(t => t.TodoId == todo.TodoId);
        if (entity is null) throw new KeyNotFoundException($"Todo not found: {todo.TodoId}");

        // Execute
        entity.Title = todo.Title;
        entity.IsDone = todo.IsDone;
        entity.UpdateTime = DateTime.UtcNow;
    }
}
```

- `TodosController.Create`（`// Contracts` 同時含 not-null 檢查與 MVC 驗證）

```csharp
public IActionResult Create([Bind("Title")] Todo? todo = null)
{
    // Contracts
    ArgumentNullException.ThrowIfNull(todo);
    if (!ModelState.IsValid) return View(todo);

    // Execute
    _todoContext.TodoRepository.Add(todo);

    // Return
    return RedirectToAction(nameof(Index));
}
```

**說明**

單字標籤讓方法步驟邊界一眼可辨，不用逐行讀；統一詞彙表避免同一操作各自
表述（`FindById` vs. `Query`）；`// Contracts`／`// Return` 分別把輸入檢查、
正常結果跟其他邏輯區隔開來。guard clause 式的提前 `return` 不標
`// Return`，是因為已經被 `if` 自我解釋，不需要標籤重複說明。

## 9. Context 設計規範

**規則**

- `{Domain}Context` 是 Domain 的入口物件：把所有 Repository 介面透過建構子
  注入進來，用唯讀屬性對外提供。
- `{Domain}Context` 每個 `{Domain}` 只有一個，不隨 Entity 數量增加而變多：
  同一個 `{Domain}` 新增 Entity 時，是在既有的 Context 上多加一個
  Repository 屬性，不是另外開一個新的 Context；一個 `.sln` 有多個
  `{Domain}` 時，每個 `{Domain}` 各自有自己的 Context，彼此不共用。
- 外部呼叫端（Controller 等）一律注入 `{Domain}Context` 來使用 Repository，
  不直接注入個別 Repository 介面。

**範例**

- `TodoContext`：把 `ITodoRepository` 透過建構子注入進來，用唯讀屬性對外
  提供

```csharp
public class TodoContext
{
    // Fields
    private readonly ITodoRepository _todoRepository;


    // Constructors
    public TodoContext(ITodoRepository todoRepository)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(todoRepository);

        // Default
        _todoRepository = todoRepository;
    }


    // Properties
    public ITodoRepository TodoRepository
    {
        get { return _todoRepository; }
    }
}
```

**說明**

單一入口物件集中所有 Repository，呼叫端只需注入一個依賴；Context 的數量
跟著 `{Domain}` 走，不跟著 Entity 走，避免 Entity 一多就要到處新增、注入
一堆 Context。本專案：CLK.Todos 目前只有 `Todo` 一個 `{Domain}`，`{Domain}`
新增 `Category` Entity 時，是在 `TodoContext` 上多加一個
`ICategoryRepository` 屬性，不是另外開一個 `CategoryContext`；
`TodoContext` 剛好跟 `{Entity}Context` 同名只是巧合，不代表 Context 是
跟著 Entity 命名的。

## 10. Repository 設計規範

**規則**

- 命名：介面 `I{Entity}Repository`；非持久化實作 `Mock{Entity}{介面名}`；
  真實資料庫實作（尚未使用，先預留）`{技術}{Entity}{介面名}`（例如
  `EfTodoRepository`）。
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

**範例**

- 命名：`ITodoRepository`、`MockTodoRepository`。
- `EfTodoRepository`：搭配 `IDbContextFactory`

```csharp
public class EfTodoRepository : ITodoRepository
{
    // Fields
    private readonly IDbContextFactory<TodoDbContext> _todoDbContextFactory;


    // Constructors
    public EfTodoRepository(IDbContextFactory<TodoDbContext> todoDbContextFactory)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(todoDbContextFactory);

        // Default
        _todoDbContextFactory = todoDbContextFactory;
    }


    // Methods
    public void Add(Todo todo)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(todo);

        // Create
        using (var todoDbContext = _todoDbContextFactory.CreateDbContext())
        {
            // Execute
            todo.CreateTime = DateTime.UtcNow;
            todo.UpdateTime = DateTime.UtcNow;
            todoDbContext.Todos.Add(todo);
            todoDbContext.SaveChanges();
        }
    }
}
```

**說明**

介面／實作分開命名，一看類別名稱就知道它是不是「真的會持久化」，避免
Mock 跟真正接資料庫的實作混用同一套命名造成誤會；固定方法順序與命名規則，
讓任何 Repository 介面都好找、好猜回傳型別；業務邏輯留在 Entity（充血
模型），Repository 維持單純的存取角色。

主鍵不用檢查重複，是因為外部提供的 GUIDv7 碰撞機率低到可忽略；稽核時間戳記
統一由 Repository 的 `Add`／`Update` 蓋，不用各呼叫端各自處理——`Add` 不
信任物件建構當下的預設值、`Update` 不信任呼叫端傳進來的值，都是為了避免
呼叫端忘記蓋或蓋錯。

Query／Command 失敗語意分開，是因為兩者「找不到」的意義不同：Query 呼叫端
本來就不確定資料在不在，找不到是正常結果的一部分；Command 呼叫前理應已經
用 `FindById` 確認過資料存在，這裡若仍找不到代表資料在兩次操作之間被異動
（例如被其他請求刪除），屬於例外狀況，用例外表達比回傳 `bool` 更能凸顯
「這不是預期路徑」。

`DbContext` 本身不是 thread-safe、生命週期本應是 Scoped，但這個專案的
Repository 實作全部走 Singleton，所以改成建構子注入 `IDbContextFactory`，
把「取得 `DbContext`」這件事下放到每個方法自己處理（各自建立、各自
`Dispose`），不用為了 EF 另開一套 Scoped 的規則。

## 11. Entity 設計規範

**規則**

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
- Entity 狀態轉換方法（例如 `ToggleDone()`）不用自己碰 `UpdateTime`，
  經過 Repository 的 `Update` 就一定會蓋到。
- 充血模型（Rich Domain Model）：跟 Entity 自身狀態有關的業務邏輯，一律
  寫成 Entity 上的方法，不要寫在 Repository 或 Controller 裡。
- 呼叫端標準流程：Repository 查出 Entity → 呼叫 Entity 方法改變狀態 →
  Repository 存回去。
- 業務欄位（非主鍵、非稽核時間戳記）一律用
  `System.ComponentModel.DataAnnotations` 屬性保護輸入合法性，依型別與
  業務規則挑選（例如 `[Required]`、`[StringLength]`、`[EmailAddress]`），
  不能只靠 Controller 的 `ModelState.IsValid` 裸檢查，也不用自己在 Entity
  方法或 Controller 裡手寫等價的 if 判斷。
- `ErrorMessage` 一律用「參數驗證」角度撰寫：陳述「這個屬性違反了什麼
  規則」（`{屬性} 不可為空`、`{屬性} 長度不可超過 N 字`），不要用引導使用者
  操作的祈使句（不要 `請輸入 XXX`）。跟 Method／Constructor `// Contracts`
  的合約檢查（`ArgumentException` 系列）用同一種語氣，全專案「合約違規」
  一律陳述規則本身，不指示下一步動作。

**範例**

- 主鍵與稽核時間戳記

```csharp
public class Todo
{
    public Guid TodoId { get; set; } = Guid.CreateVersion7();

    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;
}
```

- DataAnnotations 保護業務欄位（本專案：`User.Email`），`ErrorMessage`
  陳述違反的規則，不指示使用者動作

```csharp
public class User
{
    [Required(ErrorMessage = "Email 不可為空")]
    [EmailAddress(ErrorMessage = "Email 格式不正確")]
    [StringLength(100, ErrorMessage = "Email 長度不可超過 100 字")]
    public string Email { get; set; } = string.Empty;
}
```

- 充血模型（本專案：切換完成狀態邏輯在 `Todo` 上，不在 Repository）

```csharp
public class Todo
{
    public bool IsDone { get; set; }

    public void ToggleDone()
    {
        IsDone = !IsDone;
    }
}
```

- 呼叫端標準流程

```csharp
public IActionResult Toggle(Guid todoId)
{
    var todo = _todoContext.TodoRepository.FindById(todoId);
    if (todo is null) return NotFound();

    todo.ToggleDone();
    _todoContext.TodoRepository.Update(todo);

    return RedirectToAction(nameof(Index));
}
```

**說明**

`{Entity}Id` 一看就知道屬於哪個 Entity；GUIDv7 可在用戶端產生、天生依建立
時間排序，比 `int` 流水號更適合當索引鍵、也更安全。UTC 是不受時區影響的
絕對基準，跨伺服器也一致。避免「貧血模型」（Entity 只有屬性沒有行為，邏輯
散落在 Service／Controller／Repository 各處，同一規則被實作兩三次）——
邏輯跟著資料放在一起，不管從 Controller、Console 工具還是測試呼叫，規則
都保證一致。

DataAnnotations 標註屬性本身，讓合法性規則跟著 Entity 走，不因為呼叫端
（MVC Controller、Console 工具、測試）各自重寫一套判斷而不一致，也不用
等進了 Repository／資料庫才發現值不合法。`ErrorMessage` 用陳述式而不是
祈使句，是要跟 Method／Constructor 的合約檢查訊息統一語氣——「合約」講的
是規則被違反了，不是在教使用者下一步要做什麼；後者屬於 UI 文案，該由畫面
層（例如 `asp-validation-for` 旁邊另外顯示提示文字）處理，不該混進合約
訊息裡。

## 12. Dependency Injection 設計規範

**規則**

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

**範例**

- `Program.cs`（非持久化實作）

```csharp
builder.Services.AddSingleton<ITodoRepository, MockTodoRepository>();
builder.Services.AddSingleton<TodoContext>();
```

- `Program.cs`（真實資料庫實作）

```csharp
builder.Services.AddDbContextFactory<TodoDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddSingleton<ITodoRepository, EfTodoRepository>();
builder.Services.AddSingleton<TodoContext>();
```

**說明**

Domain／Access 兩層都不持有跟單一 HTTP 請求綁定的狀態——Repository 只是
資料存取的入口，Context 只是聚合這些入口——用 Singleton 可以避免每個請求
都重新建立一整條相依鏈；也讓「所有 Repository 實作的生命週期規則一致」這
件事，不因為 Mock 換成真實資料庫實作而被打破。
