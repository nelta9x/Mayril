# TimerManager Documentation

`TimerManager`는 게임 내 시간 기반 이벤트를 예약하고 관리하는 시스템입니다. `World` 클래스에 종속되어 있으며, `Update` 루프를 통해 시간을 추적하고 이벤트를 실행합니다.

## Architecture (아키텍처)

- **`TimerManager`**: 핵심 관리자. 이벤트를 `MinHeap`에 저장하고 매 프레임 `PollEvents`를 호출하여 실행 시점이 된 이벤트를 처리합니다.
- **`TimerHandle`**: 예약된 이벤트를 식별하는 키입니다. 이벤트를 취소할 때 사용합니다.
- **`TimerEventMinHeap`**: 내부적으로 이벤트를 실행 시간 순으로 정렬하여 저장하는 자료구조입니다.

## Key API Usage (주요 사용법)

`TimerManager`는 `World.Instance.WorldTimerManager`를 통해 접근할 수 있습니다.

### 1. 이벤트 예약 (`PostEvent`)

```csharp
// 3초 뒤에 "Hello" 로그 출력
TimerHandle handle = World.Instance.WorldTimerManager.PostEvent(3.0f, () =>
{
    Debug.Log("Hello after 3 seconds!");
});
```

### 2. 이벤트 취소 (`CancelEvent`)

예약된 이벤트가 실행되기 전에 취소할 수 있습니다. 이미 실행되었거나 유효하지 않은 핸들인 경우 `false`를 반환합니다.

```csharp
bool isCancelled = World.Instance.WorldTimerManager.CancelEvent(handle);
if (isCancelled)
{
    Debug.Log("Timer cancelled successfully.");
}
```

### 3. 반복 타이머 구현

`TimerManager`는 기본적으로 반복 기능을 제공하지 않지만, 콜백 내부에서 다시 `PostEvent`를 호출하여 구현할 수 있습니다.

```csharp
void RecursiveTimer()
{
    Debug.Log("Tick!");
    // 1초 뒤에 다시 실행
    World.Instance.WorldTimerManager.PostEvent(1.0f, RecursiveTimer);
}
```

## Best Practices (모범 사례)

- **`TimerHandle` 관리**: 이벤트를 취소해야 할 가능성이 있다면 반환된 `TimerHandle`을 멤버 변수 등에 저장해두세요.
- **`null` 콜백**: `PostEvent`에 `null`을 전달하면 경고 로그가 출력되고 이벤트가 등록되지 않습니다.
- **예외 처리**: 콜백 내부에서 예외가 발생하더라도 `TimerManager`는 이를 잡아 로그를 출력하고, 다음 이벤트 처리를 계속 진행합니다(Crash 방지).
