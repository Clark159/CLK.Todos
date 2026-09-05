# 架構慣例筆記

這份筆記記錄 CLK.Todos 練習過程中逐步確立下來的架構規則，之後會整理成 Skill
給其他團隊共用。內容只反映**目前有效**的規則；規則異動時直接更新對應章節，
不用時間順序流水帳記錄，避免舊規則跟新規則互相矛盾。

## 1. 專案結構與分層

```
src/
├── CLK.Todos.sln
├── CLK.Todos/              Domain 層，不分資料夾，檔案直接放專案根目錄
│   ├── TodoContext.cs      （Domain 入口物件）
│   ├── Todo.cs
│   └── ITodoRepository.cs
├── CLK.Todos.Accesses/     資料存取實作層，不分資料夾
│   └── MockTodoRepository.cs
└── CLK.Todos.WebApp/       表現層（ASP.NET MVC），維持 MVC 慣例資料夾
    ├── Controllers/
    ├── Models/
    └── Views/
```

- **相依方向**：`CLK.Todos.WebApp` → `CLK.Todos.Accesses` → `CLK.Todos`。
  Domain 專案（`CLK.Todos`）在最底層，不被任何專案以外的東西依賴，自己也不依賴任何人。
- **sln 位置**：`CLK.Todos.sln` 放在 `src/` 底下，跟所有專案同一層，
  不放在 repo 根目錄。repo 根目錄只留 `README.md`、`.gitignore`、`LICENSE`、`docs/`
  這類跟 .NET 建置無關的東西。
- **資料夾要不要拆，依專案性質決定**：
  - `CLK.Todos`（Domain）、`CLK.Todos.Accesses`（資料存取實作）**不拆資料夾**，
    所有檔案都放在專案根目錄，靠 namespace 統一（見第 2 節）就足夠辨識。
  - `CLK.Todos.WebApp`（表現層）**維持 MVC 慣例資料夾**（`Controllers/`、
    `Models/`、`Views/`），因為這是 ASP.NET MVC 框架本身預期的結構，
    Razor View 的慣例路由（`Views/{Controller}/{Action}.cshtml`）也依賴這個結構。
  - 不論拆不拆資料夾，資料夾都只是**物理上**分類檔案方便找，不影響
    namespace——namespace 規則見第 2 節。

**為什麼這樣做：** repo 根目錄保持乾淨、之後加 `tests/` 也對稱；相依方向單向、
沒有循環參照，任何一層要抽換都不會牽動它下面的層。

## 2. Namespace 慣例

**每個專案內所有檔案，namespace 一律等於專案名稱本身，不因為放在哪個資料夾
就多加後綴。** 例如：

- `CLK.Todos` 專案裡的檔案，namespace 一律是 `CLK.Todos`。
- `CLK.Todos.Accesses` 專案裡的檔案，namespace 一律是 `CLK.Todos.Accesses`。
- `CLK.Todos.WebApp` 專案裡，不管檔案是不是放在 `Controllers/`、`Models/` 資料夾，
  namespace 一律是 `CLK.Todos.WebApp`（不是 `CLK.Todos.WebApp.Controllers`、
  `CLK.Todos.WebApp.Models`）。

**namespace 宣告一律用 file-scoped 寫法**（`namespace X;`，不要用帶大括號的
區塊寫法）；**`using` 陳述式一樣放在檔案最上面、比 `namespace` 前面，中間
空一行**（這是 C# 官方範本、`dotnet new`、Visual Studio 預設的順序，不要
反過來）：

```csharp
using System.ComponentModel.DataAnnotations;

namespace CLK.Todos;

public class Todo
{
    // ...
}
```

沒有 `using` 陳述式的檔案，直接從 `namespace` 開始：

```csharp
namespace CLK.Todos;

public interface ITodoRepository
{
    // ...
}
```

**為什麼這樣做：** namespace 統一等於專案名稱，用一個 `using {專案名稱}`
就能拿到整個專案的型別，不用因為挪動檔案到不同資料夾就要跟著改 namespace、
跟著改呼叫端的 using。file-scoped 寫法比大括號區塊少一層縮排，檔案裡的
程式碼不用整段往右推一格；`using`／`namespace` 的順序跟間距維持 C# 官方
慣例，不特立獨行，這樣 IDE 自動排序 using、`dotnet format` 這類工具的
預設行為才不會跟專案慣例打架。

## 3. Domain 專案（`CLK.Todos`）

- 純類別庫（`dotnet new classlib`），**只依賴 .NET BCL**，不參照 ASP.NET Core
  或任何基礎設施套件（`System.ComponentModel.DataAnnotations` 屬於 BCL，可以用）。
- **不拆資料夾**，所有檔案（Entity、Repository 介面、Context）都直接放在
  專案根目錄，靠 namespace 統一（見第 2 節）就足夠辨識，不需要再用資料夾分類。
- **Entity**：例如 `Todo.cs`。
- **Repository 介面**：只放介面，**不放實作**——實作屬於基礎設施細節，見第 4 節。
  - 命名一律 `I{Entity}Repository`（例如 `ITodoRepository`）。
  - 方法命名與排序規則見第 11 節，不要洩漏儲存細節（例如不要叫 `SelectFromDb`）。
- **`{Solution}Context`（例如 `TodoContext`）是 Domain 的入口物件**。
  - 所有 Repository 介面都透過建構子注入到 Context 裡，並用唯讀屬性對外提供
    （屬性命名規則見第 7 節，寫法用 `get` / `return`，不用 `=>`，見第 8 節）。
  - 外部一律注入 `TodoContext` 來使用 Repository，**不直接注入個別 Repository
    介面**（例如 Controller 不會直接拿 `ITodoRepository`，而是拿
    `TodoContext` 再存取 `todoContext.TodoRepository`）。

**為什麼這樣做：** Domain 專案不依賴任何框架，之後才能被其他專案型態（Console、
Worker、不同的 Web 框架）重複使用而不用背 ASP.NET Core 的相依性；「規格」（介面）
跟「實作」分開，換實作時 Domain 完全不用改。用單一入口物件（Context）集中
所有 Repository，呼叫端只要注入一個東西，不用每加一個 Entity 就多一個要注入
的介面，行為上也貼近未來若導入 EF Core 的 `DbContext` 用法，轉換成本較低。

## 4. 資料存取實作層（`CLK.Todos.Accesses`）

- 獨立類別庫，參照 Domain 專案（`CLK.Todos`），**專門存放 Repository 介面的實作**。
- 不拆資料夾，實作類別直接放在專案根目錄（理由同 Domain 專案，見第 1 節）。
- 上層專案（`CLK.Todos.WebApp`）參照 `CLK.Todos.Accesses`，自己不放任何實作。

**為什麼這樣做：** 之後若要同時支援多種儲存方式（例如 Mock + 真正的資料庫），
在這個專案（或視情況再拆多個）新增實作類別即可，Web 專案跟 Domain 完全不用
感知底下換了什麼儲存機制。

## 5. 命名慣例

### Repository 介面

一律 `I{Entity}Repository`，例如 `ITodoRepository`。

### 非持久化實作

先求能動、資料不會真的存下來的實作，一律 `Mock{Entity}{介面名}`，
例如 `MockTodoRepository`。

### 真實資料庫的實作（尚未使用，先預留規則）

之後接真實資料庫時，一律 `{技術}{Entity}{介面名}`，例如 `EfTodoRepository`。

**為什麼這樣做：** 一看類別名稱就知道它是不是「真的會持久化」，避免 Mock 跟真正
接資料庫的實作混用同一套命名造成誤會。

### 應用程式進入點專案（可執行的專案）

有實際進入點（`Main` / `Program.cs`）、會被啟動執行的專案，一律
`{Solution}.{類型}App`，依專案性質決定 `{類型}`：

- ASP.NET MVC 網站：`{Solution}.WebApp`，例如 `CLK.Todos.WebApp`。
- Console 程式（尚未使用，先預留規則）：`{Solution}.ConsoleApp`。

**為什麼這樣做：** 讓「這個專案是不是可以直接執行的應用程式」從專案名稱就能
判斷出來，跟 Domain／Accesses 這類被參照、不能單獨執行的類別庫明確區分；
之後如果同一個 solution 要同時有網站跟 Console 工具（例如批次匯入資料），
兩者命名方式一致，不用另外想規則。

## 6. DI 註冊慣例

- 在 `CLK.Todos.WebApp/Program.cs` 註冊：
  - 每個 Repository 介面對實作：`AddSingleton<ITodoRepository, MockTodoRepository>()`。
  - Domain 的入口物件本身也要註冊：`AddSingleton<TodoContext>()`
    （建構子依賴的 Repository 介面會由 DI 容器自動解析注入）。
- Controller（或其他呼叫端）一律只依賴 `TodoContext`，透過它的屬性存取
  Repository（例如 `_todoContext.TodoRepository.FindAll()`），**不直接注入
  Repository 介面**。

## 7. 建構子注入命名慣例

適用於**所有**類別，不限於某一層：

1. **有從建構子注入物件的類別，一律先用私有欄位承接**，不要繞過私有欄位直接
   把注入的物件賦值給屬性或在方法間傳來傳去。
2. **建構子／方法參數命名**：注入物件的型別名稱，**去掉介面代表字首 `I`**（如果
   是介面的話），再把第一個字母轉小寫。
   - `TodoContext` → 參數 `todoContext`
   - `ITodoRepository` → 去掉 `I` → `TodoRepository` → 參數 `todoRepository`
3. **私有欄位命名**：`_` + 第 2 點的命名結果。
   - `_todoContext`、`_todoRepository`
4. **公開屬性命名**：注入物件的型別名稱，**去掉介面代表字首 `I`**，但**不轉小寫**
   （維持 C# 屬性慣例的 PascalCase）。
   - `ITodoRepository` → 去掉 `I` → 屬性名 `TodoRepository`

範例（`TodoContext`）：

```csharp
public class TodoContext
{
    private readonly ITodoRepository _todoRepository;

    public TodoContext(ITodoRepository todoRepository)
    {
        _todoRepository = todoRepository;
    }

    public ITodoRepository TodoRepository
    {
        get { return _todoRepository; }
    }
}
```

**為什麼這樣做：** 參數名、欄位名、屬性名都能從「注入的型別是什麼」直接反推出來，
不用每個類別自己想一套命名，看程式碼的人也能立刻知道某個欄位裝的是什麼型別，
不用跳去看宣告。

## 8. 類別成員排序慣例

類別（含介面）內的成員，一律照以下順序分類，**只列出實際有內容的分類**（沒有
建構子就不寫 `// Constructors`）：

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

- 每個分類前面加上對應的註解標頭（例如 `// Fields`），**標頭下面第一個成員
  緊接著寫，不空行**。
- 同一分類內，第一個之後的每個成員之間空一行。
- 不同分類之間空兩行。
- **`Imports`（`using` 陳述式）是例外，不算進上面的分類清單**：不加
  `// Imports` 這種標頭註解；`using` 陳述式的位置跟間距見第 2 節
  （`using` 在最上面、跟下面的 `namespace` 之間空一行）。
- **屬性一律用完整的 `get` / `return` 寫法，不要用 `=>` expression-bodied
  寫法**（自動屬性 `{ get; set; }` 不受影響，這條只針對有邏輯、需要寫
  `return` 的計算屬性）。
- **`// Fields` 分類內部，同步用的 lock 物件（例如 `private readonly
  object _lock = new();`）排最前面**，其餘欄位接在後面（例如
  `MockTodoRepository` 的 `_lock` 排在 `_todos` 前面）——`_lock` 是用來
  保護後面欄位的存取，排最前面代表「這個類別底下的欄位需要同步保護」，
  一眼就能看出這個類別有執行緒安全的考量。

範例（`TodoContext`）：

```csharp
namespace CLK.Todos;

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

**為什麼這樣做：** 打開任何一個類別檔案，成員的排列順序都一樣，不用每次重新
適應這個檔案的組織方式；分類標頭也讓人能快速跳到想看的區塊（例如只想看這個
類別依賴了什麼，直接找 `// Fields`）。

**注意：** 這條規則適用於類別／介面**內部**的成員排序。像 `Program.cs`
這種用 top-level statements（不包在類別裡）寫的進入點檔案不適用。

## 9. 合約檢查（Guard Clause）慣例

**建構子或方法的參數，一律在方法本體最前面做基本合約檢查**，並且用
`// Contracts` 標籤（跟第 15 節「方法內部的小區塊標籤」同一套排版），
跟後面的邏輯空一行：

- **參照型別（物件）參數**：不能為 `null`，用 `ArgumentNullException.ThrowIfNull(參數)`。
- **字串參數**：不能為 `null` 或空白，用 `ArgumentException.ThrowIfNullOrWhiteSpace(參數)`。
- **值型別參數**（`int`、`bool`、`DateTime` 這類）：本身不可能是 `null`，
  不需要合約檢查。
- **MVC Controller 的驗證判斷（例如 `ModelState.IsValid`、路由 id 跟表單 id
  是否一致）也算合約檢查的一種**，一樣放進 `// Contracts` 標籤底下，即使它不是
  丟例外、而是提前 `return`。
- **每一個檢查都獨立成一行、一個判斷式，不要用 `||` / `&&` 把多個檢查
  合併成一個條件**——即使結果都是「不合法就 `return`」，也要拆開寫，
  一眼就能看出這裡總共驗證了幾件事、各自驗證什麼。
- 像 `if (條件) return ...;` 這種單一陳述式的 guard clause，**一律省略
  大括號、寫成一行**，不要展開成四行的 `if { }` 區塊——**這條不只限於
  `// Contracts` 底下的參數檢查，方法本體任何地方「不合法／找不到就
  提前 return」的單一陳述式判斷都比照辦理**（例如 `// Search` 步驟裡
  查無資料就回傳的判斷）。

`// Contracts` 的格式：**標籤緊接著第一個檢查、不空行**（跟第 15 節的
標籤規則一致）；如果檢查內容不只一行，**內容彼此之間也不空行**（緊貼在
一起）；檢查結束後，跟後面的邏輯空一行：

```csharp
// Contracts
ArgumentNullException.ThrowIfNull(todoRepository);
```

範例（`TodoContext` 建構子）：

```csharp
public TodoContext(ITodoRepository todoRepository)
{
    // Contracts
    ArgumentNullException.ThrowIfNull(todoRepository);

    _todoRepository = todoRepository;
}
```

目前套用 not-null 檢查的地方：`TodoContext` 建構子、`HomeController` /
`TodosController` 建構子、`MockTodoRepository.Add` / `Update`、
`TodosController.Create` / `Edit`（POST，`todo` 參數）。字串合約檢查
（`ThrowIfNullOrWhiteSpace`）目前程式碼裡還沒有適用的字串參數，先在這裡記下
規則，之後遇到再套用。

**not-null 檢查 + MVC 驗證判斷放在同一個 `// Contracts` 標籤底下，但各自獨立
一行**：`TodosController.Create` / `Edit`（POST）的 `todo` 參數給預設值
`Todo todo = null`（讓 MVC action 簽章上看得出這個參數可選），但一進方法
本體還是先用 `ArgumentNullException.ThrowIfNull(todo)` 擋掉 null（正常
情況下 model binder 一定會建立實例，這裡是防禦性寫法，不是預期會發生），
接著才是跟業務規則有關的驗證，各自獨立一行、不合法就提前 `return`：

```csharp
public IActionResult Create([Bind("Title")] Todo todo = null)
{
    // Contracts
    ArgumentNullException.ThrowIfNull(todo);
    if (!ModelState.IsValid) return View(todo);

    // Execute
    _todoContext.TodoRepository.Add(todo);

    // Result
    return RedirectToAction(nameof(Index));
}
```

`Edit`（POST）多一個「路由 id 跟表單 id 是否一致」的檢查，一樣獨立一行：

```csharp
public IActionResult Edit(Guid todoId, [Bind("TodoId,Title,IsDone")] Todo todo = null)
{
    // Contracts
    ArgumentNullException.ThrowIfNull(todo);
    if (todoId != todo.TodoId) return View(todo);
    if (!ModelState.IsValid) return View(todo);

    // Execute
    _todoContext.TodoRepository.Update(todo);

    // Result
    return RedirectToAction(nameof(Index));
}
```

**為什麼這樣做：** `// Contracts` 標籤讓「這個方法對輸入的基本要求是什麼」
從外觀就能一眼認出來，不會跟後面真正的商業邏輯混在一起看；跟第 15 節的
其他步驟標籤是同一套視覺語言，不用另外學一種排版，也不必靠 `#region`
摺疊/展開才能看清楚檢查內容。

## 10. 不使用 Nullable 參考型別（Nullable Reference Types）

所有專案的 `.csproj` 一律設定 `<Nullable>disable</Nullable>`（不是預設範本的
`enable`）：

```xml
<PropertyGroup>
  <TargetFramework>net9.0</TargetFramework>
  <Nullable>disable</Nullable>
  ...
</PropertyGroup>
```

- 參照型別（`string`、`Todo` 這類 class）**不要加 `?`**（例如寫
  `Todo FindById(Guid todoId)`、`Todo todo = null`，不要寫 `Todo? FindById(...)`）。
  Nullable 功能關掉之後，這種標註不會被編譯器檢查，加了反而會產生
  `CS8632` 警告（「可為 Null 的參考型別註釋應只用於 nullable 註釋內容中」）。
- 值型別要表示「可能沒有值」，還是可以用 `?`（例如 `int?`），這是 C# 原生的
  `Nullable<T>`，跟參考型別的 Nullable 註釋是兩回事，不受這條規則影響。
- 「一個參照型別的值到底會不會是 null」改用第 9 節的合約檢查慣例
  （`ArgumentNullException.ThrowIfNull`／可為 null 的參數給預設值 `null`
  並自己判斷 `is null`）在**執行期**把關，不依賴編譯器的靜態 Nullable 分析。

**為什麼這樣做：** 統一用 guard clause／執行期判斷來表達「這裡可不可以是
null」，不同時疊加編譯器的 Nullable 靜態分析，避免兩套機制互相打架（例如
關掉 Nullable 後，舊的 `?` 標註反而變成警告來源）。

## 11. Repository 方法命名與排序慣例

Repository 介面（跟它的實作）裡的方法，**一律照以下順序排列**：

```csharp
Todo Add(Todo todo);

bool Update(Todo todo);

bool Remove(Guid todoId);

Todo FindByXX(Guid todoId);

IReadOnlyList<Todo> FindAll();

IReadOnlyList<Todo> FindAllByXX();
```

- 順序固定是：新增 → 修改 → 刪除 → 查單筆 → 查全部 → 查全部（有條件）。
- **刪除方法一律叫 `Remove`，不叫 `Delete`。**
- **查詢方法的命名規則**：
  - 只有 `Find` 開頭（例如 `FindById`）：代表查詢**單筆**，回傳型別是 `Todo`
    （或 `Todo` 為 null，代表查無資料）。
  - `FindAll` 開頭（例如 `FindAll`、`FindAllByCategory`）：代表查詢**多筆**，
    回傳型別是 `IReadOnlyList<Todo>`。
- **Repository 只放存取資料的方法（CRUD＋查詢），不放業務邏輯／狀態轉換的
  方法**（例如「切換完成狀態」這種操作）。這類邏輯屬於 Entity 自己的行為，
  規則見第 12 節「充血模型」，不會出現在 Repository 裡。

目前 `ITodoRepository` 的實際順序：`Add` → `Update` → `Remove` → `FindById`
→ `FindAll`，完全符合上面的固定模式，沒有額外的自訂操作。

**為什麼這樣做：** 固定的方法順序跟命名規則，讓打開任何一個 Repository
介面都能立刻找到「新增在哪裡、查詢在哪裡」，不用每個介面重新適應排列方式；
`Find` 跟 `FindAll` 的字首差異也讓呼叫端不用看回傳型別就知道這個方法查的是
單筆還是多筆。

## 12. 充血模型（Rich Domain Model）慣例

**跟 Entity 自身狀態有關的業務邏輯，寫成 Entity 上的方法，不要寫在
Repository 或 Controller 裡。** Repository 只負責存取資料（見第 11 節），
不承載任何業務規則；「資料要怎麼變化」的邏輯屬於資料自己。

範例：切換完成狀態的邏輯在 `Todo` 類別上：

```csharp
public class Todo
{
    // Properties
    public bool IsDone { get; set; }


    // Methods
    public void ToggleDone()
    {
        IsDone = !IsDone;
    }
}
```

**呼叫端的標準流程**：先用 Repository 查出 Entity，呼叫 Entity 上的方法
改變狀態，再用 Repository 存回去——**不要**在 Repository 上開一個
`ToggleDone(Guid todoId)` 這種直接用 id 操作、把邏輯藏在 Repository 裡的方法：

```csharp
public IActionResult Toggle(Guid todoId)
{
    var todo = _todoContext.TodoRepository.FindById(todoId);
    if (todo is null) return NotFound();

    todo.ToggleDone();
    _todoContext.TodoRepository.Update(todo);

    // Result
    return RedirectToAction(nameof(Index));
}
```

**為什麼這樣做：** 避免變成「貧血模型」（Entity 只有屬性、沒有行為，邏輯
散落在 Service／Controller／Repository 各處，同一個規則可能被不同地方
用不同方式實作兩三次）。邏輯跟著資料放在一起，之後不管從 Controller、
Console 工具、還是測試呼叫 `Todo.ToggleDone()`，規則都保證一致，
Repository 也能維持單純的存取角色，不會越長越肥。

## 13. `// Default` 與 `// Result` 註解慣例

### 建構子：`// Default`

建構子裡「把參數存進欄位」的賦值陳述式，前面加上 `// Default` 註解
（跟前面的 `// Contracts` 之間空一行，`// Default` 本身跟賦值陳述式
之間不空行）：

```csharp
public TodoContext(ITodoRepository todoRepository)
{
    // Contracts
    ArgumentNullException.ThrowIfNull(todoRepository);

    // Default
    _todoRepository = todoRepository;
}
```

### 方法：`// Result`

方法裡代表「正常結果」的最後一個 `return`，前面加上 `// Result` 註解；
如果前面有其他邏輯，空一行再接 `// Result`，沒有的話（`// Result` 是
方法本體第一行）就不用空行。

**例外：屬性的 `get` 如果整個只有一行（就只有那個 `return`），不加
`// Result`，而且整個 `get` 縮成一行寫**，比展開成三行好讀：

```csharp
public ITodoRepository TodoRepository
{
    get { return _todoRepository; }
}
```

**提前結束的 guard clause 式 `return`（例如 `if (todo is null) return NotFound();`
這種）不用加 `// Result`**——不管它是不是在 `// Contracts` 標籤底下，
這種 return 已經由前面的 `if` 條件自我解釋，只有「這個方法真正要交付的
結果」才需要標註：

```csharp
public IActionResult Edit(Guid todoId)
{
    var todo = _todoContext.TodoRepository.FindById(todoId);
    if (todo is null) return NotFound();

    // Result
    return View(todo);
}
```

一個方法如果有多個「正常結果」的 return（例如 `MockTodoRepository.Update`
先判斷「找不到就回傳 false」是提前結束、不標，最後「更新成功回傳 true」
才是正常結果、要標）：

```csharp
public bool Update(Todo todo)
{
    // Contracts
    ArgumentNullException.ThrowIfNull(todo);

    lock (_lock)
    {
        var existing = _todos.FirstOrDefault(t => t.TodoId == todo.TodoId);
        if (existing is null) return false;

        existing.Title = todo.Title;
        existing.IsDone = todo.IsDone;

        // Result
        return true;
    }
}
```

**為什麼這樣做：** `// Default` 讓建構子一眼就能分出「合約檢查」跟
「真正做的事（存欄位）」兩個階段；`// Result` 讓方法裡「這是提前擋掉的
特殊情況」跟「這是正常要交付的結果」清楚分開，不用逐行讀邏輯才能找到
真正的輸出在哪裡。

## 14. 呼叫自己的方法或屬性不加 `this.`

**在類別內部呼叫自己（包含繼承來的）方法或屬性，一律不加 `this.` 前綴**：

```csharp
// Todo 自己的 IsDone 屬性
public void ToggleDone()
{
    IsDone = !IsDone;
}
```

- **欄位本來就不加 `this.`**——欄位已經用 `_` 前綴跟參數/區域變數區分開來
  （見第 7 節），加了反而累贅。
- **`nameof(...)` 裡面本來就不加 `this.`**（例如 `nameof(Index)`，這是
  編譯期取名稱字串，不是真的呼叫）。
- **Controller 裡繼承自基底類別 `Controller` 的成員也不加**（例如
  `ModelState`、`HttpContext`、`View(...)`、`RedirectToAction(...)`、
  `NotFound()`）——不管是不是 `return` 直接呼叫，一律省略：

```csharp
// TodosController
public IActionResult Create([Bind("Title")] Todo todo = null)
{
    // Contracts
    ArgumentNullException.ThrowIfNull(todo);
    if (!ModelState.IsValid) return View(todo);

    _todoContext.TodoRepository.Add(todo);

    // Result
    return RedirectToAction(nameof(Index));
}
```

目前套用的地方：`TodosController` / `HomeController` 裡的 `ModelState`、
`HttpContext`；`Todo.ToggleDone()` 裡的 `IsDone`；`ErrorViewModel.ShowRequestId`
裡的 `RequestId`。

**為什麼這樣做：** `this.` 在這個專案裡沒有實質區分作用——欄位已經靠 `_`
前綴、方法參數靠命名跟類別成員區分開來，不會混淆到需要 `this.` 才能認出
「這是自己的成員」；省略之後每一行都少一截視覺雜訊，看程式碼的人一樣能
從命名（`_` 前綴 vs. 沒有前綴）判斷出處，不需要額外的前綴當提示。

## 15. 方法內部的小區塊標籤

**方法本體避免寫太深的巢狀，盡量拆成一個個平鋪的區塊**，每個區塊前面
加一個單字的小註解講清楚這個區塊在做什麼——**註解只是提示這個區塊的
角色，不要把整段邏輯寫進註解裡**。跟 `// Contracts`、`// Default` 同一套
排版：第一個標籤前面不空行，後面每個標籤前面空一行。

### 標籤詞彙表

小標籤**優先從下面這份清單挑字**，不夠用才自己另外想詞：

| 標籤 | 用途 | 目前是否已有實例 |
|---|---|---|
| `// Contracts` | 方法／建構子最前面的參數合約檢查（見第 9 節） | 有 |
| `// Variables` | 宣告本地變數（還沒賦值、純宣告用途的區塊） | 尚無，先保留 |
| `// Initialize` | 初始化一個物件或集合的起始狀態 | 尚無，先保留 |
| `// Default` | 建構子裡「把參數存進欄位」的預設賦值（見第 13 節） | 有 |
| `// Define` | 定義／組出一個新的物件或值 | 尚無，先保留 |
| `// Result` | 方法裡代表「正常結果」的最後一個 `return`（見第 13 節） | 有 |
| `// Require` | 方法本體中途（不是開頭參數）冒出的額外前置要求 | 尚無，先保留 |
| `// Arguments` | 呼叫下一個方法前，組裝要傳入的引數 | 尚無，先保留 |
| `// Notify` | 發出通知（例如記錄 log、觸發事件通知） | 尚無，先保留 |
| `// Search` | 查詢資料（例如呼叫 Repository 的 `FindById`／`FindAll`） | 有 |
| `// Raise` | 拋出例外，或引發（raise）一個事件 | 尚無，先保留 |
| `// Execute` | 執行核心動作（例如呼叫 Repository 的 `Add`／`Update`／`Remove`，或呼叫 Entity 的狀態轉換方法） | 有 |
| `// Lock` | 進入 `lock` 區塊做執行緒同步 | 有 |

「尚無，先保留」的標籤先在這裡記下定義，之後程式碼裡出現對應場景再套用，
不用現在硬找地方套。

範例（`MockTodoRepository.Update`，查詢用 `// Search`、寫入用 `// Execute`）：

```csharp
public bool Update(Todo todo)
{
    // Contracts
    ArgumentNullException.ThrowIfNull(todo);

    // Lock
    lock (_lock)
    {
        // Search
        var existing = _todos.FirstOrDefault(t => t.TodoId == todo.TodoId);
        if (existing is null) return false;

        // Execute
        existing.Title = todo.Title;
        existing.IsDone = todo.IsDone;

        // Result
        return true;
    }
}
```

範例（`TodosController.Toggle`，步驟橫跨查詢、切換、儲存三個階段，
切換狀態＋存回去算同一個「執行」區塊）：

```csharp
public IActionResult Toggle(Guid todoId)
{
    // Search
    var todo = _todoContext.TodoRepository.FindById(todoId);
    if (todo is null) return NotFound();

    // Execute
    todo.ToggleDone();
    _todoContext.TodoRepository.Update(todo);

    // Result
    return RedirectToAction(nameof(Index));
}
```

只有一個步驟的方法（例如 `FindById`、`FindAll` 這種本體就是單一個 `return`）
不需要額外標籤，`// Result` 本身就足夠說明。

**為什麼這樣做：** 方法一長，光看程式碼要花時間才能分辨「這幾行在做
同一件事、還是已經換到下一步了」；一個單字標籤讓步驟邊界一眼可辨，
掃過標籤就能知道整個方法的流程，不用逐行讀。統一從固定詞彙表挑字，
也讓不同方法、不同專案的標籤用語一致，不會同一種操作在這裡叫
`FindById`、在那裡叫 `Query`、`Lookup` 各自表述。巢狀太深的方法（多層
`if`／`else`／迴圈疊在一起）光看縮排就很難分辨區塊邊界，盡量用提前
`return`（見第 9 節）把邏輯攤平成一層層平鋪的區塊，標籤才有意義。

## 16. Entity 主鍵：命名 `{Entity}Id`、優先使用 GUIDv7

**Entity 的主鍵屬性一律命名 `{Entity}Id`，不要用單純的 `Id`**（例如
`Todo` 的主鍵是 `TodoId`，不是 `Id`）；型別優先用 `Guid`，並且用
`Guid.CreateVersion7()` 產生（不是 `Guid.NewGuid()` 這種隨機、無序的
v4），不要用 `int` 加流水號計數器：

```csharp
public class Todo
{
    // Properties
    public Guid TodoId { get; set; }
}
```

- **所有代表這個 Entity 主鍵的 `id` 變數／參數，一律照同一套規則命名
  成 `{entity}Id`**（例如 `todoId`），不管出現在 Repository 介面／實作、
  Controller action 參數、MVC 路由樣板、View 的 `asp-route-*`，全部
  一致，不留通用的 `id` 這個名字：
  - Repository：`FindById(Guid todoId)`、`Remove(Guid todoId)`。
  - Controller action 參數：`Edit(Guid todoId)`、`Delete(Guid todoId)`、
    `DeleteConfirmed(Guid todoId)`、`Toggle(Guid todoId)`。
  - MVC 路由樣板：`Program.cs` 的預設路由 pattern 用
    `{controller=Todos}/{action=Index}/{todoId?}`，不是泛用的 `{id?}`。
  - View：`asp-route-todoId="@todo.TodoId"`（不是 `asp-route-id`）。
- **產生時機**：由 Repository 的 `Add` 方法在寫入當下呼叫
  `Guid.CreateVersion7()` 賦值給 `todo.TodoId`（見第 15 節
  `MockTodoRepository.Add` 的 `// Execute` 區塊），呼叫端不用自己組 id。
- **不需要**額外的流水號欄位（例如 `_nextId`）——GUIDv7 本身已經具備
  時間排序性，拿掉計數器後 Repository 也少一個要處理併發遞增的欄位。
- MVC Controller、View 不用額外處理型別轉換：ASP.NET Core 的路由與
  表單繫結原生支援 `Guid`；`[Bind(...)]` 清單裡的欄位名記得跟著改成
  `TodoId`。

目前套用的地方：`Todo.TodoId`、`ITodoRepository.FindById`／`Remove`
（參數 `todoId`）、`MockTodoRepository` 全部方法、`TodosController` 的
`Edit`／`Delete`／`DeleteConfirmed`／`Toggle`（action 參數 `todoId`）、
`Program.cs` 預設路由的 `{todoId?}`、Views 裡的
`asp-route-todoId="@todo.TodoId"`／`asp-for="TodoId"`。

**為什麼這樣做：** `{Entity}Id` 比單純的 `Id` 更明確——在只看得到欄位
名稱的地方（例如 SQL 查詢結果、log、跨 Entity 的 join）能立刻知道這個
id 屬於哪個 Entity，不用回頭看是哪張表／哪個型別；`int` 流水號需要一個
共用計數器才能保證不重複，多執行個體（例如之後真的接資料庫、或多台
伺服器）很容易衝突；GUIDv7 在用戶端就能產生全域唯一值，不用問資料庫要
下一個號碼是多少，同時前 48 bit 是時間戳，天生照建立時間排序，比純隨機
的 v4 更適合當資料庫索引鍵、也比 `int` 更難被外部猜測、列舉。

## 17. Entity 稽核時間戳記：`CreateTime` 與 `UpdateTime`

**Entity 記錄建立時間的屬性一律叫 `CreateTime`（不是 `CreatedAt`），
更新時間叫 `UpdateTime`**，兩者都是 `DateTime`，**一律存 UTC**，預設值
都給 `DateTime.UtcNow`（不是 `DateTime.Now`）：

```csharp
public class Todo
{
    // Properties
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;
}
```

- **`CreateTime`**：物件建立當下的預設值就是最終值，之後不會再變動，
  不需要 Repository 額外賦值。
- **`UpdateTime`**：預設值跟 `CreateTime` 一樣（代表「還沒被更新過，
  最後異動時間就是建立時間」），**由 Repository 的 `Update` 方法在
  寫入當下重新蓋上 `DateTime.UtcNow`**（見 `MockTodoRepository.Update` 的
  `// Execute` 區塊，跟 `Title`／`IsDone` 這些欄位一起複製，只是這欄
  不信任呼叫端傳進來的值，一律用當下時間覆蓋）。
- Entity 上的狀態轉換方法（例如 `Todo.ToggleDone()`）**不用自己碰
  `UpdateTime`**——只要有經過 Repository 的 `Update` 就一定會蓋到最新
  時間，不用每個會改變狀態的方法各自處理一次。
- **這條不只限於 `CreateTime`／`UpdateTime`，是全專案通則：任何 Entity
  上儲存的時間戳記一律用 `DateTime.UtcNow` 產生、以 UTC 存放**；需要
  顯示給使用者看的地方（例如 View）才在畫面層轉成當地時區，不要在
  Domain／資料儲存層存當地時間。

目前套用的地方：`Todo.CreateTime`／`UpdateTime`、
`MockTodoRepository.Add`（`CreateTime` 用屬性預設值，不用額外賦值）／
`Update`（蓋 `UpdateTime`）／`FindAll`（排序用 `CreateTime`）。目前
`Index.cshtml`／`Delete.cshtml` 直接顯示 `CreateTime`／`UpdateTime`
原始值（UTC），還沒轉當地時區——之後有需要再補時區轉換。

**為什麼這樣做：** `CreateTime`／`UpdateTime` 一組對稱命名，比
`CreatedAt` 這種只顧建立、沒有對應更新時間的命名更完整；蓋
`UpdateTime` 的責任統一放在 Repository 的 `Update` 方法（跟第 11 節
「Repository 只放存取資料的方法」一致——這是持久化當下的稽核動作，
不是 Entity 自身的業務邏輯），不用每個會呼叫 `Update` 的地方（Controller、
Entity 方法）各自記得要蓋時間戳記，只要走過 `Update` 就保證正確。存
UTC 是因為 `DateTime.Now` 綁死伺服器當地時區，一旦伺服器搬到別的時區、
或多台伺服器分佈在不同時區，同一筆資料在不同機器上取到的「當地時間」
會不一致；UTC 是單一、不受時區影響的絕對時間基準，排序、比較、跨系統
交換資料都不會出錯，這是多數後端系統的標準做法。
