# EventSystem

Mayril의 **EventSystem**은 Zero-Overhead, Type-Safe를 지향하는 **정적 제네릭 이벤트 버스(Static Generic Event Bus)**입니다.
C#의 제네릭 정적 클래스 특성을 이용하여 컴파일 타임에 이벤트 채널을 분리, 딕셔너리 조회 비용을 제거했습니다.

## Core Architecture

### 1. Static Generic Pattern
```csharp
public static class EventBus<T> { ... }
```
- **Zero Lookup Cost**: `EventBus<A>`와 `EventBus<B>`는 별개의 클래스입니다. 런타임 딕셔너리 조회(O(1)~O(log n)) 없이 메서드를 직접 호출(Direct Call)합니다.
- **Type Safety**: 컴파일 타임에 메시지 타입이 보장됩니다. 잘못된 캐스팅 런타임 에러가 근본적으로 차단됩니다.

### 2. Double Buffering
이벤트 순회 중 구독이 변경될 때 발생하는 `CollectionModifiedException`을 방지합니다.

- **Frontend (`HashSet`)**: `Register`/`Unregister` 전용. 중복 구독을 O(1)로 방지합니다.
- **Backend (`List`)**: 실제 `Trigger` 순회용. `Trigger` 시점에 Frontend가 변경되었다면(Dirty) 동기화합니다.
- **Benefit**: 이벤트 루프 내에서 구독/해지가 발생해도, 현재 프레임의 순회는 안전하게 보장됩니다.

---

## Best Practices & Anti-Patterns

### 1. No Lambdas (람다 사용 금지)
**Classic Memory Leak.**

```csharp
// ❌ WRONG: 힙에 새로운 익명 대리자 객체 생성. 참조를 잃어버려 해제 불가.
EventBus<MyEvent>.Register(evt => Debug.Log(evt));

// ✅ RIGHT: 메서드 그룹 사용.
EventBus<MyEvent>.Register(OnEvent);
```
람다식은 등록 시 매번 새로운 인스턴스를 생성하므로 `Unregister`가 불가능합니다.

### 2. Lifecycle Management
`EventBus`는 정적(Static)이므로 씬 전환 시에도 구독자가 유지됩니다.
- `MonoBehaviour`: 반드시 `OnEnable` 등록, `OnDisable` 해제.
- **Dangling References**: 파괴된 객체의 메서드가 호출되면 `MissingReferenceException`을 유발합니다. `EventBus`는 이를 `try-catch`로 방어하지만, 성능 낭비와 로그 오염을 초래합니다.

---

## Code Reference

### Built-in Events
| Event | Context |
|-------|---------|
| `WorldStarted`/`Destroyed` | 씬(Scene) 생명주기 |
| `EntityPlayStarted`/`Ended` | 네트워크 객체(`Entity`) 생명주기 |

### Snippet
```csharp
public struct PlayerJumped { public int Force; }

// Sender
EventBus<PlayerJumped>.Trigger(new PlayerJumped { Force = 10 });

// Receiver
void OnEnable() => EventBus<PlayerJumped>.Register(OnJump);
void OnDisable() => EventBus<PlayerJumped>.Unregister(OnJump);
void OnJump(PlayerJumped msg) { ... }
```
