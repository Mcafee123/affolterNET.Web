namespace affolterNET.Web.Core.Services;

public class PermissionServiceTests
{
    [Fact]
    public void ExtractPermissionsFromAuthorizationClaim_WithScopedPermissions_ReturnsResourceAndAction()
    {
        // Arrange - typical Keycloak RPT authorization claim
        var authorizationClaim = """
        {
            "permissions": [
                {
                    "rsname": "admin-resource",
                    "scopes": ["view", "manage"]
                },
                {
                    "rsname": "user-resource",
                    "scopes": ["read", "create", "update", "delete"]
                }
            ]
        }
        """;

        // Act
        var permissions = PermissionService.ExtractPermissionsFromAuthorizationClaim(authorizationClaim);

        // Assert
        Assert.Equal(6, permissions.Count);
        Assert.Contains(permissions, p => p.Resource == "admin-resource" && p.Action == "view");
        Assert.Contains(permissions, p => p.Resource == "admin-resource" && p.Action == "manage");
        Assert.Contains(permissions, p => p.Resource == "user-resource" && p.Action == "read");
        Assert.Contains(permissions, p => p.Resource == "user-resource" && p.Action == "create");
        Assert.Contains(permissions, p => p.Resource == "user-resource" && p.Action == "update");
        Assert.Contains(permissions, p => p.Resource == "user-resource" && p.Action == "delete");
    }

    [Fact]
    public void ExtractPermissionsFromAuthorizationClaim_WithNoScopes_ReturnsResourceOnly()
    {
        // Arrange - permission without scopes
        var authorizationClaim = """
        {
            "permissions": [
                {
                    "rsname": "default-resource"
                }
            ]
        }
        """;

        // Act
        var permissions = PermissionService.ExtractPermissionsFromAuthorizationClaim(authorizationClaim);

        // Assert
        Assert.Single(permissions);
        Assert.Equal("default-resource", permissions[0].Resource);
        Assert.Equal(string.Empty, permissions[0].Action);
    }

    [Fact]
    public void ExtractPermissionsFromAuthorizationClaim_WithEmptyScopes_ReturnsResourceOnly()
    {
        // Arrange - permission with empty scopes array
        var authorizationClaim = """
        {
            "permissions": [
                {
                    "rsname": "empty-scope-resource",
                    "scopes": []
                }
            ]
        }
        """;

        // Act
        var permissions = PermissionService.ExtractPermissionsFromAuthorizationClaim(authorizationClaim);

        // Assert
        Assert.Single(permissions);
        Assert.Equal("empty-scope-resource", permissions[0].Resource);
        Assert.Equal(string.Empty, permissions[0].Action);
    }

    [Fact]
    public void ExtractPermissionsFromAuthorizationClaim_WithMixedPermissions_ReturnsCorrectFormat()
    {
        // Arrange - mix of scoped and unscoped permissions
        var authorizationClaim = """
        {
            "permissions": [
                {
                    "rsname": "scoped-resource",
                    "scopes": ["action1"]
                },
                {
                    "rsname": "unscoped-resource"
                }
            ]
        }
        """;

        // Act
        var permissions = PermissionService.ExtractPermissionsFromAuthorizationClaim(authorizationClaim);

        // Assert
        Assert.Equal(2, permissions.Count);
        Assert.Contains(permissions, p => p.Resource == "scoped-resource" && p.Action == "action1");
        Assert.Contains(permissions, p => p.Resource == "unscoped-resource" && p.Action == string.Empty);
    }

    [Fact]
    public void ExtractPermissionsFromAuthorizationClaim_WithEmptyPermissionsArray_ReturnsEmptyList()
    {
        // Arrange
        var authorizationClaim = """
        {
            "permissions": []
        }
        """;

        // Act
        var permissions = PermissionService.ExtractPermissionsFromAuthorizationClaim(authorizationClaim);

        // Assert
        Assert.Empty(permissions);
    }

    [Fact]
    public void ExtractPermissionsFromAuthorizationClaim_WithNoPermissionsProperty_ReturnsEmptyList()
    {
        // Arrange
        var authorizationClaim = """
        {
            "other_property": "value"
        }
        """;

        // Act
        var permissions = PermissionService.ExtractPermissionsFromAuthorizationClaim(authorizationClaim);

        // Assert
        Assert.Empty(permissions);
    }

    [Fact]
    public void ExtractPermissionsFromAuthorizationClaim_WithInvalidJson_ReturnsEmptyList()
    {
        // Arrange
        var authorizationClaim = "not valid json";

        // Act
        var permissions = PermissionService.ExtractPermissionsFromAuthorizationClaim(authorizationClaim);

        // Assert
        Assert.Empty(permissions);
    }

    [Fact]
    public void ExtractPermissionsFromAuthorizationClaim_WithNullScopeName_SkipsNullScope()
    {
        // Arrange - scope with null value (edge case)
        var authorizationClaim = """
        {
            "permissions": [
                {
                    "rsname": "test-resource",
                    "scopes": ["valid-scope", null]
                }
            ]
        }
        """;

        // Act
        var permissions = PermissionService.ExtractPermissionsFromAuthorizationClaim(authorizationClaim);

        // Assert
        Assert.Single(permissions);
        Assert.Equal("test-resource", permissions[0].Resource);
        Assert.Equal("valid-scope", permissions[0].Action);
    }
}
