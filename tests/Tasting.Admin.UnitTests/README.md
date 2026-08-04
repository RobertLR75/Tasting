# Frontend Testing Strategy

This directory contains bUnit and integration tests for the Tasting Admin frontend.

## Structure

```
tests/Tasting.Admin.UnitTests/
├── Components/          # Shared component tests (SearchBar, ActionButton, etc.)
├── Features/
│   ├── Identity/       # Users page tests
│   ├── Catalog/        # Breweries page tests
│   └── Arrangement/    # Arrangements page tests
├── Fixtures/           # Mock API responses and test data
└── Builders/           # Fluent builders for test data
```

## Running Tests

### All tests
```bash
dotnet test tests/Tasting.Admin.UnitTests
```

### Specific test class
```bash
dotnet test tests/Tasting.Admin.UnitTests --filter "ClassName=SearchBarTests"
```

### Watch mode
```bash
dotnet watch --project tests/Tasting.Admin.UnitTests test
```

---

## Component Testing with bUnit

### Example: SearchBar

```csharp
[Fact]
public async Task SearchBar_Emits_SearchTerm_OnButtonClick()
{
    var emitted = "";
    var cut = RenderComponent<SearchBar>(parameters => parameters
        .Add(p => p.Label, "Search")
        .Add(p => p.SearchTerm, "test-term")
        .Add(p => p.OnSearch, EventCallback.Factory.Create<string>(
            null, x => emitted = x
        ))
    );

    var button = cut.Find("button");
    await button.ClickAsync(new());

    Assert.Equal("test-term", emitted);
}
```

### bUnit Syntax Cheat Sheet

```csharp
// Render component
var cut = RenderComponent<MyComponent>();

// Pass parameters
.Add(p => p.Label, "Value")

// Find elements
cut.Find("button")
cut.FindAll("tr")
cut.Find(".css-class")

// Wait for async operations
await cut.InvokeAsync(() => { /* */ });

// Check state
Assert.Contains("text", cut.Markup);
cut.MarkupMatches("<div>expected</div>");
```

---

## Page Testing with Mocks

### Example: UsersPage

```csharp
[Fact]
public async Task UsersPage_Renders_UserList()
{
    // Arrange
    var mockHttpClient = CreateMockHttpClient();
    mockHttpClient.Setup(c => c.GetFromJsonAsync<ListUsersResponse>(
        "/api/v1/users", It.IsAny<CancellationToken>()
    )).ReturnsAsync(new ListUsersResponse(
        new List<UserDto>
        {
            UserDtoBuilder.Default().Build(),
            UserDtoBuilder.Admin().Build()
        },
        total: 2
    ));

    var cut = RenderComponent<UsersPage>(parameters => parameters
        .AddCascadingValue<HttpClient>(mockHttpClient.Object)
    );

    // Act
    await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

    // Assert
    Assert.Contains("John Doe", cut.Markup);
}
```

---

## Contract Testing Against Mocks

### API Contract Verification

Before backend endpoints are ready, verify contracts using mock responses:

```csharp
[Fact]
public async Task ListUsersResponse_DeserializesCorrectly()
{
    var json = @"{
        ""users"": [
            { ""id"": 1, ""firstName"": ""John"", ""lastName"": ""Doe"", 
              ""email"": ""john@example.com"", ""role"": ""Admin"", ""status"": ""Active"" }
        ],
        ""total"": 1
    }";

    using var doc = JsonDocument.Parse(json);
    var response = JsonSerializer.Deserialize<ListUsersResponse>(json);

    Assert.NotNull(response);
    Assert.Single(response.Users);
    Assert.Equal("John", response.Users.First().FirstName);
}
```

---

## Test Data Builders

Build test objects fluently:

```csharp
// Default user
var user = UserDtoBuilder.Default().Build();

// Admin user
var admin = UserDtoBuilder.Admin().Build();

// Inactive user
var inactive = UserDtoBuilder.Inactive().Build();

// Custom user
var custom = UserDtoBuilder.Default()
    .WithFirstName("Jane")
    .WithEmail("jane@example.com")
    .WithRole("Admin")
    .Build();
```

---

## Best Practices

### 1. Test Isolation
- Each test is independent
- Use builders to create fresh test data
- Mock external HTTP calls

### 2. Arrange-Act-Assert Pattern
```csharp
[Fact]
public async Task Feature_Behavior_Expected()
{
    // Arrange
    var testData = UserDtoBuilder.Default().Build();
    
    // Act
    var result = component.Method(testData);
    
    // Assert
    Assert.True(result);
}
```

### 3. Naming Convention
- `{ComponentName}_{Scenario}_{ExpectedBehavior}`
- Example: `SearchBar_WithEmptyTerm_DoesNotEmit`

### 4. Mock External Dependencies
```csharp
var mockHttpClient = new Mock<HttpClient>();
mockHttpClient
    .Setup(c => c.GetFromJsonAsync<ListUsersResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(mockResponse);
```

---

## Writing Tests for Track Deliverables

### Track A: Shell & Auth
- [ ] LoginPage renders form
- [ ] LoginPage submits credentials
- [ ] AuthGuard redirects unauthenticated users
- [ ] MainLayout shows logout button
- [ ] NavMenu renders all feature links

### Track B: Shared Components
- [ ] SearchBar emits search term
- [ ] ActionButton is clickable
- [ ] StatusBadge shows correct color per status
- [ ] FormLayout displays error message
- [ ] DataTable renders items

### Track C: Users Slice
- [ ] UsersPage loads and displays users
- [ ] UsersPage filters by search term
- [ ] AddUserPage creates new user
- [ ] EditUserPage updates user
- [ ] ChangeRolePage updates role (guards enforced by backend)

### Track D: Breweries Slice
- [ ] BreweriesPage loads and displays breweries
- [ ] BreweryBeersPage loads beers for brewery
- [ ] AddBreweryPage creates new brewery
- [ ] AddBeerPage creates new beer

### Track E: Arrangements Slice
- [ ] ArrangementsPage shows status-aware actions
- [ ] AddBeersPage multi-selects beers
- [ ] AddParticipantsPage multi-selects users
- [ ] StatusChangePage transitions valid statuses

---

## CI/CD Integration

Tests run automatically on:
- Every push to a feature branch
- Every pull request
- Main branch before merge

Minimum requirement: **90% tests passing**

---

## Debugging Tests

### Run single test with verbose output
```bash
dotnet test --filter "SearchBarTests" --verbosity=detailed
```

### Breakpoint debugging
1. Set breakpoint in test
2. Run: `dotnet test --debugger`
3. Attach debugger when prompted

### Print markup for inspection
```csharp
Console.WriteLine(cut.Markup);  // Full component HTML
cut.DebugDump();               // Pretty-printed tree
```

---

## Future Enhancements

- [ ] Screenshot comparison tests (Percy.io)
- [ ] Accessibility testing (axe-core)
- [ ] E2E tests with Playwright
- [ ] Performance benchmarks
- [ ] Contract verification against Swagger/OpenAPI

