using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TSI.OCR.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedImageField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "Fields",
                type: "BLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Fields");
        }
    }
}
