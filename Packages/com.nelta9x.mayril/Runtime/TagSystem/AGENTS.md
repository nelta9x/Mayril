# TagSystem

Mayril의 **TagSystem**은 **계층 구조(Hierarchy)**를 지원하는 고성능 태그 시스템입니다. 
문자열 기반("Status.Debuff.Stun")으로 정의되지만, 런타임에는 **Int ID**와 **Container Expansion** 기법을 사용하여 `O(1)` 검색 성능을 보장합니다.

## Core Architecture

### 1. ID-based & Hashing
- **String to ID**: FNV-1a 해싱을 사용하여 `string`을 `int` ID로 변환합니다.
- **Central Management**: `GameTagManager`가 ID<->String 매핑과 부모 관계 캐싱을 담당합니다.

### 2. Hierarchical Expansion (Container Logic)
`GameTagContainer`는 태그 추가 시 **부모 태그의 카운트도 함께 증가**시키는 "Expansion" 전략을 사용합니다.

- **Example**: `AddTag("Status.Debuff.Stun")` 수행 시
  1. `Status.Debuff.Stun` Count++
  2. `Status.Debuff` Count++ (Implicit)
  3. `Status` Count++ (Implicit)

- **Benefit**: `HasTag("Status.Debuff")` 호출 시 별도의 트리 순회 없이 딕셔너리(`_tagCounts`) 조회 한 번(`O(1)`)으로 즉시 `True`를 반환합니다.

---

## Key Components

| Class | Role |
|-------|------|
| **GameTag** | `struct`. ID와 이름을 래핑. **직렬화 시에는 이름**을, **런타임에는 ID**를 사용. |
| **GameTagContainer** | 핵심 컨테이너. `AddTag`/`RemoveTag`/`HasTag`. 계층 로직이 캡슐화되어 있음. |
| **GameTagManager** | `static`. 태그 등록 및 부모 관계(`GetParentTagIds`) 캐싱. 해싱 함수(`StringToHash`) 포함. |

---

## Best Practices & Anti-Patterns

### 1. Avoid String Hashing in Hot Paths
`GameTag`의 문자열 생성자는 해싱 비용이 있습니다. `Update` 루프 내부에서 문자열로 태그를 생성하지 마십시오.

```csharp
// ❌ WRONG: 매 프레임 해싱 발생
void Update() {
    if (container.HasTag(new GameTag("Status.Dead"))) { ... }
}

// ✅ RIGHT: 멤버 변수나 캐싱된 태그 사용
private static readonly GameTag Tag_Dead = new GameTag("Status.Dead");
void Update() {
    if (container.HasTag(Tag_Dead)) { ... }
}
```

### 2. Hierarchy naming
태그 이름은 `.`(공백 없음)으로 구분합니다. 
- `Type.Subtype.Leaf` 패턴을 권장합니다.
- 예: `State.CrowdControl.Stun`

### 3. Container Mutability
`GameTagContainer`는 참조 카운팅을 하므로, 동일한 태그를 두 번 `Add`하면 `Remove`도 두 번 해야 사라집니다. (Reference Counting)

---

## Snippet

```csharp
// 1. Definition (Member variable for cache)
private readonly GameTag _stunTag = new GameTag("Status.CC.Stun");

// 2. Add
container.AddTag(_stunTag);

// 3. Check (Implicit Parent Support)
// "Status.CC.Stun"을 넣었지만 "Status.CC"도 True 반환
bool isCC = container.HasTag(new GameTag("Status.CC")); // true
```
