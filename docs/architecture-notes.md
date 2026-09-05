# .NET 架構規則

這份文件是 .NET 分層架構的規則，適用於 Domain／Accesses 這類共用層，也適用於
各種進入點專案（ASP.NET MVC、Console、Blazor 等）。規則異動時直接更新對應
章節，不用時間順序流水帳記錄，避免舊規則跟新規則互相矛盾。

> 每一節統一用「規則 → 範例 → 說明」整理。**規則一律用 `{Solution}`、
> `{Entity}` 佔位符描述，不綁定具體名稱**。**範例**具體標註本專案
> （CLK.Todos）用 `Todo` 這個 Entity 示範規則的樣子，是規則的**一個實例**，
> 不是規則本身。

## 目錄

1. [Architecture 設計規則](#1-architecture-設計規則)
2. [Namespace 設計規則](#2-namespace-設計規則)
3. [Class 設計規則](#3-class-設計規則)
4. [Constructor 設計規則](#4-constructor-設計規則)
5. [Method 設計規則](#5-method-設計規則)
6. [Context 設計規則](#6-context-設計規則)
7. [Repository 設計規則](#7-repository-設計規則)
8. [Entity 設計規則](#8-entity-設計規則)

## 1. Architecture 設計規則

**規則**

- 三層專案，相依方向單向：`{Solution}.WebApp` → `{Solution}.Accesses` →
  `{Solution}`。`{Solution}.sln` 跟三個專案放在 `src/` 底下，不放 repo
  根目錄（根目錄只留 `README.md`、`.gitignore`、`docs/` 這類跟建置無關的東西）。
- **`{Solution}`（Domain）**：純類別庫，**只依賴 .NET BCL**，不參照 ASP.NET
  Core。不拆資料夾，檔案直接放根目錄，靠 namespace（見第 2 節）辨識：
  - Entity（`{Entity}.cs`，規則見「Entity 設計規則」）、Repository 介面
    （`I{Entity}Repository`，只放介面不放實作，方法規則見「Repository 設計
    規則」）。
  - `{Solution}Context`：Domain 入口物件，規則見「Context 設計規則」。
- **`{Solution}.Accesses`**：獨立類別庫，參照 `{Solution}`，專門放 Repository
  介面的實作，一樣不拆資料夾。
- **`{Solution}.WebApp`**：表現層，**維持 MVC 慣例資料夾**
  （`Controllers/`／`Models/`／`Views/`），因為 Razor 慣例路由依賴這個結構。
  只有這層需要靠資料夾分類，其餘兩層靠 namespace 就夠。
- **可執行的進入點專案命名**：一律 `{Solution}.{類型}App`（ASP.NET MVC 網站
  是 `{Solution}.WebApp`；Console 程式尚未使用，先預留 `{Solution}.ConsoleApp`）
  ——跟 Domain／Accesses 這類不能單獨執行的類別庫明確區分。
- **DI 註冊**（在 `{Solution}.WebApp/Program.cs`）：每個 Repository 介面對
  實作註冊一次 `AddSingleton<I{Entity}Repository, Mock{Entity}Repository>()`，
  `{Solution}Context` 本身也要註冊 `AddSingleton<{Solution}Context>()`。

**範例**

```
src/
├── CLK.Todos.sln
├── CLK.Todos/              Domain 層
│   ├── TodoContext.cs
│   ├── Todo.cs
│   └── ITodoRepository.cs
├── CLK.Todos.Accesses/     資料存取實作層
│   └── MockTodoRepository.cs
└── CLK.Todos.WebApp/       表現層
    ├── Controllers/
    ├── Models/
    └── Views/
```

```csharp
builder.Services.AddSingleton<ITodoRepository, MockTodoRepository>();
builder.Services.AddSingleton<TodoContext>();
```

**說明**

相依方向單向、沒有循環參照，任何一層要抽換都不牽動它下面的層；Domain 不依賴
框架，之後才能被其他專案型態（Console、Worker）重複使用；「規格」（介面）跟
「實作」分開，換實作時 Domain 完全不用改。

## 2. Namespace 設計規則

**規則**

- **每個專案內所有檔案，namespace 一律等於專案名稱本身，不因資料夾而加後綴**
  （例如 `{Solution}.WebApp` 裡不管檔案放在哪個資料夾，namespace 都是
  `{Solution}.WebApp`）。
- **一律用 file-scoped 寫法**（`namespace X;`，不用大括號區塊）。
- **`using` 放在檔案最上面，跟 `namespace` 之間空一行**（C# 官方範本順序，
  不要反過來）；沒有 `using` 的檔案直接從 `namespace` 開始。

**範例**

```csharp
using System.ComponentModel.DataAnnotations;

namespace CLK.Todos;

public class Todo
{
    // ...
}
```

**說明**

namespace 統一等於專案名稱，一個 `using {專案名稱}` 就能拿到整個專案的型別，
不用因為挪動檔案到不同資料夾就要跟著改 namespace；file-scoped 寫法少一層
縮排；`using`／`namespace` 順序維持官方慣例，IDE 自動排序、`dotnet format`
才不會跟專案慣例打架。

## 3. Class 設計規則

**規則**

- 類別（含介面）內成員照以下順序分類，**只列出實際有內容的分類**：

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
- `Imports`（`using`）不算進分類清單、不加標頭，位置規則見第 2 節。
- **屬性一律用完整 `get` / `return`，不用 `=>` expression-bodied**（自動屬性
  `{ get; set; }` 不受影響）。
- **公開屬性命名**：注入型別去掉介面字首 `I`，維持 PascalCase（不轉小寫）。
- **`// Fields` 內，lock 物件排最前面**，其餘欄位接在後面——代表這個類別
  有執行緒安全考量。
- **類別內部呼叫自己（含繼承來）的方法或屬性，一律不加 `this.` 前綴**——
  欄位已經靠 `_` 前綴、參數靠命名跟成員區分，不需要 `this.` 才能辨識。
  Controller 繼承自 `Controller` 的成員（`ModelState`、`HttpContext`、
  `View(...)`、`RedirectToAction(...)`）也一律不加。
- 成員排序規則只管類別／介面**內部**排序，`Program.cs` 這種 top-level
  statements 進入點檔案不適用。

**範例**

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

`MockTodoRepository` 的 `_lock` 排在 `_todos` 前面，就是「lock 排最前面」
的實例。

**說明**

打開任何一個類別檔案，成員排列順序都一樣，不用每次重新適應；分類標頭也讓人
能快速跳到想看的區塊（例如只想看依賴，直接找 `// Fields`）；`this.` 在這個
專案裡沒有實質區分作用，省略後每一行少一截視覺雜訊。

## 4. Constructor 設計規則

適用於**所有**類別，不限於某一層。

**規則**

1. **一律先用私有欄位承接注入物件**，不要繞過欄位直接賦值給屬性或到處傳遞。
2. **參數命名**：注入型別去掉介面字首 `I`（如果是介面），第一個字母轉小寫。
3. **私有欄位命名**：`_` + 參數命名。
4. **合約檢查放方法本體最前面**，用 `// Contracts` 標籤，格式規則跟方法共用，
   見「Method 設計規則」；建構子最常見的是參照型別參數的 not-null 檢查。
5. **`// Default`**：合約檢查後、把參數存進欄位的賦值陳述式，前面加這個
   標籤，跟 `// Contracts` 空一行、跟賦值本身不空行。

**範例**

- `TodoContext` → 參數 `todoContext` → 欄位 `_todoContext`
- `ITodoRepository` → 參數 `todoRepository` → 欄位 `_todoRepository` →
  屬性 `TodoRepository`（屬性命名規則見「Class 設計規則」）

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

參數名、欄位名都能從「注入的型別是什麼」直接反推出來，不用每個類別自己想
一套命名，看程式碼的人也能立刻知道某個欄位裝的是什麼型別；`// Default`
讓建構子一眼就能分出「合約檢查」跟「真正做的事（存欄位）」兩個階段。

## 5. Method 設計規則

**規則**

- **方法本體避免深巢狀，拆成平鋪區塊**，每個區塊前面加一個單字小標籤，
  第一個標籤前不空行、後面每個標籤前空一行；**標籤只提示角色，不要把邏輯
  寫進註解裡**。優先從下表挑字：

  | 標籤 | 用途 |
  |---|---|
  | `// Contracts` | 方法／建構子最前面的參數合約檢查 |
  | `// Default` | 建構子專用，見「Constructor 設計規則」 |
  | `// Search` | 查詢資料（例如 Repository 的 `FindById`／`FindAll`） |
  | `// Execute` | 執行核心動作（Repository 的 `Add`／`Update`／`Remove`，或 Entity 狀態轉換） |
  | `// Return` | 方法裡代表「正常結果」的最後一個 `return` |
  | `// Lock` | 進入 `lock` 區塊做執行緒同步 |
  | `// Variables`／`// Initialize`／`// Define`／`// Require`／`// Arguments`／`// Notify`／`// Raise` | 尚無實例，先保留定義，遇到對應場景再套用 |

- **`// Contracts`**：方法（或建構子）參數的合約檢查放最前面：
  - 參照型別參數：不能為 `null` → `ArgumentNullException.ThrowIfNull(參數)`。
  - 字串參數：不能為 `null`／空白 → `ArgumentException.ThrowIfNullOrWhiteSpace(參數)`。
  - 值型別參數（`int`、`bool`、`DateTime`）不需要檢查。
  - MVC 驗證判斷（`ModelState.IsValid`、路由 id 跟表單 id 是否一致）也算
    合約檢查，放進同一個 `// Contracts` 底下。
  - 每個檢查獨立一行，不用 `||`／`&&` 合併；檢查之間不空行，跟後面邏輯空
    一行。
- **`// Return`**：方法裡真正交付結果的最後一個 `return` 才標；guard
  clause 式的提前 `return`（例如 `if (todo is null) return NotFound();`）
  不用標，因為已經被 `if` 自我解釋。屬性 `get` 如果整個只有一行，不加
  `// Return`，`get` 縮成一行寫。
- **guard clause 一律省略大括號、寫成一行**（不只限於 `// Contracts`，
  方法本體任何「不合法／找不到就提前 return」都比照辦理）。
- 只有一個步驟的方法（例如 `FindById`）不需要額外標籤，`// Return` 本身
  就夠。

**範例**

```csharp
public bool Update(Todo todo)
{
    // Contracts
    ArgumentNullException.ThrowIfNull(todo);

    // Lock
    lock (_lock)
    {
        // Search
        var entity = _todos.FirstOrDefault(t => t.TodoId == todo.TodoId);
        if (entity is null) return false;

        // Execute
        entity.Title = todo.Title;
        entity.IsDone = todo.IsDone;
        entity.UpdateTime = DateTime.UtcNow;

        // Return
        return true;
    }
}
```

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

一個單字標籤讓步驟邊界一眼可辨，掃過標籤就知道整個方法的流程，不用逐行讀；
統一詞彙表避免同一種操作在不同地方各自表述（`FindById` vs. `Query` vs.
`Lookup`）；`// Contracts` 讓「輸入的基本要求」跟商業邏輯分開看；`// Return`
把「正常結果」跟「提前擋掉的特殊情況」分開，不用逐行讀邏輯才能找到真正的
輸出在哪裡。

## 6. Context 設計規則

**規則**

- `{Solution}Context` 是 Domain 的入口物件：把所有 Repository 介面透過建構子
  注入進來，用唯讀屬性對外提供（欄位／屬性命名規則見「Constructor 設計規則」／
  「Class 設計規則」）。
- 外部呼叫端（Controller 等）一律注入 `{Solution}Context` 來使用 Repository，
  **不直接注入個別 Repository 介面**。
- 在 `{Solution}.WebApp/Program.cs` 用 `AddSingleton<{Solution}Context>()`
  註冊（跟 Repository 介面對實作的註冊一起做，見第 1 節）。

**範例**

建構子注入與屬性寫法見「Constructor 設計規則」的 `TodoContext` 範例；DI 註冊：

```csharp
builder.Services.AddSingleton<TodoContext>();
```

**說明**

單一入口物件集中所有 Repository，呼叫端只要注入一個東西，不用每加一個
Entity 就多一個要注入的介面；行為上也貼近未來若導入 EF Core 的 `DbContext`
用法，轉換成本較低。

## 7. Repository 設計規則

**規則**

- **命名**：介面 `I{Entity}Repository`；非持久化實作 `Mock{Entity}{介面名}`；
  真實資料庫實作（尚未使用，先預留）`{技術}{Entity}{介面名}`（例如
  `EfTodoRepository`）——一看類別名稱就知道它是不是「真的會持久化」，避免
  Mock 跟真正接資料庫的實作混用同一套命名造成誤會。
- **方法順序固定**：新增 → 修改 → 刪除 → 查單筆 → 查全部 → 查全部（有條件）。
- **刪除方法一律叫 `Remove`，不叫 `Delete`。**
- **查詢命名**：`Find` 開頭查**單筆**（回傳 `{Entity}?`）；`FindAll` 開頭查
  **多筆**（回傳 `IReadOnlyList<{Entity}>`）。
- **Repository 只放存取資料的方法（CRUD＋查詢），不放業務邏輯／狀態轉換**
  （例如「切換完成狀態」）。跟 Entity 自身狀態有關的業務邏輯寫成 Entity 上
  的方法（充血模型），不要寫在 Repository 或 Controller 裡。
- **呼叫端標準流程**：Repository 查出 Entity → 呼叫 Entity 方法改變狀態 →
  Repository 存回去。**不要**在 Repository 開 `Toggle{XX}(Guid {entity}Id)`
  這種直接用 id 操作、把邏輯藏在 Repository 裡的方法。

**範例**

`ITodoRepository`、`MockTodoRepository`。

```csharp
{Entity} Add({Entity} entity);

bool Update({Entity} entity);

bool Remove(Guid {entity}Id);

{Entity}? FindByXX(Guid {entity}Id);

IReadOnlyList<{Entity}> FindAll();

IReadOnlyList<{Entity}> FindAllByXX();
```

充血模型（本專案：切換完成狀態邏輯在 `Todo` 上，不在 Repository）：

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

固定的方法順序跟命名規則，讓打開任何一個 Repository 介面都能立刻找到
「新增在哪裡、查詢在哪裡」；`Find` 跟 `FindAll` 的字首差異讓呼叫端不用看
回傳型別就知道查單筆還是多筆；避免「貧血模型」（Entity 只有屬性沒有行為，
邏輯散落在 Service／Controller／Repository 各處，同一規則被實作兩三次）——
邏輯跟著資料放在一起，不管從 Controller、Console 工具還是測試呼叫，規則都
保證一致，Repository 也維持單純的存取角色。

## 8. Entity 設計規則

**規則**

- **主鍵**：屬性命名 `{Entity}Id`（不要單純的 `Id`），型別 `Guid`，用
  `Guid.CreateVersion7()` 產生（不是隨機無序的 `Guid.NewGuid()`），不用
  `int` 流水號。由 Entity 屬性預設值在建立當下產生、外部提供，Repository
  不再自己產生。
  - 所有代表這個主鍵的變數／參數統一命名 `{entity}Id`，貫穿 Repository、
    Controller action 參數、MVC 路由樣板（`{{entity}Id?}`，不是泛用
    `{id?}`）、View 的 `asp-route-{entity}Id`，不留通用的 `id`。
- **稽核時間戳記**：`CreateTime`（不是 `CreatedAt`）、`UpdateTime`，型別
  `DateTime`，**一律存 UTC**，預設值 `DateTime.UtcNow`（不是 `DateTime.Now`）。
  這條是全專案通則，任何時間戳記都比照辦理；需要顯示當地時間才在畫面層轉換。
  - `Add`：`CreateTime`／`UpdateTime` 都在 Repository 的 `Add` 方法寫入
    當下重新蓋上 `DateTime.UtcNow`，不信任物件建構當下的預設值。
  - `Update`：`CreateTime` 維持既有值不覆寫；`UpdateTime` 由 `Update`
    方法在寫入當下重新蓋上，不信任呼叫端傳進來的值。
  - Entity 上的狀態轉換方法（例如 `ToggleDone()`）不用自己碰
    `UpdateTime`——經過 Repository 的 `Update` 就一定會蓋到。

**範例**

```csharp
public class Todo
{
    public Guid TodoId { get; set; } = Guid.CreateVersion7();

    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;
}
```

**說明**

`{Entity}Id` 比單純的 `Id` 更明確，在只看得到欄位名稱的地方（SQL 查詢結果、
log、跨 Entity 的 join）能立刻知道這個 id 屬於哪個 Entity；GUIDv7 在用戶端
就能產生全域唯一值，前 48 bit 是時間戳，天生照建立時間排序，比 `int` 流水號
（需要共用計數器、多執行個體易衝突）更適合當索引鍵、也更難被外部猜測列舉。
`CreateTime`／`UpdateTime` 統一放 Repository 的 `Add`／`Update` 蓋，不用每個
呼叫端各自記得；`Update` 不動 `CreateTime` 因為建立時間是資料一旦寫入就不該
再變的歷史事實。UTC 是單一、不受時區影響的絕對時間基準，伺服器搬到別的時區
或多台伺服器分佈不同時區也不會讓同一筆資料的時間不一致，這是多數後端系統的
標準做法。
