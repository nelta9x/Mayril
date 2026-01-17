# InventorySystem Documentation

Mayril의 인벤토리 시스템은 서버 권한(Server Authoritative) 모델을 기반으로 하며, `Inventory` (네트워크 및 로직 조정)와 `ItemCollection` (순수 데이터 컨테이너)으로 구성됩니다.

## Architecture (아키텍처)

### 1. `Inventory` (Abstract Class)
- **역할**: `NetworkBehaviour`를 상속받아 네트워크 동기화를 담당하며, 인벤토리의 주요 트랜잭션(아이템 추가, 제거, 이동 등)을 관리합니다.
- **특징**:
  - `NetworkList<InventorySlot>`을 사용하여 슬롯 데이터를 관리합니다.
  - 추상 클래스이므로, 구체적인 아이템 생성 로직(`CreateItem`)은 상속받은 클래스에서 구현해야 합니다.
  - 서버에서만 상태 변경이 가능(`IsServerInstance` 체크)하며, 클라이언트는 `NetworkList`의 변경 이벤트를 통해 로컬 상태를 동기화합니다.

### 2. `ItemCollection`
- **역할**: 인벤토리 슬롯과 아이템 객체를 관리하는 순수 C# 클래스입니다.
- **특징**:
  - `InventorySlot` (구조체) 리스트와 `IItem` (객체) 리스트를 병렬로 관리합니다.
  - `Inventory` 내부에서 사용되며, 네트워크 의존성이 없습니다.
  - 빈 슬롯 검색, 인덱스 유효성 검사 등의 로우 레벨 로직을 수행합니다.

### 3. `InventorySlot` (Struct)
- **역할**: 네트워크 전송을 위한 경량화된 슬롯 데이터 구조체입니다.
- **데이터**: `ItemCode` (int)와 `Quantity` (int)만을 포함합니다.
- **동기화**: `INetworkSerializable`을 구현하여 Netcode for GameObjects와 호환됩니다.

---

## Implementation Guide (구현 가이드)

새로운 인벤토리 타입(예: 플레이어 인벤토리, 창고 등)을 만들려면 `Inventory` 클래스를 상속받아야 합니다.

### 필수 구현 사항

```csharp
public class PlayerInventory : Inventory
{
    // 아이템 데이터베이스나 팩토리를 참조해야 할 수 있습니다.
    [SerializeField] private ItemDatabase itemDatabase;

    protected override IItem? CreateItem(int itemCode, int quantity)
    {
        // 1. itemCode를 사용하여 아이템 정의(ItemDefinition)를 찾습니다.
        var itemDef = itemDatabase.GetItemDefinition(itemCode);
        if (itemDef == null) return null;

        // 2. 새로운 아이템 객체를 생성하여 반환합니다.
        return new Item(itemDef, quantity);
    }
}
```

---

## Network Synchronization (네트워크 동기화)

이 시스템은 **Server Authoritative** 방식을 따릅니다.

1.  **Action**: 클라이언트가 아이템 이동 요청을 보냅니다 (예: RPC를 통해).
2.  **Validation**: 서버는 요청을 검증하고 `Inventory` 메서드(`SwapSlots` 등)를 호출합니다.
3.  **State Change**: `Inventory`는 내부 `ItemCollection`을 업데이트하고, `_networkSlots` (NetworkList)를 수정합니다.
4.  **Synchronization**: `NetworkList`의 변경 사항이 자동으로 모든 클라이언트에게 전파됩니다.
5.  **Client Update**: 클라이언트의 `OnNetworkSlotsChanged`가 호출되어 로컬 `ItemCollection`을 서버 상태와 일치시킵니다. (이때 `CreateItem`이 호출되어 시각적/로직적 아이템 객체가 생성됩니다.)

> [!WARNING]
> 클라이언트 코드에서 `Inventory`의 상태 변경 메서드(예: `AddItem`)를 직접 호출하면 실패하거나(`IsServerInstance` 체크), 로컬 상태가 서버와 꼬일 수 있습니다. 항상 서버 RPC를 통해 요청해야 합니다.

---

## Key API Usage (주요 사용법)

모든 상태 변경 메서드는 **서버(Host/Server)** 에서만 호출해야 합니다.

### 1. 아이템 추가 (`AddItem`)
```csharp
// 자동 슬롯 배정 및 스택 병합 시도
var result = inventory.AddItem(newItem);

if (result.Success) {
    Debug.Log($"Item added at slot {result.SlotIndex}");
} else {
    Debug.Log("Inventory full!");
}
```

### 2. 특정 슬롯에 아이템 추가 (`AddItemAt`)
```csharp
// 특정 슬롯에 강제로 추가 시도 (이미 있으면 병합 시도)
var result = inventory.AddItemAt(newItem, targetSlotIndex);
```

### 3. 아이템 이동/교환 (`SwapSlots`)
```csharp
// 두 슬롯의 아이템을 교환하거나 병합합니다.
bool success = inventory.SwapSlots(indexA, indexB);
```

### 4. 아이템 나누기 (`SplitStack`)
```csharp
// sourceIndex의 아이템 중 일부(quantity)를 targetIndex로 이동
bool success = inventory.SplitStack(sourceIndex, targetIndex, quantity);
```

### 5. 인벤토리 확장/축소
```csharp
// 슬롯 5개 추가
inventory.Extend(5);

// 뒤에서부터 슬롯 2개 제거
inventory.Shrink(2);
```
