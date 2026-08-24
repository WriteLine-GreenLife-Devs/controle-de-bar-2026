using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleDeBar.Infra.Compartilhado.Orm.Migrations
{
    /// <inheritdoc />
    public partial class Add_Modulo_Pedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TBPedido",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProdutoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomeProduto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrecoPraticado = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBPedido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBPedido_TBConta_ContaId",
                        column: x => x.ContaId,
                        principalTable: "TBConta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBPedido_TBProduto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "TBProduto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBPedido_ContaId",
                table: "TBPedido",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_TBPedido_ProdutoId",
                table: "TBPedido",
                column: "ProdutoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBPedido");
        }
    }
}
