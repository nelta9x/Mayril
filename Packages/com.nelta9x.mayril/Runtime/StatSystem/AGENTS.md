# StatSystem

Mayril의 StatSystem은 네트워크 동기화를 지원하고 AbilitySystem과 통합된 유연한 스탯 시스템입니다.
`BaseValue`(기본값)에 여러 `StatModifier`(버프/디버프)를 연산하여 `CurrentValue`(최종값)를 도출합니다.

## 아키텍처

### 핵심 컴포넌트

- **`StatValue`**: 스탯의 핵심 클래스. `NetworkVariableBase`를 상속받아 값이 변경될 때 클라이언트와 동기화됩니다.
- **`IStatSet`**: 스탯 컨테이너가 구현해야 할 인터페이스입니다. `GetStatValue`를 통해 태그 기반 검색을 지원해야 합니다. 기본 구현체(`StatSet`)는 제거되었습니다 (Minimalism).
- **`GameTag`**: 문자열이나 Enum 대신 계층형 태그 시스템을 사용하여 스탯을 식별합니다.

### 계산 공식

최종 값(`CurrentValue`)은 다음 순서로 계산됩니다:

```math
CurrentValue = (BaseValue + \sum Additive) \times (1 + \sum Multiplicative) + \sum Fixed
```

1.  **Additive**: 기본값에 합산 (예: 스탯 보너스)
2.  **Multiplicative**: 합산된 값에 곱연산 (예: 퍼센트 증가)
3.  **Fixed**: 최종적으로 고정값 합산 (예: 최종 데미지 추가)

## 사용 가이드

### 1. 스탯 정의하기 (`IStatSet` 구현)

캐릭터의 스탯을 정의하려면 `NetworkBehaviour`를 상속받고 `IStatSet` 인터페이스를 구현합니다. `StatSet` 베이스 클래스는 존재하지 않으므로, 스탯 저장 및 검색 로직을 직접 구현해야 합니다. 이는 개발자에게 구현의 자유(Switch-case vs Dictionary)를 제공합니다.

```csharp
using Mayril.StatSystem;
using Mayril.TagSystem;
using Unity.Netcode;

public class CharacterStats : NetworkBehaviour, IStatSet
{
    // 순수 프로퍼티로 정의
    public StatValue Health { get; private set; } = new StatValue(100f);
    public StatValue Attack { get; private set; } = new StatValue(10f);

    // 스탯 검색 구현 (Switch-Case 사용 시 딕셔너리 할당조차 없음)
    public StatValue GetStatValue(GameTag tag)
    {
        // O(1) 정수 ID 비교
        if (tag.Id == GameTags.CharacterHealth.Id) return Health;
        if (tag.Id == GameTags.CharacterAttack.Id) return Attack;
        return null;
    }

    // 값 변경 전 검증/보정 (예: 체력은 0 밑으로 내려갈 수 없음)
    public void OnStatValueChanging(StatValue stat, ref float newCurrentValue)
    {
        if (stat == Health)
        {
            newCurrentValue = Mathf.Max(0, newCurrentValue);
        }
    }

    // 값 변경 후 로직 (예: 체력이 0이 되면 사망 처리)
    public void OnStatValueChanged(StatValue stat)
    {
        if (stat == Health && stat.CurrentValue <= 0)
        {
            // Die()
        }
    }
}
```

### 2. 스탯 사용하기 (AbilitySystem 연동)

### 2. 스탯 사용하기 (AbilitySystem 연동)

`AbilitySystemComponent`에 스탯을 명시적으로 등록해야 합니다.

```csharp
private void Awake()
{
    // ... 스탯 초기화
    
    // ASC에 등록
    GetComponent<AbilitySystemComponent>().RegisterStatSet(this);
}
```

```csharp
// 태그로 스탯 가져오기 (문자열 등록 불필요)
var healthStat = abilitySystem.GetStat(new GameTag("Character.Health"));
```

### 3. 모디파이어 적용하기

`Effect` 시스템을 통해 적용하거나, 직접 코드로 적용할 수 있습니다.

```csharp
// 공격력 +5 (Additive)
var buff = new StatModifier(StatModifierType.Additive, 5f);
stat.AddModifier(buff);
```

> [!IMPORTANT]
> `StatModifier`는 구조체(`struct`)이므로 값 기반으로 비교됩니다. 동일한 타입과 값을 가진 모디파이어를 구분하려면 별도의 관리 로직이 필요할 수 있습니다.

## 모범 사례

- **초기화**: `StatValue`는 `NetworkVariable`이므로 반드시 `OnNetworkSpawn` 이후에 사용해야 안전합니다.
- **GameTag 사용**: 스탯 이름은 하드코딩된 문자열 대신 `GameTag` 상수를 정의하여 사용하는 것을 권장합니다.
- **스탯 검색 최적화**: 스탯 개수가 적다면 `Dictionary` 대신 `Switch-Case` 문을 사용하여 메모리 할당을 0으로 만드는 것을 권장합니다.

## 설계 철학 (Design Philosophy)

이 시스템은 **Simplicity**, **Explicitness**, **Performance**를 최우선으로 설계되었습니다.

1.  **Immutability & Explicitness**:
    *   `StatSet` 베이스 클래스 없이 `IStatSet` 인터페이스를 직접 구현합니다. 이는 모든 초기화 및 검색 로직을 코드에 명시적으로 드러내어("No Magic"), 동작을 예측 가능하게 만듭니다.
2.  **Data-Oriented Design**:
    *   `StatModifier`와 같은 빈번한 데이터는 `struct`를 사용하고, 단일 `List` 순회로 처리하여 메모리 할당을 최소화하고 캐시 효율성을 극대화합니다.
3.  **Integer Identity**:
    *   문자열 비교 대신 정수형 ID 기반의 `GameTag`를 사용하여 빠른 검색(`O(1)`)을 보장합니다.

## 안티 패턴 (Anti-Patterns)

- ❌ **`OnValidate`에서 스탯 초기화**: 스탯은 네트워크 객체이므로 런타임(`Awake` or `OnNetworkSpawn`)에 초기화되어야 합니다.
- ❌ **문자열로 스탯 검색**: `GetStat(new GameTag("Health"))` 대신 `public static readonly GameTag Health = ...` 상수를 정의하여 사용하세요. 오타 실수를 방지하고 성능을 높입니다.
- ❌ **컨테이너 과잉 엔지니어링**: `IStatSet` 구현체는 단순해야 합니다. 복잡한 로직은 `Ability`나 `Effect`로 위임하세요.
