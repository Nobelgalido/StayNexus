using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StayNexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyTheming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeroImageUrl",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryColor",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tagline",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeroImageUrl",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "PrimaryColor",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Tagline",
                table: "Properties");
        }
    }
}
