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
  - 方法用領域語言命名（`GetAll` / `GetById` / `Add` / `Update` / `Delete`），
    不要洩漏儲存細節（例如不要叫 `SelectFromDb`）。
- **`{Solution}Context`（例如 `TodoContext`）是 Domain 的入口物件**。
  - 所有 Repository 介面都透過建構子注入到 Context 裡，並用同名（複數）的
    唯讀屬性對外提供，例如 `public ITodoRepository Todos { get; }`。
  - 外部一律注入 `TodoContext` 來使用 Repository，**不直接注入個別 Repository
    介面**（例如 Controller 不會直接拿 `ITodoRepository`，而是拿
    `TodoContext` 再存取 `context.Todos`）。

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
  Repository（例如 `_context.Todos.GetAll()`），**不直接注入 Repository 介面**。
