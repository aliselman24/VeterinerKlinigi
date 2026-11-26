using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VeterinerKlinigi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clinics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clinics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Species = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Breed = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    WeightKg = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pets", x => x.Id);
                    table.CheckConstraint("CK_Pets_Weight", " CAST(\"WeightKg\" AS NUMERIC) > 0 ");
                    table.ForeignKey(
                        name: "FK_Pets_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Specialties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClinicServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClinicId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Fee = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicServices", x => x.Id);
                    table.UniqueConstraint("AK_ClinicServices_Id_ClinicId", x => new { x.Id, x.ClinicId });
                    table.CheckConstraint("CK_ClinicServices_Duration", " \"DurationMinutes\" BETWEEN 1 AND 1440 ");
                    table.CheckConstraint("CK_ClinicServices_Fee", " CAST(\"Fee\" AS NUMERIC) >= 0 ");
                    table.ForeignKey(
                        name: "FK_ClinicServices_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicWorkingHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClinicId = table.Column<int>(type: "INTEGER", nullable: false),
                    DayOfWeek = table.Column<int>(type: "INTEGER", nullable: false),
                    OpensAt = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    ClosesAt = table.Column<TimeOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicWorkingHours", x => x.Id);
                    table.CheckConstraint("CK_ClinicWorkingHours_Day", " \"DayOfWeek\" BETWEEN 0 AND 6 ");
                    table.CheckConstraint("CK_ClinicWorkingHours_Interval", " \"OpensAt\" < \"ClosesAt\" ");
                    table.ForeignKey(
                        name: "FK_ClinicWorkingHours_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Veterinarians",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClinicId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Veterinarians", x => x.Id);
                    table.UniqueConstraint("AK_Veterinarians_Id_ClinicId", x => new { x.Id, x.ClinicId });
                    table.ForeignKey(
                        name: "FK_Veterinarians_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Veterinarians_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VaccinationRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PetId = table.Column<int>(type: "INTEGER", nullable: false),
                    VaccineName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    AdministeredOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    NextDoseOn = table.Column<DateOnly>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaccinationRecords", x => x.Id);
                    table.CheckConstraint("CK_VaccinationRecords_NextDose", " \"NextDoseOn\" IS NULL OR \"NextDoseOn\" >= \"AdministeredOn\" ");
                    table.ForeignKey(
                        name: "FK_VaccinationRecords_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VeterinarianAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VeterinarianId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    StartsAt = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    EndsAt = table.Column<TimeOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VeterinarianAvailabilities", x => x.Id);
                    table.CheckConstraint("CK_VeterinarianAvailabilities_Interval", " \"StartsAt\" < \"EndsAt\" ");
                    table.ForeignKey(
                        name: "FK_VeterinarianAvailabilities_Veterinarians_VeterinarianId",
                        column: x => x.VeterinarianId,
                        principalTable: "Veterinarians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VeterinarianServices",
                columns: table => new
                {
                    VeterinarianId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClinicServiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClinicId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VeterinarianServices", x => new { x.VeterinarianId, x.ClinicServiceId });
                    table.ForeignKey(
                        name: "FK_VeterinarianServices_ClinicServices_ClinicServiceId_ClinicId",
                        columns: x => new { x.ClinicServiceId, x.ClinicId },
                        principalTable: "ClinicServices",
                        principalColumns: new[] { "Id", "ClinicId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VeterinarianServices_Veterinarians_VeterinarianId_ClinicId",
                        columns: x => new { x.VeterinarianId, x.ClinicId },
                        principalTable: "Veterinarians",
                        principalColumns: new[] { "Id", "ClinicId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VeterinarianSpecialties",
                columns: table => new
                {
                    SpecialtiesId = table.Column<int>(type: "INTEGER", nullable: false),
                    VeterinariansId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VeterinarianSpecialties", x => new { x.SpecialtiesId, x.VeterinariansId });
                    table.ForeignKey(
                        name: "FK_VeterinarianSpecialties_Specialties_SpecialtiesId",
                        column: x => x.SpecialtiesId,
                        principalTable: "Specialties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VeterinarianSpecialties_Veterinarians_VeterinariansId",
                        column: x => x.VeterinariansId,
                        principalTable: "Veterinarians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PetId = table.Column<int>(type: "INTEGER", nullable: false),
                    VeterinarianId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClinicServiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartsAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Fee = table.Column<decimal>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.CheckConstraint("CK_Appointments_Duration", " \"DurationMinutes\" BETWEEN 1 AND 1440 ");
                    table.CheckConstraint("CK_Appointments_Fee", " CAST(\"Fee\" AS NUMERIC) >= 0 ");
                    table.CheckConstraint("CK_Appointments_Status", " \"Status\" BETWEEN 0 AND 3 ");
                    table.ForeignKey(
                        name: "FK_Appointments_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_VeterinarianServices_VeterinarianId_ClinicServiceId",
                        columns: x => new { x.VeterinarianId, x.ClinicServiceId },
                        principalTable: "VeterinarianServices",
                        principalColumns: new[] { "VeterinarianId", "ClinicServiceId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExaminationNotes",
                columns: table => new
                {
                    AppointmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExaminationNotes", x => x.AppointmentId);
                    table.ForeignKey(
                        name: "FK_ExaminationNotes_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PetId",
                table: "Appointments",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_VeterinarianId_ClinicServiceId",
                table: "Appointments",
                columns: new[] { "VeterinarianId", "ClinicServiceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_VeterinarianId_StartsAtUtc",
                table: "Appointments",
                columns: new[] { "VeterinarianId", "StartsAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicServices_ClinicId",
                table: "ClinicServices",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicWorkingHours_ClinicId_DayOfWeek_OpensAt",
                table: "ClinicWorkingHours",
                columns: new[] { "ClinicId", "DayOfWeek", "OpensAt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pets_OwnerId",
                table: "Pets",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Specialties_Name",
                table: "Specialties",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecords_PetId_NextDoseOn",
                table: "VaccinationRecords",
                columns: new[] { "PetId", "NextDoseOn" });

            migrationBuilder.CreateIndex(
                name: "IX_VeterinarianAvailabilities_VeterinarianId_Date_StartsAt",
                table: "VeterinarianAvailabilities",
                columns: new[] { "VeterinarianId", "Date", "StartsAt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Veterinarians_ClinicId",
                table: "Veterinarians",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Veterinarians_UserId",
                table: "Veterinarians",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VeterinarianServices_ClinicServiceId_ClinicId",
                table: "VeterinarianServices",
                columns: new[] { "ClinicServiceId", "ClinicId" });

            migrationBuilder.CreateIndex(
                name: "IX_VeterinarianServices_VeterinarianId_ClinicId",
                table: "VeterinarianServices",
                columns: new[] { "VeterinarianId", "ClinicId" });

            migrationBuilder.CreateIndex(
                name: "IX_VeterinarianSpecialties_VeterinariansId",
                table: "VeterinarianSpecialties",
                column: "VeterinariansId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicWorkingHours");

            migrationBuilder.DropTable(
                name: "ExaminationNotes");

            migrationBuilder.DropTable(
                name: "VaccinationRecords");

            migrationBuilder.DropTable(
                name: "VeterinarianAvailabilities");

            migrationBuilder.DropTable(
                name: "VeterinarianSpecialties");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Specialties");

            migrationBuilder.DropTable(
                name: "Pets");

            migrationBuilder.DropTable(
                name: "VeterinarianServices");

            migrationBuilder.DropTable(
                name: "ClinicServices");

            migrationBuilder.DropTable(
                name: "Veterinarians");

            migrationBuilder.DropTable(
                name: "Clinics");
        }
    }
}
