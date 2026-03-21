using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
namespace DataAccess
{
    public class LctmsDbContext : DbContext
    {
        public LctmsDbContext()
        {
        }

        public LctmsDbContext(DbContextOptions<LctmsDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Admin> Admins { get; set; } = null!;
        public virtual DbSet<Teacher> Teachers { get; set; } = null!;
        public virtual DbSet<Student> Students { get; set; } = null!;
        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<Class> Classes { get; set; } = null!;
        public virtual DbSet<Enrollment> Enrollments { get; set; } = null!;
        public virtual DbSet<Slot> Slots { get; set; } = null!;
        public virtual DbSet<Schedule> Schedules { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfiguration config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                string? connectionString = config.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureAdmin(modelBuilder);
            ConfigureTeacher(modelBuilder);
            ConfigureStudent(modelBuilder);
            ConfigureCourse(modelBuilder);
            ConfigureClass(modelBuilder);
            ConfigureEnrollment(modelBuilder);
            ConfigureSlot(modelBuilder);
            ConfigureSchedule(modelBuilder);

            modelBuilder.Entity<Schedule>()
            .HasOne(s => s.Class)
            .WithMany(c => c.Schedules)
            .HasForeignKey(s => s.ClassId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(
                "Server=.;Database=LCTMS;Trusted_Connection=True;TrustServerCertificate=True;");


        private static void ConfigureAdmin(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.ToTable("Admin");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.FullName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime2(0)")
                    .HasDefaultValueSql("SYSDATETIME()");

                entity.HasIndex(e => e.Username)
                    .IsUnique();

                entity.HasIndex(e => e.Email)
                    .IsUnique();
            });
        }

        private static void ConfigureTeacher(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.ToTable("Teacher");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.TeacherCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.FullName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20);

                entity.Property(e => e.DateOfBirth)
                    .HasColumnType("date");

                entity.Property(e => e.Gender)
                    .HasMaxLength(10);

                entity.Property(e => e.Address)
                    .HasMaxLength(150);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime2(0)")
                    .HasDefaultValueSql("SYSDATETIME()");

                entity.HasIndex(e => e.Username)
                    .IsUnique();

                entity.HasIndex(e => e.TeacherCode)
                    .IsUnique();

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.HasCheckConstraint("CK_Teacher_Gender",
                    "[Gender] IS NULL OR [Gender] IN (N'Male', N'Female', N'Other')");
            });
        }

        private static void ConfigureStudent(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("Student");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.StudentCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.FullName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20);

                entity.Property(e => e.DateOfBirth)
                    .HasColumnType("date");

                entity.Property(e => e.Gender)
                    .HasMaxLength(10);

                entity.Property(e => e.Address)
                    .HasMaxLength(150);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime2(0)")
                    .HasDefaultValueSql("SYSDATETIME()");

                entity.HasIndex(e => e.Username)
                    .IsUnique();

                entity.HasIndex(e => e.StudentCode)
                    .IsUnique();

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.HasCheckConstraint("CK_Student_Gender",
                    "[Gender] IS NULL OR [Gender] IN (N'Male', N'Female', N'Other')");
            });
        }

        private static void ConfigureCourse(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Course");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.CourseCode)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.SubjectCourse)
                    .HasMaxLength(50);

                entity.Property(e => e.DurationWeeks)
                    .IsRequired();

                entity.Property(e => e.Fee)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime2(0)")
                    .HasDefaultValueSql("SYSDATETIME()");

                entity.HasIndex(e => e.CourseCode)
                    .IsUnique();

                entity.HasCheckConstraint("CK_Course_DurationWeeks", "[DurationWeeks] > 0");
                entity.HasCheckConstraint("CK_Course_Fee", "[Fee] >= 0");
                entity.HasCheckConstraint("CK_Course_Status",
                    "[Status] IN (N'Open', N'Closed')");
            });
        }

        private static void ConfigureClass(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Class>(entity =>
            {
                entity.ToTable("Class");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ClassCode)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.StartDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(e => e.EndDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(e => e.Capacity)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime2(0)")
                    .HasDefaultValueSql("SYSDATETIME()");

                entity.HasIndex(e => e.ClassCode)
                    .IsUnique();

                entity.HasIndex(e => e.CourseId);
                entity.HasIndex(e => e.TeacherId);

                entity.HasOne<Course>()
                    .WithMany()
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Class_Course");

                entity.HasOne<Teacher>()
                    .WithMany()
                    .HasForeignKey(e => e.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Class_Teacher");

                entity.HasCheckConstraint("CK_Class_Date", "[EndDate] >= [StartDate]");
                entity.HasCheckConstraint("CK_Class_Capacity", "[Capacity] > 0");
                entity.HasCheckConstraint("CK_Class_Status",
                    "[Status] IN (N'Open', N'Full', N'Closed')");

                entity.HasCheckConstraint("CK_Enrollment_Status",
    "[Status] IN (N'Pending', N'Approved', N'Rejected', N'Cancel')");
            });
        }

        private static void ConfigureEnrollment(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.ToTable("Enrollment");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.RegisteredAt)
                    .HasColumnType("datetime2(0)")
                    .HasDefaultValueSql("SYSDATETIME()");

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime2(0)")
                    .HasDefaultValueSql("SYSDATETIME()");

                entity.HasIndex(e => e.StudentId);
                entity.HasIndex(e => e.ClassId);
                entity.HasIndex(e => e.Status);

                entity.HasIndex(e => new { e.StudentId, e.ClassId })
                    .IsUnique();

                entity.HasOne<Student>()
                    .WithMany()
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Enrollment_Student");

                entity.HasOne<Class>()
                    .WithMany()
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Enrollment_Class");

                entity.HasCheckConstraint("CK_Enrollment_Status",
                    "[Status] IN (N'Pending', N'Approved', N'Rejected', N'Rejected')");
            });
        }

        private static void ConfigureSlot(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Slot>(entity =>
            {
                entity.ToTable("Slot");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.SlotName)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.StartTime)
                    .HasColumnType("time(0)")
                    .IsRequired();

                entity.Property(e => e.EndTime)
                    .HasColumnType("time(0)")
                    .IsRequired();

                entity.HasIndex(e => e.SlotName)
                    .IsUnique();

                entity.HasCheckConstraint("CK_Slot_Time", "[EndTime] > [StartTime]");
            });
        }

        private static void ConfigureSchedule(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.ToTable("Schedule");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.DayOfWeek)
                    .IsRequired();

                entity.Property(e => e.RoomName)
                    .HasMaxLength(50);

                entity.HasIndex(e => e.ClassId);
                entity.HasIndex(e => e.SlotId);

                entity.HasIndex(e => new { e.ClassId, e.DayOfWeek, e.SlotId })
                    .IsUnique();

                entity.HasOne<Class>()
                    .WithMany()
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Schedule_Class");

                entity.HasOne<Slot>()
                    .WithMany()
                    .HasForeignKey(e => e.SlotId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Schedule_Slot");

                entity.HasCheckConstraint("CK_Schedule_DayOfWeek",
                    "[DayOfWeek] BETWEEN 1 AND 7");
            });
        }
    }
}