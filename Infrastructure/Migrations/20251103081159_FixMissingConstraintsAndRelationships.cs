using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixMissingConstraintsAndRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_Products_ProductId",
                table: "ProductImage");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductReviews_AppUsers_UserId",
                table: "ProductReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductReviews_ProductReviews_ProductReviewId",
                table: "ProductReviews");

            migrationBuilder.DropIndex(
                name: "IX_UserAddresses_UserId_IsDefault",
                table: "UserAddresses");

            migrationBuilder.DropIndex(
                name: "IX_ProductReviews_ProductReviewId",
                table: "ProductReviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductImage",
                table: "ProductImage");

            migrationBuilder.DropColumn(
                name: "ProductReviewId",
                table: "ProductReviews");

            migrationBuilder.DropColumn(
                name: "Specifications",
                table: "ProductReviews");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "AppUsers");

            migrationBuilder.RenameTable(
                name: "ProductImage",
                newName: "ProductImages");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImage_ProductId",
                table: "ProductImages",
                newName: "IX_ProductImages_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductImages",
                table: "ProductImages",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_UserId_IsDefault",
                table: "UserAddresses",
                columns: new[] { "UserId", "IsDefault" },
                unique: true,
                filter: "([IsDefault] = CAST(1 AS bit))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Shop_CommissionRate",
                table: "Shops",
                sql: "[CommissionRate] >= 0 AND [CommissionRate] <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Product_BasePrice",
                table: "Products",
                sql: "[BasePrice] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Product_PriceRange",
                table: "Products",
                sql: "[MaxPrice] IS NULL OR [MaxPrice] >= [BasePrice]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Review_Rating",
                table: "ProductReviews",
                sql: "[Rating] BETWEEN 1 AND 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItem_Price",
                table: "OrderItems",
                sql: "[Price] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItem_Quantity",
                table: "OrderItems",
                sql: "[Quantity] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CartItem_Quantity",
                table: "CartItems",
                sql: "[Quantity] > 0");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_Products_ProductId",
                table: "ProductImages",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductReviews_AppUsers_UserId",
                table: "ProductReviews",
                column: "UserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_Products_ProductId",
                table: "ProductImages");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductReviews_AppUsers_UserId",
                table: "ProductReviews");

            migrationBuilder.DropIndex(
                name: "IX_UserAddresses_UserId_IsDefault",
                table: "UserAddresses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Shop_CommissionRate",
                table: "Shops");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Product_BasePrice",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Product_PriceRange",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Review_Rating",
                table: "ProductReviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItem_Price",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItem_Quantity",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CartItem_Quantity",
                table: "CartItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductImages",
                table: "ProductImages");

            migrationBuilder.RenameTable(
                name: "ProductImages",
                newName: "ProductImage");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImage",
                newName: "IX_ProductImage_ProductId");

            migrationBuilder.AddColumn<int>(
                name: "ProductReviewId",
                table: "ProductReviews",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Specifications",
                table: "ProductReviews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShopId",
                table: "AppUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductImage",
                table: "ProductImage",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_UserId_IsDefault",
                table: "UserAddresses",
                columns: new[] { "UserId", "IsDefault" },
                unique: true,
                filter: "[IsDefault] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_ProductReviewId",
                table: "ProductReviews",
                column: "ProductReviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_Products_ProductId",
                table: "ProductImage",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductReviews_AppUsers_UserId",
                table: "ProductReviews",
                column: "UserId",
                principalTable: "AppUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductReviews_ProductReviews_ProductReviewId",
                table: "ProductReviews",
                column: "ProductReviewId",
                principalTable: "ProductReviews",
                principalColumn: "Id");
        }
    }
}
