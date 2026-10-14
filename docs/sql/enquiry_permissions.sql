-- ============================================================
-- ENQUIRY MODULE PERMISSIONS SEED
-- ============================================================

-- Insert permissions (module = role? ya apna module?)
-- Assumption: Module "student" (Id from Modules table) use kar rahe hain
-- Adjust ModuleId as per your seed.

DECLARE @StudentModuleId BIGINT = (SELECT Id FROM dbo.Modules WHERE [Key] = 'student');

-- Enquiry permissions
INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'create', 'enquiry.create', 'Create enquiry'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'enquiry.create');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'read', 'enquiry.read', 'View enquiry'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'enquiry.read');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'update', 'enquiry.update', 'Update enquiry'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'enquiry.update');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'delete', 'enquiry.delete', 'Delete enquiry'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'enquiry.delete');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'assign', 'enquiry.assign', 'Assign enquiry to counselor'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'enquiry.assign');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'convert', 'enquiry.convert', 'Convert enquiry to registration'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'enquiry.convert');

INSERT INTO dbo.Permissions (ModuleId, Action, [Key], Description)
SELECT @StudentModuleId, 'export', 'enquiry.export', 'Export enquiries'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE [Key] = 'enquiry.export');

-- Assign to Super Admin (RoleId = 1)
INSERT INTO dbo.RolePermissions (RoleId, PermissionId, CreatedAt, IsDeleted)
SELECT 1, Id, GETUTCDATE(), 0 FROM dbo.Permissions
WHERE [Key] LIKE 'enquiry.%'
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = 1 AND PermissionId = Permissions.Id);

-- Assign to Organization Admin (RoleId = 2)
INSERT INTO dbo.RolePermissions (RoleId, PermissionId, CreatedAt, IsDeleted)
SELECT 2, Id, GETUTCDATE(), 0 FROM dbo.Permissions
WHERE [Key] LIKE 'enquiry.%'
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = 2 AND PermissionId = Permissions.Id);

-- Assign to Branch Admin (RoleId = 3)
INSERT INTO dbo.RolePermissions (RoleId, PermissionId, CreatedAt, IsDeleted)
SELECT 3, Id, GETUTCDATE(), 0 FROM dbo.Permissions
WHERE [Key] LIKE 'enquiry.%'
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = 3 AND PermissionId = Permissions.Id);

PRINT 'Enquiry permissions seeded.';
GO
