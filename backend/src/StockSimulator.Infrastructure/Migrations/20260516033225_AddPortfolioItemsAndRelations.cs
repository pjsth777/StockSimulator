using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockSimulator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioItemsAndRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AverageBuyPrice",
                table: "PortfolioItems",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageBuyPrice",
                table: "PortfolioItems");
        }
    }
}
