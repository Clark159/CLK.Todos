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

**namespace 宣告一律用帶大括號的區塊寫法**（不是 C# 較新的 file-scoped
`namespace X;` 寫法）：

```csharp
namespace CLK.Todos
{
    public class Todo
    {
        // ...
    }
}
```

**為什麼這樣做：** namespace 統一等於專案名稱，用一個 `using {專案名稱}`
就能拿到整個專案的型別，不用因為挪動檔案到不同資料夾就要跟著改 namespace、
跟著改呼叫端的 using。大括號寫法把「這個檔案的內容都屬於這個 namespace」的
範圍用視覺化的區塊表示出來，比 file-scoped 寫法更直觀好懂。

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
  Repository（例如 `_todoContext.TodoRepository.GetAll()`），**不直接注入
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

    public ITodoRepository TodoRepository => _todoRepository;
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
- `// Imports`（`using` 陳述式）寫在檔案最上方、`namespace` 區塊外面，
  規則跟其他分類一樣（標頭後不空行、跟後面的 `namespace` 空兩行）。
- **屬性一律用完整的 `get` / `return` 寫法，不要用 `=>` expression-bodied
  寫法**（自動屬性 `{ get; set; }` 不受影響，這條只針對有邏輯、需要寫
  `return` 的計算屬性）。

範例（`TodoContext`）：

```csharp
namespace CLK.Todos
{
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
}
```

**為什麼這樣做：** 打開任何一個類別檔案，成員的排列順序都一樣，不用每次重新
適應這個檔案的組織方式；分類標頭也讓人能快速跳到想看的區塊（例如只想看這個
類別依賴了什麼，直接找 `// Fields`）。

**注意：** 這條規則適用於類別／介面**內部**的成員排序。像 `Program.cs`
這種用 top-level statements（不包在類別裡）寫的進入點檔案不適用。

## 9. 合約檢查（Guard Clause）慣例

**建構子或方法的參數，一律在方法本體最前面做基本合約檢查**，並且用
`#region Contracts` / `#endregion` 包起來，跟後面的邏輯空一行：

- **參照型別（物件）參數**：不能為 `null`，用 `ArgumentNullException.ThrowIfNull(參數)`。
- **字串參數**：不能為 `null` 或空白，用 `ArgumentException.ThrowIfNullOrWhiteSpace(參數)`。
- **值型別參數**（`int`、`bool`、`DateTime` 這類）：本身不可能是 `null`，
  不需要合約檢查。
- **如果某個參數本來就預期可能傳入 `null`**（該參數是可選的），直接給預設值
  `null`（例如 `Todo todo = null`，注意**不要加 `?`**——見第 10 節，這個
  專案的 Nullable 參考型別功能是關閉的），這種參數**不用**加上述的 not-null
  檢查——用預設值本身表達「這裡允許 null」，不用再寫 if 判斷排除 null 情境，
  但呼叫這個參數的邏輯要自己判斷 `is null` 再決定怎麼處理。
- **MVC Controller 的驗證判斷（例如 `ModelState.IsValid`）也算合約檢查的一種**，
  一樣放進 `#region Contracts`，即使它不是丟例外、而是提前 `return`。

`#region Contracts` 的格式：標頭後空一行、檢查內容、內容後空一行、
`#endregion`：

```csharp
#region Contracts

ArgumentNullException.ThrowIfNull(todoRepository);

#endregion
```

範例（`TodoContext` 建構子）：

```csharp
public TodoContext(ITodoRepository todoRepository)
{
    #region Contracts

    ArgumentNullException.ThrowIfNull(todoRepository);

    #endregion

    _todoRepository = todoRepository;
}
```

目前套用 not-null 檢查的地方：`TodoContext` 建構子、`HomeController` /
`TodosController` 建構子、`MockTodoRepository.Add` / `Update`。字串合約檢查
（`ThrowIfNullOrWhiteSpace`）目前程式碼裡還沒有適用的字串參數，先在這裡記下
規則，之後遇到再套用。

**可為 null 的實例，搭配 MVC 驗證判斷**：`TodosController.Create` / `Edit`
（POST）的 `todo` 參數給預設值 `Todo todo = null`——因為表單送出的資料理論上
可能因為某些情況綁定不到值，這裡預期它可能是 `null`，所以不加 not-null
檢查，改成跟 `ModelState.IsValid` 一起放進 `#region Contracts`，不合法就
提前 `return` 導回同一頁：

```csharp
public IActionResult Create([Bind("Title")] Todo todo = null)
{
    #region Contracts

    if (todo is null || !ModelState.IsValid)
    {
        return View(todo);
    }

    #endregion

    _todoContext.TodoRepository.Add(todo);
    return RedirectToAction(nameof(Index));
}
```

**為什麼這樣做：** `#region Contracts` 讓「這個方法對輸入的基本要求是什麼」
從外觀就能一眼認出來、甚至可以摺疊起來，不會跟後面真正的商業邏輯混在一起看。

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
  `Todo GetById(int id)`、`Todo todo = null`，不要寫 `Todo? GetById(...)`）。
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

bool Remove(int id);

Todo GetByXX(int id);

IReadOnlyList<Todo> GetAll();

IReadOnlyList<Todo> GetAllByXX();
```

- 順序固定是：新增 → 修改 → 刪除 → 查單筆 → 查全部 → 查全部（有條件）。
- **刪除方法一律叫 `Remove`，不叫 `Delete`。**
- **查詢方法的命名規則**：
  - 只有 `Get` 開頭（例如 `GetById`）：代表查詢**單筆**，回傳型別是 `Todo`
    （或 `Todo` 為 null，代表查無資料）。
  - `GetAll` 開頭（例如 `GetAll`、`GetAllByCategory`）：代表查詢**多筆**，
    回傳型別是 `IReadOnlyList<Todo>`。
- **Repository 只放存取資料的方法（CRUD＋查詢），不放業務邏輯／狀態轉換的
  方法**（例如「切換完成狀態」這種操作）。這類邏輯屬於 Entity 自己的行為，
  規則見第 12 節「充血模型」，不會出現在 Repository 裡。

目前 `ITodoRepository` 的實際順序：`Add` → `Update` → `Remove` → `GetById`
→ `GetAll`，完全符合上面的固定模式，沒有額外的自訂操作。

**為什麼這樣做：** 固定的方法順序跟命名規則，讓打開任何一個 Repository
介面都能立刻找到「新增在哪裡、查詢在哪裡」，不用每個介面重新適應排列方式；
`Get` 跟 `GetAll` 的字首差異也讓呼叫端不用看回傳型別就知道這個方法查的是
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
`ToggleDone(int id)` 這種直接用 id 操作、把邏輯藏在 Repository 裡的方法：

```csharp
public IActionResult Toggle(int id)
{
    var todo = _todoContext.TodoRepository.GetById(id);
    if (todo is null)
    {
        return NotFound();
    }

    todo.ToggleDone();
    _todoContext.TodoRepository.Update(todo);
    return RedirectToAction(nameof(Index));
}
```

**為什麼這樣做：** 避免變成「貧血模型」（Entity 只有屬性、沒有行為，邏輯
散落在 Service／Controller／Repository 各處，同一個規則可能被不同地方
用不同方式實作兩三次）。邏輯跟著資料放在一起，之後不管從 Controller、
Console 工具、還是測試呼叫 `Todo.ToggleDone()`，規則都保證一致，
Repository 也能維持單純的存取角色，不會越長越肥。
