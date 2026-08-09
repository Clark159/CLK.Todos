# 架構慣例筆記

這份筆記記錄我們在 CLK.Todos 練習過程中逐步確立下來的架構規則，
之後會整理成 Skill 給其他團隊共用。每條規則盡量記下「規則本身」跟
「為什麼這樣訂」，方便之後轉寫成 Skill 時保留脈絡。

## 目前規則

### 1. 獨立的 Domain 專案，相依要乾淨

- 新增 `src/CLK.Todos`（跟 solution 同名、無後綴），是一個純類別庫（`dotnet new classlib`），
  只依賴 .NET BCL，不參照 ASP.NET Core 或任何基礎設施套件。
- **Entity** 放在 `Entities/` 資料夾，namespace 為 `CLK.Todos.Entities`。
  例如 `Entities/Todo.cs`。
- **Repository 介面**放在 `Repositories/` 資料夾，namespace 為 `CLK.Todos.Repositories`，
  命名一律 `I{Entity}Repository`（例如 `ITodoRepository`）。
  介面方法用領域語言命名（`GetAll` / `GetById` / `Add` / `Update` / `Delete`），
  不要洩漏儲存細節（例如不要叫 `SelectFromDb`）。
- **Repository 的實作**（例如 `InMemoryTodoRepository`）不放在 Domain 專案，
  維持在呼叫端專案（目前是 `CLK.Todos.Web/Services/`），之後若拆出 Infrastructure 專案再搬過去。
  這樣 Domain 專案才能保持「相依乾淨」——它只定義規格，不管規格怎麼被實現。
- 上層專案（例如 `CLK.Todos.Web`）用 `dotnet add reference` 參照 Domain 專案，
  並在 DI 註冊時對應：`AddSingleton<ITodoRepository, InMemoryTodoRepository>()`。

**為什麼這樣做：** Domain 專案不依賴任何框架，之後才能被其他專案型態（Console、Worker、
不同的 Web 框架）重複使用而不用跟著背 ASP.NET Core 的相依性；同時把「規格」跟「實作」
分開，未來要換資料庫實作時，Domain 跟 Controller 都不用改，只要新增一個實作類別、
改 DI 註冊即可。

### 2. sln 放在 `src/` 資料夾裡面

- `CLK.Todos.sln` 不放在 repo 根目錄，而是放在 `src/CLK.Todos.sln`，
  跟所有專案（`CLK.Todos`、`CLK.Todos.Web`、`CLK.Todos.Accesses`…）同一層。
- 每個專案在 sln 裡的路徑是相對 `src/` 的（例如 `CLK.Todos.Web\CLK.Todos.Web.csproj`），
  不再有一層虛擬的 `src` 方案資料夾。

**為什麼這樣做：** repo 根目錄只留 `README.md`、`.gitignore`、`LICENSE`、`docs/` 這類
「跟原始碼本身無關」的東西，所有跟 .NET 建置有關的東西（sln、csproj）都收在 `src/`
底下，結構更乾淨、之後如果要加 `tests/` 資料夾也對稱。

### 3. Repository 實作放在獨立的 `{Solution}.Accesses` 專案

- 新增 `src/CLK.Todos.Accesses` 類別庫，參照 Domain 專案（`CLK.Todos`），
  專門存放 `ITodoRepository` 等介面的**實作**。
- 資料夾結構跟 Domain 專案對稱：實作放在 `Repositories/` 資料夾，
  namespace 為 `CLK.Todos.Accesses.Repositories`。
- 上層專案（`CLK.Todos.Web`）改參照 `CLK.Todos.Accesses`，不再自己放實作。

**為什麼這樣做：** 把「資料存取的實作細節」從 Web 專案抽出來，獨立成一個專案，
未來如果要同時支援多種儲存方式（例如 Mock + 真正的資料庫），可以在同一個
`Accesses` 專案（或視情況再拆多個）裡新增多個實作類別，Web 專案跟 Domain
完全不用感知底下換了什麼儲存機制。

### 4. 記憶體實作一律叫 `Mock`，不叫 `InMemory`

- `InMemoryTodoRepository` 更名為 `MockTodoRepository`。
- 之後任何「非真實持久化、只是先求能動」的實作，命名都用 `Mock{Entity}{介面名}`
  這個模式，跟真正接資料庫的實作（例如未來可能的 `EfTodoRepository`）明確區分。
