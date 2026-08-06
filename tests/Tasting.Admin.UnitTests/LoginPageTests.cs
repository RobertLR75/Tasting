using System.ComponentModel.DataAnnotations;
using Tasting.Admin.Features.Auth.Models;

namespace Tasting.Admin.UnitTests;

public sealed class LoginPageTests
{
    // --- LoginFormModel validation ---

    [Fact]
    public void LoginFormModel_WithValidEmailAndPassword_PassesValidation()
    {
        var model = new LoginFormModel { Email = "user@example.com", Password = "secret" };

        var results = Validate(model);

        Assert.Empty(results);
    }

    [Fact]
    public void LoginFormModel_WithEmptyEmail_FailsValidation()
    {
        var model = new LoginFormModel { Email = "", Password = "secret" };

        var results = Validate(model);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(LoginFormModel.Email)));
    }

    [Fact]
    public void LoginFormModel_WithInvalidEmailFormat_FailsValidation()
    {
        var model = new LoginFormModel { Email = "not-an-email", Password = "secret" };

        var results = Validate(model);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(LoginFormModel.Email)));
    }

    [Fact]
    public void LoginFormModel_WithEmptyPassword_FailsValidation()
    {
        var model = new LoginFormModel { Email = "user@example.com", Password = "" };

        var results = Validate(model);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(LoginFormModel.Password)));
    }

    [Fact]
    public void LoginFormModel_WithEmptyEmailAndPassword_ReturnsTwoValidationErrors()
    {
        var model = new LoginFormModel { Email = "", Password = "" };

        var results = Validate(model);

        Assert.True(results.Count >= 2, $"Expected at least 2 errors, got {results.Count}");
    }

    [Theory]
    [InlineData("a@b.com")]
    [InlineData("user.name+tag@domain.co.uk")]
    [InlineData("admin@tasting.no")]
    public void LoginFormModel_WithVariousValidEmails_PassesEmailValidation(string email)
    {
        var model = new LoginFormModel { Email = email, Password = "pass" };

        var results = Validate(model);

        Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(LoginFormModel.Email)));
    }

    // --- LoginRequest mapping ---

    [Fact]
    public void LoginRequest_MapsFromLoginFormModel_Correctly()
    {
        var model = new LoginFormModel { Email = "user@example.com", Password = "mypassword" };

        var request = new LoginRequest(model.Email, model.Password);

        Assert.Equal(model.Email, request.Email);
        Assert.Equal(model.Password, request.Password);
    }

    // --- LoginPage razor structure ---

    [Fact]
    public void LoginPage_ShouldUseEditForm()
    {
        var markup = ReadLoginPage();
        Assert.Contains("<EditForm", markup);
        Assert.Contains("OnValidSubmit", markup);
    }

    [Fact]
    public void LoginPage_ShouldUseDataAnnotationsValidator()
    {
        var markup = ReadLoginPage();
        Assert.Contains("<DataAnnotationsValidator", markup);
    }

    [Fact]
    public void LoginPage_EmailField_ShouldBindToModel()
    {
        var markup = ReadLoginPage();
        Assert.Contains("_model.Email", markup);
        Assert.Contains("For=", markup);
    }

    [Fact]
    public void LoginPage_PasswordField_ShouldBindToModel()
    {
        var markup = ReadLoginPage();
        Assert.Contains("_model.Password", markup);
    }

    [Fact]
    public void LoginPage_EmailField_ShouldHaveOnKeyDownHandler()
    {
        var markup = ReadLoginPage();
        var emailFieldSection = ExtractMudTextField(markup, "_model.Email");
        Assert.Contains("OnKeyDown", emailFieldSection);
    }

    [Fact]
    public void LoginPage_PasswordField_ShouldHaveOnKeyDownHandler()
    {
        var markup = ReadLoginPage();
        var passwordFieldSection = ExtractMudTextField(markup, "_model.Password");
        Assert.Contains("OnKeyDown", passwordFieldSection);
    }

    [Fact]
    public void LoginPage_ShouldNotContainDebugConsoleWriteLines()
    {
        var markup = ReadLoginPage();
        Assert.DoesNotContain("Console.WriteLine", markup);
    }

    [Fact]
    public void LoginPage_ShouldNotContainManualNullCheckGuard()
    {
        var markup = ReadLoginPage();
        Assert.DoesNotContain("string.IsNullOrWhiteSpace(email)", markup);
        Assert.DoesNotContain("string.IsNullOrWhiteSpace(password)", markup);
    }

    [Fact]
    public void LoginPage_ButtonShouldHaveOnClickHandler()
    {
        var markup = ReadLoginPage();
        Assert.Contains("HandleButtonClickAsync", markup);
    }

    // --- Helpers ---

    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }

    private static string ReadLoginPage()
        => File.ReadAllText(GetProjectFile("src/Frontend/Tasting.Admin/Features/Auth/Pages/LoginPage.razor"));

    private static string GetProjectFile(string relativePath)
        => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", relativePath));

    private static string ExtractMudTextField(string markup, string bindingTarget)
    {
        var start = markup.IndexOf($"@bind-Value=\"{bindingTarget}\"", StringComparison.Ordinal);
        if (start == -1) return "";
        // Walk back to find the opening <MudTextField
        var tagStart = markup.LastIndexOf("<MudTextField", start, StringComparison.Ordinal);
        if (tagStart == -1) return "";
        // Walk forward to find the closing />
        var tagEnd = markup.IndexOf("/>", start, StringComparison.Ordinal);
        if (tagEnd == -1) return "";
        return markup[tagStart..(tagEnd + 2)];
    }
}
