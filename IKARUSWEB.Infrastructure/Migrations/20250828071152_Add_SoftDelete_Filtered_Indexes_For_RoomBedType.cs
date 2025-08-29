using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IKARUSWEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_SoftDelete_Filtered_Indexes_For_RoomBedType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomBedTypes_Tenants_TenantId",
                table: "RoomBedTypes");

            migrationBuilder.DropIndex(
                name: "IX_RoomBedTypes_TenantId_Code",
                table: "RoomBedTypes");

            migrationBuilder.DropIndex(
                name: "IX_RoomBedTypes_TenantId_Name",
                table: "RoomBedTypes");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "RoomBedTypes",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomBedTypes_TenantId_Code",
                table: "RoomBedTypes",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RoomBedTypes_TenantId_Name",
                table: "RoomBedTypes",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoomBedTypes_TenantId_Code",
                table: "RoomBedTypes");

            migrationBuilder.DropIndex(
                name: "IX_RoomBedTypes_TenantId_Name",
                table: "RoomBedTypes");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "RoomBedTypes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomBedTypes_TenantId_Code",
                table: "RoomBedTypes",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RoomBedTypes_TenantId_Name",
                table: "RoomBedTypes",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomBedTypes_Tenants_TenantId",
                table: "RoomBedTypes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
