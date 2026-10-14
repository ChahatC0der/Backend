-- ============================================================
-- ALUMNI MODULE PERMISSIONS SEED
-- ============================================================

DECLARE @StudentModuleId BIGINT = (SELECT Id FROM dbo.Modules WHERE [Key] = 'student');

-- Alumni permissions
INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'create', 'alumni.create', 'Create alumni'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'alumni.create');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'read', 'alumni.read', 'View alumni'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'alumni.read');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'update', 'alumni.update', 'Update alumni'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'alumni.update');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'delete', 'alumni.delete', 'Delete alumni'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'alumni.delete');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'convert', 'alumni.convert', 'Convert student to alumni'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'alumni.convert');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'export', 'alumni.export', 'Export alumni'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'alumni.export');

-- Assign to Super Admin
INSERT INTO dbo.RolePermissions (RoleId, PermissionId, CreatedAt, IsDeleted)
SELECT 1, Id, GETUTCDATE(), 0 FROM dbo.Permissions WHERE [Key] LIKE 'alumni.%'
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = 1 AND PermissionId = Permissions.Id);

-- Assign to Organization Admin
INSERT INTO dbo.RolePermissions (RoleId, PermissionId, CreatedAt, IsDeleted)
SELECT 2, Id, GETUTCDATE(), 0 FROM dbo.Permissions WHERE [Key] LIKE 'alumni.%'
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = 2 AND PermissionId = Permissions.Id);

-- Assign to Branch Admin
INSERT INTO dbo.RolePermissions (RoleId, PermissionId, CreatedAt, IsDeleted)
SELECT 3, Id, GETUTCDATE(), 0 FROM dbo.Permissions WHERE [Key] LIKE 'alumni.%'
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = 3 AND PermissionId = Permissions.Id);

PRINT 'Alumni permissions seeded.';
GO
