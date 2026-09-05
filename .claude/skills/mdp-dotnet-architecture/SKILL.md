---
name: mdp-dotnet-architecture
description: CLK.Todos 專案的 .NET 設計規範（目錄分層、Namespace、Class 成員排序、Constructor/Method 慣例、Context／Repository／Entity／DI 規則）。撰寫或修改本 repo 內任何 .cs 檔案（含 Domain／Access／Host 層）前必讀，確保產出跟現有程式碼風格一致。規則本文：同資料夾的 architecture.md（唯一正本，這份 SKILL.md 不重複規則內容）。
user-invocable: true
---

# .NET 設計規範（AI 執行版）

規則本文是同資料夾的 [architecture.md](architecture.md)——唯一正本，涵蓋全部 12 節
規則＋範例＋說明。本檔不重抄規則內容，只放「套用規則時」用得到的 AI
專屬工作流程，避免規則兩邊各存一份、改一邊忘了改另一邊。

## 使用時機

- 在 `src/` 底下新增或修改任何 `.cs` 檔案之前，**先完整讀一次
  [architecture.md](architecture.md)**，以它為準；本檔只是讀完之後的操作清單。
- 新增一個 Entity（例如 `User`）時，依序做完下面「新增 Entity 檢查
  清單」的每一步，缺一步就回頭補。
- 寫完後跑一次「自我檢查清單」，逐條核對，不合就修到合為止。
- 兩份文件內容衝突時，以 [architecture.md](architecture.md) 為準；發現衝突
  視同 Skill 過時，應該回頭修正本檔而不是硬套。

## 新增 Entity 檢查清單（以 `{Entity}` = User 為例，對照 §9～§12）

1. `src/CLK.Todos/User.cs`：Entity，主鍵 `UserId`（`Guid`／
   `CreateVersion7()`）＋ `CreateTime`／`UpdateTime`＋業務欄位（用
   `DataAnnotations` 保護，`ErrorMessage` 陳述違反的規則），狀態轉換
   邏輯（若有）寫成方法。
2. `src/CLK.Todos/IUserRepository.cs`：Repository 介面，方法順序照
   §10 樣板（`Add` → `Update` → `Remove` → `FindBy...` → `FindAll` →
   `FindAllBy...`）。
3. `src/CLK.Todos/TodoContext.cs`：加一個 `IUserRepository` 唯讀屬性＋
   建構子參數，不開新 Context。
4. `src/CLK.Todos.Accesses/MockUserRepository.cs`：非持久化實作，
   lock／Search／Execute 分區塊，失敗語意照 §10（Query 回傳值表達找不到，
   Command 丟 `KeyNotFoundException`）。
5. `src/CLK.Todos.WebApp/Program.cs`：`AddSingleton<IUserRepository,
   MockUserRepository>()`，放在 Repository 註冊區塊、`TodoContext`
   註冊之前；生命週期一律 Singleton。
6. `src/CLK.Todos.WebApp/Controllers/UsersController.cs`：CRUD action，
   注入 `TodoContext`（不直接注入 `IUserRepository`），方法標籤照
   §8，路由參數用 `userId`。
7. Views（若需要 UI）：路由樣板與 `asp-route-userId` 對應 `UserId`。

## 自我檢查清單（寫完程式碼後逐條核對）

- [ ] Namespace file-scoped，且等於專案名稱（§3）。
- [ ] 類別成員分類順序、空行規則正確；不必要的 `this.` 都拿掉（§4）。
- [ ] 欄位／參數／屬性命名鏈（`I{X}` → `{x}` 參數 → `_{x}` 欄位 →
      `{X}` 屬性）一致（§5～§7）。
- [ ] 建構子：`// Contracts` 檢查 not-null → 空一行 → `// Default`
      賦值進欄位（§6）。
- [ ] 方法內區塊標籤正確、guard clause 省略大括號、`// Return` 只標
      最後交付結果的 return（§8）。
- [ ] Repository 方法順序＋命名（`Find`／`FindAll`）＋失敗語意
      （Query 回傳值／Command 丟例外）都符合 §10。
- [ ] Entity 主鍵型別／命名／預設值、稽核時間戳記命名／型別／UTC 都
      符合 §11。
- [ ] 業務欄位都有 `DataAnnotations` 保護，`ErrorMessage` 是陳述式
      （`不可為空`／`不可超過…`），沒有祈使句（`請輸入…`）（§11）。
- [ ] `{Domain}Context`（`TodoContext`）沒有為新 Entity 另開 Context
      （§9）。
- [ ] DI 註冊都在 `Program.cs`，順序＋生命週期（Singleton）符合 §12。
- [ ] Controller／路由／View 用 `{entity}Id`，不是泛用 `id`（§11）。
