-- ============================================================
-- REGISTRATION MODULE PERMISSIONS SEED
-- ============================================================

DECLARE @StudentModuleId BIGINT = (SELECT Id FROM dbo.Modules WHERE [Key] = 'student');

-- Registration permissions
INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'create', 'registration.create', 'Create registration'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'registration.create');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'read', 'registration.read', 'View registration'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'registration.read');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'update', 'registration.update', 'Update registration'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'registration.update');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'delete', 'registration.delete', 'Delete registration'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'registration.delete');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'verify', 'registration.verify', 'Verify registration documents / review'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'registration.verify');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'approve', 'registration.approve', 'Approve or reject registration'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'registration.approve');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'convert', 'registration.convert', 'Convert registration to admission'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'registration.convert');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'export', 'registration.export', 'Export registrations'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'registration.export');

-- Assign to roles
INSERT INTO dbo.RolePermissions (RoleId, PermissionId, CreatedAt, IsDeleted)
SELECT 1, Id, GETUTCDATE(), 0 FROM dbo.Permissions WHERE [Key] LIKE 'registration.%'
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = 1 AND PermissionId = Permissions.Id);

INSERT INTO dbo.RolePermissions (RoleId, PermissionId, CreatedAt, IsDeleted)
SELECT 2, Id, GETUTCDATE(), 0 FROM dbo.Permissions WHERE [Key] LIKE 'registration.%'
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = 2 AND PermissionId = Permissions.Id);

INSERT INTO dbo.RolePermissions (RoleId, PermissionId, CreatedAt, IsDeleted)
SELECT 3, Id, GETUTCDATE(), 0 FROM dbo.Permissions WHERE [Key] LIKE 'registration.%'
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = 3 AND PermissionId = Permissions.Id);

PRINT 'Registration permissions seeded.';
GO
