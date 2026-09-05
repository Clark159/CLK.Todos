---
name: dotnet-design-rules
description: CLK.Todos 專案的 .NET 設計規範（目錄分層、Namespace、Class 成員排序、Constructor/Method 慣例、Context／Repository／Entity／DI 規則）。撰寫或修改本 repo 內任何 .cs 檔案（含 Domain／Access／Host 層）前必讀，確保產出跟現有程式碼風格一致。來源文件：docs/architecture-notes.md，規則有異動以該文件為準。
user-invocable: true
---

# .NET 設計規範（AI 執行版）

本 skill 把 `docs/architecture-notes.md` 的規則轉成可直接照做的檢查清單。
規則用 `{Domain}`／`{Entity}`／`{entity}` 佔位符描述；本專案 `{Domain}` =
`Todo`（sln／目錄前綴），目前 `{Entity}` 有 `Todo`，新增 `User` 時
`{Entity}` = `User`。跟文件說明衝突時，以 `docs/architecture-notes.md`
為準（本 skill 只是操作化摘要，不是另一套規則）。

## 使用時機

- 在 `src/` 底下新增或修改任何 `.cs` 檔案之前，先套用本清單。
- 新增一個 Entity（例如 `User`）時，依序做完「新增 Entity 檢查清單」
  的每一步，缺一步就回頭補。
- 寫完後跑一次「自我檢查清單」，逐條核對，不合就修到合為止。

## 1. Workspace 目錄

- 原始碼一律進 `src/`，跟 `.sln` 同層；測試專案未來放 `tests/`（跟
  `src/` 同層，目前還沒有）。
- `docs/` 放架構／設計文件；`README.md`／`.gitignore` 放 repo 根目錄。
- repo 根目錄不放任何 `.sln`／專案檔。

## 2. Architecture 分層

- `{Domain}`：Domain 層，類別庫，不依賴任何框架。提供 Entity、
  Repository 介面、Context。
- `{Domain}.Accesses`：Access 層，類別庫。提供 Repository 介面的實作
  （Mock／未來的 Ef 等）。
- `{Domain}.WebApp` 等：Host 層。可以有多個 Host 專案，都同時依賴
  `{Domain}` ＋ `{Domain}.Accesses`。
- 相依方向單向：Domain 不依賴任何層；Access 依賴 Domain；Host 依賴
  Domain＋Access。不可逆向、不可循環參照。

## 3. Namespace

- 一個專案內所有檔案 namespace 都等於專案名稱本身，不因資料夾加後綴。
- 一律 file-scoped：`namespace X;`，不用大括號區塊。
- `using` 在檔案最上方，跟 `namespace` 空一行；沒有 `using` 就直接從
  `namespace` 開始。
- `{Domain}`／`{Domain}.Accesses` 不拆資料夾，檔案放專案根目錄；
  `{Domain}.WebApp` 維持 MVC 慣例資料夾（`Controllers/`／`Models/`／
  `Views/`）。

## 4. Class 成員排序

固定順序，只列出「實際有內容」的分類，標頭下第一個成員不空行、同分類
內成員間空一行、不同分類間空兩行；`using` 不算分類、不加標頭：

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

- 呼叫自己（含繼承來）的成員一律不加 `this.`（含 Controller 繼承來的
  `ModelState`／`HttpContext`／`View(...)`／`RedirectToAction(...)`）。
- 此排序只管類別／介面內部，`Program.cs`（top-level statements）不適用。

## 5. Field

- 命名：`_` + 參數命名（注入型別去介面字首 `I`，字首轉小寫）。
- `// Fields` 分類內 lock 物件排最前面，其餘欄位接在後面。

## 6. Constructor

- 參數命名：注入型別去介面字首 `I`（若是介面），字首轉小寫。
- 先檢查合約（`// Contracts`），再用 `// Default` 標籤把參數存進欄位，
  兩者之間空一行、`// Default` 跟賦值本身不空行。
- 一律先存進私有欄位，不直接賦值給屬性或到處傳遞。
- 參照型別參數：`ArgumentNullException.ThrowIfNull(參數)`。

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
}
```

## 7. Properties

- 公開屬性命名：注入型別去介面字首 `I`，維持 PascalCase。
- 一律用完整 `get { return ...; }`，不用 `=>`（自動屬性 `{ get; set; }`
  不受影響）。
- `get` 只有一個 `return` 時整段縮成一行，不加 `// Return` 標籤。

## 8. Method

- 方法本體拆平鋪區塊，每區塊前加單字標籤；第一個標籤前不空行，後面每個
  標籤前空一行。標籤只提示角色，不把邏輯寫進註解。優先從這些字挑：
  `// Contracts`（參數合約檢查，含 MVC `ModelState.IsValid` 與路由/表單
  id 一致性檢查）、`// Search`（查詢）、`// Execute`（核心動作／狀態
  轉換）、`// Return`（正常結果的最後一個 return）、`// Lock`（進入
  `lock`）、`// Create`（建立接下來要用的物件實例）。
- `// Contracts` 放方法最前面：參照型別 `ArgumentNullException.
  ThrowIfNull`；字串 `ArgumentException.ThrowIfNullOrWhiteSpace`；值型別
  不用檢查。每個檢查獨立一行，不用 `||`／`&&` 合併，檢查間不空行、跟後續
  邏輯空一行。
- `// Return` 只標「交付結果」的最後一個 return；guard clause 提前
  return／丟例外不標、一律省略大括號寫成一行；回傳 `void` 的方法不需要
  `// Return`。
- 只有一步的方法（如 `FindById`）不用額外標籤，`// Return` 就夠。

## 9. Context

- `{Domain}Context` 是 Domain 入口：建構子注入所有 Repository 介面，用
  唯讀屬性對外提供。
- 每個 `{Domain}` 只有一個 Context，新增 Entity 是在既有 Context 上加一
  個 Repository 屬性，不是另開新 Context。
- 外部呼叫端（Controller 等）注入 `{Domain}Context`，不直接注入個別
  Repository 介面。
- 本專案：`TodoContext` 是 `Todo` 這個 `{Domain}` 的 Context，新增
  `User` Entity 時在 `TodoContext` 加 `IUserRepository` 屬性，不建立
  `UserContext`。

## 10. Repository

- 命名：介面 `I{Entity}Repository`；Mock 實作 `Mock{Entity}Repository`；
  真實資料庫實作 `{技術}{Entity}Repository`（例如 `EfUserRepository`）。
- 方法固定順序：`Add` → `Update` → `Remove` → `FindBy...`（單筆）→
  `FindAll`（全部）→ `FindAllBy...`（全部＋條件）。
- 刪除方法一律叫 `Remove`，不叫 `Delete`。
- 查詢命名：`Find` 開頭回傳 `{Entity}?`（單筆）；`FindAll` 開頭回傳
  `IReadOnlyList<{Entity}>`（多筆）。
- `Add` 不檢查主鍵重複；`Add`／`Update` 都把 `CreateTime`／`UpdateTime`
  蓋回 `DateTime.UtcNow`（`Add` 兩個都蓋，`Update` 只蓋 `UpdateTime`，
  不信任呼叫端傳進來的值）。
- 失敗語意：Query（`Find` 開頭）用回傳值本身表達找不到（`null`／空集合）；
  Command（`Add`／`Update`／`Remove`）回傳 `void`，找不到就丟
  `KeyNotFoundException`。
- Repository 只放資料存取方法，不放業務邏輯／狀態轉換（不開
  `Toggle{XX}(Guid {entity}Id)` 這種方法）。

```csharp
void Add({Entity} entity);
void Update({Entity} entity);
void Remove(Guid {entity}Id);
{Entity}? FindByXX(Guid {entity}Id);
IReadOnlyList<{Entity}> FindAll();
IReadOnlyList<{Entity}> FindAllByXX();
```

## 11. Entity

- 主鍵：屬性 `{Entity}Id`（不要單純 `Id`），型別 `Guid`，屬性預設值
  `Guid.CreateVersion7()`（不是 `Guid.NewGuid()`、不是 `int` 流水號）。
- 主鍵變數／參數統一命名 `{entity}Id`，貫穿 Repository、Controller
  action 參數、MVC 路由樣板 `{{entity}Id?}`、View 的
  `asp-route-{entity}Id`，不留通用的 `id`。
- 稽核時間戳記：`CreateTime`／`UpdateTime`，型別 `DateTime`，UTC，預設值
  `DateTime.UtcNow`（不是 `DateTime.Now`／`CreatedAt`）。
- 狀態轉換方法（例如 `ToggleDone()`）不用自己碰 `UpdateTime`，交給
  Repository `Update` 蓋。
- 充血模型：跟 Entity 自身狀態有關的業務邏輯寫成 Entity 方法，不寫進
  Repository／Controller。呼叫端流程固定：Repository 查出 Entity →
  呼叫 Entity 方法改變狀態 → Repository 存回去。
- 業務欄位（非主鍵、非稽核時間戳記）一律用 `DataAnnotations` 屬性保護
  （依型別／規則挑 `[Required]`／`[StringLength]`／`[EmailAddress]` 等），
  不要只靠 Controller 的 `ModelState.IsValid` 裸檢查，也不要在 Entity
  方法或 Controller 手寫等價 if 判斷。
- `ErrorMessage` 用「參數驗證」角度陳述違反的規則（`{屬性} 不可為空`、
  `{屬性} 長度不可超過 N 字`），不要用祈使句指示使用者動作（不要
  `請輸入 XXX`）；跟 Method／Constructor `// Contracts` 的合約檢查訊息
  同一種語氣。

```csharp
[Required(ErrorMessage = "Email 不可為空")]
[EmailAddress(ErrorMessage = "Email 格式不正確")]
[StringLength(100, ErrorMessage = "Email 長度不可超過 100 字")]
public string Email { get; set; } = string.Empty;
```

## 12. Dependency Injection

- Domain（`{Domain}Context`）與 Access 層（Repository 實作）在 DI 容器
  一律用 `AddSingleton`。
- 全部寫在 Host 層進入點（`Program.cs`）。
- 註冊順序：先 Repository 介面對實作，再 `{Domain}Context`（跟建構子
  相依方向一致）。

```csharp
builder.Services.AddSingleton<ITodoRepository, MockTodoRepository>();
builder.Services.AddSingleton<IUserRepository, MockUserRepository>();
builder.Services.AddSingleton<TodoContext>();
```

## 新增 Entity 檢查清單（以 `{Entity}` = User 為例）

1. `src/CLK.Todos/User.cs`：Entity，主鍵 `UserId`（`Guid`／
   `CreateVersion7()`）＋ `CreateTime`／`UpdateTime`＋業務欄位（用
   `DataAnnotations` 保護，`ErrorMessage` 陳述違反的規則），狀態轉換
   邏輯（若有）寫成方法。
2. `src/CLK.Todos/IUserRepository.cs`：Repository 介面，方法順序照
   §10 樣板。
3. `src/CLK.Todos/TodoContext.cs`：加一個 `IUserRepository` 唯讀屬性＋
   建構子參數，不開新 Context。
4. `src/CLK.Todos.Accesses/MockUserRepository.cs`：非持久化實作，
   lock／Search／Execute 分區塊，失敗語意照 §10。
5. `src/CLK.Todos.WebApp/Program.cs`：`AddSingleton<IUserRepository,
   MockUserRepository>()`，放在 Repository 註冊區塊、`TodoContext`
   註冊之前。
6. `src/CLK.Todos.WebApp/Controllers/UsersController.cs`：CRUD action，
   注入 `TodoContext`（不直接注入 `IUserRepository`），方法標籤照
   §8，路由參數用 `userId`。
7. Views（若需要 UI）：路由樣板與 `asp-route-userId` 對應 `UserId`。

## 自我檢查清單（寫完程式碼後逐條核對）

- [ ] Namespace file-scoped，且等於專案名稱。
- [ ] 類別成員分類順序、空行規則正確；不必要的 `this.` 都拿掉。
- [ ] 欄位／參數／屬性命名鏈（`I{X}` → `{x}` 參數 → `_{x}` 欄位 →
      `{X}` 屬性）一致。
- [ ] 建構子：`// Contracts` 檢查 not-null → 空一行 → `// Default`
      賦值進欄位。
- [ ] 方法內區塊標籤正確、guard clause 省略大括號、`// Return` 只標
      最後交付結果的 return。
- [ ] Repository 方法順序＋命名（`Find`／`FindAll`）＋失敗語意
      （Query 回傳值／Command 丟例外）都符合 §10。
- [ ] Entity 主鍵型別／命名／預設值、稽核時間戳記命名／型別／UTC 都
      符合 §11。
- [ ] 業務欄位都有 `DataAnnotations` 保護，`ErrorMessage` 是陳述式
      （`不可為空`／`不可超過…`），沒有祈使句（`請輸入…`）。
- [ ] `{Domain}Context`（`TodoContext`）沒有為新 Entity 另開 Context。
- [ ] DI 註冊都在 `Program.cs`，順序＋生命週期（Singleton）符合 §12。
- [ ] Controller／路由／View 用 `{entity}Id`，不是泛用 `id`。
