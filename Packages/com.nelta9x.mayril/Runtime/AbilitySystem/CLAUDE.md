# CLAUDE.md - AbilitySystem

## Build & Test
- **Run Tests**: `"/Applications/Unity/Hub/Editor/6000.3.2f1/Unity.app/Contents/MacOS/Unity" -batchmode -runTests -projectPath "/Users/nelta/Projects/Mayril" -testResults "/Users/nelta/Projects/Mayril/test_results.xml" -testPlatform PlayMode -testFilter "Mayril.Tests.AbilitySystem"`
- **Test Location**: `Packages/com.nelta9x.mayril/Tests/Runtime/AbilitySystem/`

## Code Style
- **Naming**: PascalCase for public, _camelCase for private fields.
- **No Magic**: Avoid Reflection or intricate auto-wiring. Use Explicit Registration.
- **Pooling**: Always use `EffectInstancePool` (via ASC) for effects. Never `new EffectInstance()`.
- **Tags**: Use `GameTag` struct, never strings.
- **Docs**: XML Documentation (///) required for public APIs.
