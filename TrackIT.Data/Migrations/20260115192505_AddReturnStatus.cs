using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackIT.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReturnStatus",
                table: "AssignmentHistories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnStatus",
                table: "AssignmentHistories");
        }
    }
}
