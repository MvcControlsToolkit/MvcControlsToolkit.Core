using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MvcControlsToolkit.Core.OData.Test.Data.Migrations
{
    public partial class start : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReferenceModels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ABool = table.Column<bool>(nullable: false),
                    ADate = table.Column<DateTime>(nullable: false),
                    ADateTime = table.Column<DateTime>(nullable: false),
                    ADateTimeOffset = table.Column<DateTimeOffset>(nullable: false),
                    ADecimal = table.Column<decimal>(nullable: false),
                    ADouble = table.Column<double>(nullable: false),
                    ADuration = table.Column<TimeSpan>(nullable: false),
                    AFloat = table.Column<float>(nullable: false),
                    AGuid = table.Column<Guid>(nullable: false),
                    AInt = table.Column<int>(nullable: false),
                    ALong = table.Column<long>(nullable: false),
                    AMonth = table.Column<DateTime>(nullable: false),
                    ANBool = table.Column<bool>(nullable: true),
                    ANDate = table.Column<DateTime>(nullable: true),
                    ANDateTime = table.Column<DateTime>(nullable: true),
                    ANDateTimeOffset = table.Column<DateTimeOffset>(nullable: true),
                    ANDecimal = table.Column<decimal>(nullable: true),
                    ANDouble = table.Column<double>(nullable: true),
                    ANDuration = table.Column<TimeSpan>(nullable: true),
                    ANFloat = table.Column<float>(nullable: true),
                    ANGuid = table.Column<Guid>(nullable: true),
                    ANInt = table.Column<int>(nullable: true),
                    ANLong = table.Column<long>(nullable: true),
                    ANMonth = table.Column<DateTime>(nullable: true),
                    ANShort = table.Column<short>(nullable: true),
                    ANTime = table.Column<TimeSpan>(nullable: true),
                    ANWeek = table.Column<DateTime>(nullable: true),
                    AShort = table.Column<short>(nullable: false),
                    AString = table.Column<string>(nullable: true),
                    ATime = table.Column<TimeSpan>(nullable: false),
                    AWeek = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceModels", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReferenceModels");
        }
    }
}
