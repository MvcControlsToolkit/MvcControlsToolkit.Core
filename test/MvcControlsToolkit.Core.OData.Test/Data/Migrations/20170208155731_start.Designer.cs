using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MvcControlsToolkit.Core.OData.Test.Data;

namespace MvcControlsToolkit.Core.OData.Test.Data.Migrations
{
    [DbContext(typeof(TestContext))]
    [Migration("20170208155731_start")]
    partial class start
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MvcControlsToolkit.Core.OData.Test.Models.ReferenceModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("ABool");

                    b.Property<DateTime>("ADate");

                    b.Property<DateTime>("ADateTime");

                    b.Property<DateTimeOffset>("ADateTimeOffset");

                    b.Property<decimal>("ADecimal");

                    b.Property<double>("ADouble");

                    b.Property<TimeSpan>("ADuration");

                    b.Property<float>("AFloat");

                    b.Property<Guid>("AGuid");

                    b.Property<int>("AInt");

                    b.Property<long>("ALong");

                    b.Property<DateTime>("AMonth");

                    b.Property<bool?>("ANBool");

                    b.Property<DateTime?>("ANDate");

                    b.Property<DateTime?>("ANDateTime");

                    b.Property<DateTimeOffset?>("ANDateTimeOffset");

                    b.Property<decimal?>("ANDecimal");

                    b.Property<double?>("ANDouble");

                    b.Property<TimeSpan?>("ANDuration");

                    b.Property<float?>("ANFloat");

                    b.Property<Guid?>("ANGuid");

                    b.Property<int?>("ANInt");

                    b.Property<long?>("ANLong");

                    b.Property<DateTime?>("ANMonth");

                    b.Property<short?>("ANShort");

                    b.Property<TimeSpan?>("ANTime");

                    b.Property<DateTime?>("ANWeek");

                    b.Property<short>("AShort");

                    b.Property<string>("AString");

                    b.Property<TimeSpan>("ATime");

                    b.Property<DateTime>("AWeek");

                    b.HasKey("Id");

                    b.ToTable("ReferenceModels");
                });
        }
    }
}
