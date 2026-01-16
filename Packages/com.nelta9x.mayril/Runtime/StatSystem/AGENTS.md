# StatSystem

Mayril의 StatSystem은 네트워크 동기화를 지원하는 유연한 스탯 시스템입니다.
`BaseValue`(기본값)에 여러 `StatModifier`(버프/디버프)를 연산하여 `CurrentValue`(최종값)를 도출합니다.

## 아키텍처

### 핵심 컴포넌트

- **`StatValue`**: 스탯의 핵심 클래스. `NetworkVariableBase`를 상속받아 값이 변경될 때 클라이언트와 동기화됩니다.
- **`IStatSet`**: `StatValue`들을 포함하는 컨테이너 인터페이스입니다. (예: `CharacterStats`)
- **`StatModifier`**: 스탯에 영향을 주는 변경 요소입니다. (예: 공격력 +10, 이동속도 50% 증가)

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

캐릭터의 스탯을 정의하려면 `NetworkBehaviour`를 상속받고 `IStatSet`을 구현합니다.

```csharp
public class CharacterStats : NetworkBehaviour, IStatSet
{
    // 스탯 정의
    public readonly StatValue Health = new(100f);
    public readonly StatValue Attack = new(10f);

    public override void OnNetworkSpawn()
    {
        // StatValue는 자동으로 초기화됩니다.
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

### 2. 모디파이어 적용하기

```csharp
// 공격력 +5 (Additive)
var buff = new StatModifier(StatModifierType.Additive, 5f);
character.Stats.Attack.AddModifier(buff);

// 공격력 50% 증가 (Multiplicative, 0.5 = 50%)
var percentBuff = new StatModifier(StatModifierType.Multiplicative, 0.5f);
character.Stats.Attack.AddModifier(percentBuff);
```

### 3. 모디파이어 제거하기

```csharp
character.Stats.Attack.RemoveModifier(buff);
```

> [!IMPORTANT]
> `StatModifier`는 구조체(`struct`)이므로 값 기반으로 비교됩니다. 동일한 타입과 값을 가진 모디파이어를 구분하려면 별도의 관리 로직이 필요할 수 있습니다.

## 모범 사례

- **초기화**: `StatValue`는 `NetworkVariable`이므로 반드시 `OnNetworkSpawn` 이후에 사용해야 안전합니다.
- **성능**: `StatModifierContainer`는 최적화를 위해 LINQ 대신 `foreach` 루프를 사용합니다. 빈번한 스탯 변경에도 GC 할당을 최소화하도록 설계되었습니다.
