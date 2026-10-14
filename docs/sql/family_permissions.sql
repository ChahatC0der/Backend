-- ============================================================
-- FAMILY MODULE PERMISSIONS SEED
-- ============================================================

DECLARE @StudentModuleId BIGINT = (SELECT Id FROM dbo.Modules WHERE [Key] = 'student');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'create', 'family.create', 'Create family'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'family.create');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'read', 'family.read', 'View family'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'family.read');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'update', 'family.update', 'Update family'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'family.update');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'delete', 'family.delete', 'Delete family'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'family.delete');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'export', 'family.export', 'Export families'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'family.export');

INSERT INTO dbo.RolePermissions (RoleId, PermissionId, CreatedAt, IsDeleted)
SELECT 1, Id, GETUTCDATE(), 0 FROM dbo.Permissions WHERE [Key] LIKE 'family.%'
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = 1 AND PermissionId = Permissions.Id);

INSERT INTO dbo.RolePermissions (RoleId, PermissionId, CreatedAt, IsDeleted)
SELECT 2, Id, GETUTCDATE(), 0 FROM dbo.Permissions WHERE [Key] LIKE 'family.%'
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = 2 AND PermissionId = Permissions.Id);

INSERT INTO dbo.RolePermissions (RoleId, PermissionId, CreatedAt, IsDeleted)
SELECT 3, Id, GETUTCDATE(), 0 FROM dbo.Permissions WHERE [Key] LIKE 'family.%'
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = 3 AND PermissionId = Permissions.Id);

PRINT 'Family permissions seeded.';
GO
