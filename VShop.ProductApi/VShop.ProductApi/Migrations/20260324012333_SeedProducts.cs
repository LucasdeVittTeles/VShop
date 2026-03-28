using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VShop.ProductApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO Products(Name, Price, Description, Stock, ImageUrl, CategoryId) " +
                "Values('Caderno', 7.55, 'Caderno', 10, 'caderno1.jpg', 1)");

            migrationBuilder.Sql("INSERT INTO Products(Name, Price, Description, Stock, ImageUrl, CategoryId) " +
                "Values('Lápis', 3.45, 'Lápis Preto', 20, 'lapis1.jpg', 1)");

            migrationBuilder.Sql("INSERT INTO Products(Name, Price, Description, Stock, ImageUrl, CategoryId) " +
                "Values('Clips', 5.33, 'Clips para papel', 50, 'clips1.jpg', 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Products");
        }
    }
}
