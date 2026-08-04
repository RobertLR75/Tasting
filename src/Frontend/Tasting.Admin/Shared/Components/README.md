# Shared UI Component Library

This folder contains reusable MudBlazor-based admin components shared across all feature slices.

## Components

### 1. **SearchBar**
Text field + button for searching lists.

```razor
<SearchBar 
    Label="Search users..." 
    SearchTerm="@searchTerm"
    OnSearch="@HandleSearch" />

@code {
    private string searchTerm = "";
    
    private async Task HandleSearch(string term)
    {
        await LoadData(term);
    }
}
```

**Parameters:**
- `Label`: Placeholder label (default: "Search")
- `SearchTerm`: Current search value
- `OnSearch`: Callback when search button clicked

---

### 2. **ActionButton**
Styled button with icon support.

```razor
<ActionButton 
    Text="Add User" 
    Icon="@Icons.Material.Filled.Add"
    Color="Color.Primary"
    OnClick="@HandleAdd" />
```

**Parameters:**
- `Text`: Button label
- `Icon`: Material icon name
- `Variant`: Button variant (Filled, Outlined, Text)
- `Color`: Button color
- `Disabled`: Disable the button
- `OnClick`: Click handler

---

### 3. **StatusBadge**
Colored chip showing status with icon.

```razor
<StatusBadge Status="@user.Status" />
```

**Supported Statuses:**
- `Active` (Green, CheckCircle)
- `Inactive` (Red, Cancel)
- `Created` (Blue, NewReleases)
- `Started` (Orange, PlayCircle)
- `Completed` (Green, TaskAlt)
- `Canceled` (Red, RemoveCircle)

---

### 4. **FormLayout**
Wrapper for form pages with title, error display, and form.

```razor
<FormLayout Title="Edit User" ErrorMessage="@errorMessage">
    <FormField Label="First Name" @bind-Value="@firstName" />
    <FormField Label="Email" InputType="InputType.Email" @bind-Value="@email" />
    <ActionButton Text="Save" OnClick="@HandleSave" />
</FormLayout>
```

**Parameters:**
- `Title`: Form title
- `ErrorMessage`: Display error (if any)
- `ChildContent`: Form fields and buttons

---

### 5. **ErrorAlert**
Dismissible error alert.

```razor
<ErrorAlert Message="@errorMessage" OnClose="@HandleErrorClose" />
```

**Parameters:**
- `Message`: Error text
- `OnClose`: Callback when dismissed

---

### 6. **LoadingIndicator**
Progress bar + loading text.

```razor
<LoadingIndicator IsLoading="@isLoading" LoadingText="Saving..." />
```

**Parameters:**
- `IsLoading`: Show/hide the indicator
- `LoadingText`: Message to display

---

### 7. **PageHeader**
Page title + optional subtitle and breadcrumbs.

```razor
<PageHeader Title="Users" Subtitle="Manage all system users">
    <MudBreadcrumbs Items="@breadcrumbs" />
</PageHeader>
```

**Parameters:**
- `Title`: Page title
- `Subtitle`: Optional subtitle
- `BreadcrumbContent`: Optional breadcrumb area

---

### 8. **ConfirmDialog**
Reusable confirmation modal.

```csharp
var result = await Dialogs.ShowAsync<ConfirmDialog>(
    "Delete User?",
    new DialogParameters { { "Message", "This cannot be undone." } }
);
if (result?.Data is true) { /* confirmed */ }
```

**Parameters:**
- `Message`: Confirmation prompt
- `ConfirmText`: Confirm button label (default: "Confirm")

---

### 9. **DataTable**
Generic table with sorting and paging.

```razor
<DataTable Items="@users" TItem="UserDto">
    <HeaderContent>
        <MudTh>Name</MudTh>
        <MudTh>Email</MudTh>
        <MudTh>Status</MudTh>
    </HeaderContent>
    <RowTemplate Context="user">
        <MudTd>@user.FirstName @user.LastName</MudTd>
        <MudTd>@user.Email</MudTd>
        <MudTd><StatusBadge Status="@user.Status" /></MudTd>
    </RowTemplate>
</DataTable>
```

**Parameters:**
- `Items`: List of items to display
- `ToolBarContent`: Toolbar area (e.g., add button)
- `HeaderContent`: Table header
- `RowTemplate`: Each row renderer

---

### 10. **FormField**
Text input with label and optional error message.

```razor
<FormField 
    Label="Email" 
    InputType="InputType.Email"
    @bind-Value="@email"
    ErrorMessage="@emailError"
    Required="true" />
```

**Parameters:**
- `Label`: Field label
- `Value`: Text value (2-way binding)
- `InputType`: HTML input type
- `ErrorMessage`: Display error (if any)
- `Required`: Mark as required

---

## Usage Guidelines

1. **Import in your page:**
   ```razor
   @using Tasting.Admin.Shared.Components
   ```

2. **Compose complex forms:**
   ```razor
   <FormLayout Title="Add User" ErrorMessage="@errorMessage">
       <FormField Label="First Name" @bind-Value="@firstName" />
       <FormField Label="Email" InputType="InputType.Email" @bind-Value="@email" />
       <ActionButton Text="Create" OnClick="@HandleCreate" />
   </FormLayout>
   ```

3. **Show confirmation before destructive actions:**
   ```csharp
   var result = await DialogService.ShowAsync<ConfirmDialog>(
       "Delete?",
       new DialogParameters { { "Message", "This will be permanently deleted." } }
   );
   ```

4. **Error handling pattern:**
   ```csharp
   try 
   { 
       await apiClient.UpdateAsync(data);
       successMessage = "Updated successfully!";
   }
   catch (Exception ex)
   {
       errorMessage = ex.Message;
   }
   ```

---

## Component Testing

Each component is testable in isolation. See `tests/Tasting.Admin.UnitTests/Components/` for bUnit tests.

```csharp
[Test]
public async Task SearchBar_EmitsSearchTerm_WhenButtonClicked()
{
    var searchTerm = "test";
    var emitted = "";
    
    var cut = RenderComponent<SearchBar>(parameters => parameters
        .Add(p => p.OnSearch, new EventCallback<string>(null, 
            new Action<string>(x => emitted = x)))
    );
    
    cut.Find("button").Click();
    
    Assert.That(emitted, Is.EqualTo(searchTerm));
}
```

---

## Future Extensions

- **Pagination component** – Standalone pagination control
- **MultiSelect** – Checkbox list for multi-select forms
- **DatePicker** – Date/time range selection
- **FileUpload** – File drag-and-drop upload

