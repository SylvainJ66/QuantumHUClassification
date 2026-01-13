# Mutation Testing with Stryker.NET

This project uses [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/) for mutation testing on domain layers.

## What is Mutation Testing?

Mutation testing evaluates the quality of your tests by introducing small changes (mutations) to your code and checking if your tests catch these changes. If a test fails after a mutation, the mutant is "killed" (good). If all tests pass, the mutant "survived" (bad - indicates weak tests).

## Installation

Stryker.NET is installed globally:

```bash
dotnet tool install -g dotnet-stryker
```

## Running Mutation Tests

**IMPORTANT**: Always run Stryker from the test project directory to ensure it only targets that specific domain project.

### Write-Side Domain

```bash
cd ExtractHUContext/Write-Side/Tests/ExtractHUContext.WriteSide.Domain.Tests
dotnet stryker --open-report:html
```

This will:
- Mutate only `ExtractHUContext.WriteSide.Domain` project
- Run all unit tests from `ExtractHUContext.WriteSide.Domain.Tests`
- Generate and open an HTML report when complete

### Read-Side Domain

```bash
cd ExtractHUContext/Read-Side/Tests/ExtractHUContext.ReadSide.Domain.Tests
dotnet stryker --open-report:html
```

This will:
- Mutate only `ExtractHUContext.ReadSide.Domain` project
- Run all unit tests from `ExtractHUContext.ReadSide.Domain.Tests`
- Generate and open an HTML report when complete

## Configuration

Each domain test project has a `stryker-config.json` file with the following settings:

- **Target Framework**: .NET 10.0
- **Mutation Scope**: Only domain layer code (`../../../Src/<Context>.Domain/**/*.cs`)
- **Thresholds**:
  - High: 80% (excellent test coverage)
  - Low: 60% (acceptable test coverage)
  - Break: 50% (build fails below this)
- **Reporters**: HTML, Progress, ClearText
- **Concurrency**: 4 parallel tests

## Understanding Results

### Mutation Score

The mutation score is the percentage of killed mutants:

```
Mutation Score = (Killed Mutants / Total Mutants) × 100
```

### Thresholds

- **80%+** (High): Excellent - Your tests are robust
- **60-79%** (Low): Acceptable - Tests are decent but could improve
- **50-59%** (Break): Warning - Tests need improvement
- **<50%**: Failing - Test quality is insufficient

### Mutant Status

- **Killed**: Test failed after mutation (good - test caught the bug)
- **Survived**: All tests passed after mutation (bad - test didn't catch the bug)
- **Timeout**: Mutation caused infinite loop or very slow execution
- **No Coverage**: Code not covered by any test
- **Compilation Error**: Mutation resulted in code that doesn't compile (ignored)

## Viewing Reports

After running Stryker, open the HTML report:

```bash
# Windows
start StrykerOutput/reports/mutation-report.html

# Linux/Mac
open StrykerOutput/reports/mutation-report.html
```

The HTML report shows:
- Overall mutation score
- Per-file mutation scores
- Specific mutations and their status
- Code diff for each mutation

## Common Mutation Types

Stryker.NET applies these mutation types:

### Arithmetic Operators
- `+` → `-`, `*`, `/`, `%`
- `-` → `+`, `*`, `/`, `%`
- `*` → `+`, `-`, `/`, `%`
- `/` → `+`, `-`, `*`, `%`

### Comparison Operators
- `>` → `>=`, `<`, `<=`
- `>=` → `>`, `<`, `<=`
- `<` → `<=`, `>`, `>=`
- `<=` → `<`, `>`, `>=`
- `==` → `!=`
- `!=` → `==`

### Logical Operators
- `&&` → `||`
- `||` → `&&`
- `!` → removed

### Boolean Literals
- `true` → `false`
- `false` → `true`

### String Literals
- `"string"` → `""`
- `""` → `"Stryker was here!"`

### LINQ Methods
- `.Any()` → `.All()`
- `.First()` → `.Last()`
- `.Skip()` → `.Take()`
- etc.

## Best Practices

1. **Run Regularly**: Run mutation tests after implementing new features or refactoring
2. **Focus on Domain**: Mutation testing is most valuable for business logic in domain layers
3. **Investigate Survivors**: When mutants survive, investigate why and improve tests
4. **Set Realistic Thresholds**: 100% is rarely achievable; 80%+ is excellent
5. **Use with Code Coverage**: Mutation testing complements but doesn't replace code coverage

## Improving Mutation Scores

If mutants survive:

1. **Check Assertions**: Ensure tests verify actual behavior, not just that code runs
2. **Test Edge Cases**: Add tests for boundary conditions
3. **Verify Return Values**: Don't just check for exceptions; verify correct results
4. **Test All Paths**: Ensure all conditional branches are tested
5. **Assert State Changes**: Verify that domain entities change state correctly

## Performance Tips

- Use `--concurrency` to control parallel execution
- Use `--diff` to only test changed files
- Use `--mutate` to target specific files or folders
- Consider running on CI/CD only for critical code

## CI/CD Integration

Example GitHub Actions workflow:

```yaml
- name: Run Mutation Tests (Write-Side Domain)
  run: |
    cd ExtractHUContext/Write-Side/Tests/ExtractHUContext.WriteSide.Domain.Tests
    dotnet stryker --reporter cleartext
```

## Example Output

```
[16:50:00 INF] Mutating 42 files
[16:52:15 INF] 127 mutants created
[16:55:30 INF] Mutation testing completed

Mutation Score: 78.74%
- Killed: 100
- Survived: 15
- Timeout: 2
- No Coverage: 10

Build succeeded - mutation score above threshold (60%)
```

## Troubleshooting

### "No tests found"
- Ensure test project builds successfully
- Check that `test-projects` in config matches your project file

### "Build failed"
- Stryker needs a successful build first
- Run `dotnet build` before `dotnet stryker`

### Slow Performance
- Reduce concurrency if memory-constrained
- Use `--diff` to only test changed code
- Exclude slow tests with `--ignore-methods`

## Additional Resources

- [Stryker.NET Documentation](https://stryker-mutator.io/docs/stryker-net/introduction/)
- [Configuration Reference](https://stryker-mutator.io/docs/stryker-net/configuration/)
- [Mutation Types](https://stryker-mutator.io/docs/stryker-net/mutations/)
