using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MvcControlsToolkit.Core.OData.Test.Data.Migrations
{
    public partial class update1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NestedReferenceModels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AInt = table.Column<int>(nullable: false),
                    AString = table.Column<string>(nullable: true),
                    FatherId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NestedReferenceModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NestedReferenceModels_ReferenceModels_FatherId",
                        column: x => x.FatherId,
                        principalTable: "ReferenceModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NestedReferenceModels_FatherId",
                table: "NestedReferenceModels",
                column: "FatherId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NestedReferenceModels");
        }
    }
}
