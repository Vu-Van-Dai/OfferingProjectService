using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateImageBlob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "AppUsers");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "ProductCategories",
                newName: "IconMimeType");

            migrationBuilder.AddColumn<byte[]>(
                name: "AvatarData",
                table: "Shops",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarMimeType",
                table: "Shops",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "ProductImages",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "ImageMimeType",
                table: "ProductImages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "IconData",
                table: "ProductCategories",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "AvatarData",
                table: "AppUsers",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarMimeType",
                table: "AppUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarData",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "AvatarMimeType",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "ImageMimeType",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "IconData",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "AvatarData",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "AvatarMimeType",
                table: "AppUsers");

            migrationBuilder.RenameColumn(
                name: "IconMimeType",
                table: "ProductCategories",
                newName: "ImageUrl");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Shops",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ProductImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "AppUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
