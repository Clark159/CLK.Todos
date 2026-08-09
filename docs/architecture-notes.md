# 架構慣例筆記

這份筆記記錄 CLK.Todos 練習過程中逐步確立下來的架構規則，之後會整理成 Skill
給其他團隊共用。內容只反映**目前有效**的規則；規則異動時直接更新對應章節，
不用時間順序流水帳記錄，避免舊規則跟新規則互相矛盾。

## 1. 專案結構與分層

```
src/
├── CLK.Todos.sln
├── CLK.Todos/              Domain 層
│   ├── Entities/
│   └── Repositories/       （只放介面）
├── CLK.Todos.Accesses/     資料存取實作層
│   └── Repositories/       （放介面的實作）
└── CLK.Todos.Web/          表現層（ASP.NET MVC）
```

- **相依方向**：`CLK.Todos.Web` → `CLK.Todos.Accesses` → `CLK.Todos`。
  Domain 專案（`CLK.Todos`）在最底層，不被任何專案以外的東西依賴，自己也不依賴任何人。
- **sln 位置**：`CLK.Todos.sln` 放在 `src/` 底下，跟所有專案同一層，
  不放在 repo 根目錄。repo 根目錄只留 `README.md`、`.gitignore`、`LICENSE`、`docs/`
  這類跟 .NET 建置無關的東西。

**為什麼這樣做：** repo 根目錄保持乾淨、之後加 `tests/` 也對稱；相依方向單向、
沒有循環參照，任何一層要抽換都不會牽動它下面的層。

## 2. Domain 專案（`CLK.Todos`）

- 純類別庫（`dotnet new classlib`），**只依賴 .NET BCL**，不參照 ASP.NET Core
  或任何基礎設施套件（`System.ComponentModel.DataAnnotations` 屬於 BCL，可以用）。
- **Entity** 放在 `Entities/` 資料夾，namespace 為 `CLK.Todos.Entities`。
  例如 `Entities/Todo.cs`。
- **Repository 介面**放在 `Repositories/` 資料夾，namespace 為 `CLK.Todos.Repositories`。
  只放介面，**不放實作**——實作屬於基礎設施細節，見第 3 節。
  - 命名一律 `I{Entity}Repository`（例如 `ITodoRepository`）。
  - 方法用領域語言命名（`GetAll` / `GetById` / `Add` / `Update` / `Delete`），
    不要洩漏儲存細節（例如不要叫 `SelectFromDb`）。

**為什麼這樣做：** Domain 專案不依賴任何框架，之後才能被其他專案型態（Console、
Worker、不同的 Web 框架）重複使用而不用背 ASP.NET Core 的相依性；「規格」（介面）
跟「實作」分開，換實作時 Domain 完全不用改。

## 3. 資料存取實作層（`CLK.Todos.Accesses`）

- 獨立類別庫，參照 Domain 專案（`CLK.Todos`），**專門存放 Repository 介面的實作**。
- 資料夾結構跟 Domain 專案對稱：實作放在 `Repositories/` 資料夾，
  namespace 為 `CLK.Todos.Accesses.Repositories`。
- 上層專案（`CLK.Todos.Web`）參照 `CLK.Todos.Accesses`，自己不放任何實作。

**為什麼這樣做：** 之後若要同時支援多種儲存方式（例如 Mock + 真正的資料庫），
在這個專案（或視情況再拆多個）新增實作類別即可，Web 專案跟 Domain 完全不用
感知底下換了什麼儲存機制。

## 4. 命名慣例

### Repository 介面

一律 `I{Entity}Repository`，例如 `ITodoRepository`。

### 非持久化實作

先求能動、資料不會真的存下來的實作，一律 `Mock{Entity}{介面名}`，
例如 `MockTodoRepository`。

### 真實資料庫的實作（尚未使用，先預留規則）

之後接真實資料庫時，一律 `{技術}{Entity}{介面名}`，例如 `EfTodoRepository`。

**為什麼這樣做：** 一看類別名稱就知道它是不是「真的會持久化」，避免 Mock 跟真正
接資料庫的實作混用同一套命名造成誤會。

## 5. DI 註冊慣例

- 在 `CLK.Todos.Web/Program.cs` 用 `AddSingleton<介面, 實作>()` 註冊，
  例如：`builder.Services.AddSingleton<ITodoRepository, MockTodoRepository>();`
- Controller 一律只依賴介面（例如 `ITodoRepository`），不直接依賴實作類別。
