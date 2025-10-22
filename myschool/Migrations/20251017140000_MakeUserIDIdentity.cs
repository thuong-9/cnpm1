using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myschool.Migrations
{
    public partial class MakeUserIDIdentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Recreate AdminUser with IDENTITY on UserID while preserving existing rows
            var sql = @"
IF OBJECT_ID('dbo.AdminUser_new', 'U') IS NOT NULL
    DROP TABLE dbo.AdminUser_new;

CREATE TABLE dbo.AdminUser_new (
    UserID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserName nvarchar(max) NULL,
    Email nvarchar(max) NULL,
    Password nvarchar(max) NULL,
    IsActive bit NULL
);

SET IDENTITY_INSERT dbo.AdminUser_new ON;
IF OBJECT_ID('dbo.AdminUser', 'U') IS NOT NULL
BEGIN
    INSERT INTO dbo.AdminUser_new (UserID, UserName, Email, Password, IsActive)
    SELECT UserID, UserName, Email, Password, IsActive FROM dbo.AdminUser;
END
SET IDENTITY_INSERT dbo.AdminUser_new OFF;

IF OBJECT_ID('dbo.AdminUser', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.AdminUser;
END

EXEC sp_rename 'dbo.AdminUser_new', 'AdminUser';
";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse: recreate non-identity table (preserve data)
            var sql = @"
IF OBJECT_ID('dbo.AdminUser_old', 'U') IS NOT NULL
    DROP TABLE dbo.AdminUser_old;

CREATE TABLE dbo.AdminUser_old (
    UserID int NOT NULL PRIMARY KEY,
    UserName nvarchar(max) NULL,
    Email nvarchar(max) NULL,
    Password nvarchar(max) NULL,
    IsActive bit NULL
);

SET IDENTITY_INSERT dbo.AdminUser_old ON;
IF OBJECT_ID('dbo.AdminUser', 'U') IS NOT NULL
BEGIN
    INSERT INTO dbo.AdminUser_old (UserID, UserName, Email, Password, IsActive)
    SELECT UserID, UserName, Email, Password, IsActive FROM dbo.AdminUser;
END
SET IDENTITY_INSERT dbo.AdminUser_old OFF;

IF OBJECT_ID('dbo.AdminUser', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.AdminUser;
END

EXEC sp_rename 'dbo.AdminUser_old', 'AdminUser';
";

            migrationBuilder.Sql(sql);
        }
    }
}
