using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class EnrollmentDao
    {
        public List<EnrollmentApprovalItem> GetRegistrationList(string? statusFilter = null, string? keyword = null)
        {
            using var context = DbContextFactory.CreateDbContext();

            var rawData =
                (from e in context.Enrollments
                 join s in context.Students on e.StudentId equals s.Id
                 join c in context.Classes on e.ClassId equals c.Id
                 select new EnrollmentApprovalItem
                 {
                     EnrollmentId = e.Id,
                     StudentId = s.Id,
                     StudentCode = s.StudentCode,
                     StudentName = s.FullName,
                     StudentEmail = s.Email,
                     ClassId = c.Id,
                     ClassCode = c.ClassCode,
                     Capacity = c.Capacity,
                     ApprovedCount = context.Enrollments.Count(x => x.ClassId == c.Id && x.Status == "Approved"),
                     StartDate = c.StartDate,
                     RegisteredAt = e.RegisteredAt,
                     EnrollmentStatus = e.Status,
                     ClassStatus = c.Status
                 }).ToList();

            foreach (var item in rawData)
            {
                bool classStarted = DateTime.Today >= item.StartDate.Date;
                bool classOpen = item.ClassStatus == "Open";
                bool classFullByApproved = item.ApprovedCount >= item.Capacity;

                item.CanApprove = item.EnrollmentStatus == "Pending"
                                  && !classStarted
                                  && classOpen
                                  && !classFullByApproved;

                item.CanReject = item.EnrollmentStatus == "Pending" && !classStarted;

                if (item.EnrollmentStatus == "Cancel")
                {
                    item.CanApprove = false;
                    item.CanReject = false;
                }

                item.ShowFinalStatus = !item.CanApprove && !item.CanReject;
                item.FinalStatusText = item.ShowFinalStatus ? item.EnrollmentStatus : string.Empty;
            }

            IEnumerable<EnrollmentApprovalItem> query = rawData;

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                query = query.Where(x => x.EnrollmentStatus == statusFilter);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();

                query = query.Where(x =>
                    x.StudentName.ToLower().Contains(keyword) ||
                    x.StudentCode.ToLower().Contains(keyword) ||
                    x.StudentEmail.ToLower().Contains(keyword) ||
                    x.ClassCode.ToLower().Contains(keyword));
            }

            return query.OrderBy(x => x.EnrollmentId).ToList();
        }

        public OperationResult ApproveEnrollment(int enrollmentId)
        {
            using var context = DbContextFactory.CreateDbContext();
            using var transaction = context.Database.BeginTransaction();

            try
            {
                var enrollment = context.Enrollments.FirstOrDefault(e => e.Id == enrollmentId);
                if (enrollment == null)
                    return OperationResult.Failure("Không tìm thấy đơn đăng ký.");

                if (enrollment.Status != "Pending")
                    return OperationResult.Failure("Chỉ được duyệt đơn đang ở trạng thái Pending.");

                var trainingClass = context.Classes.FirstOrDefault(c => c.Id == enrollment.ClassId);
                if (trainingClass == null)
                    return OperationResult.Failure("Không tìm thấy lớp học.");

                var course = context.Courses.FirstOrDefault(c => c.Id == trainingClass.CourseId);
                if (course == null)
                    return OperationResult.Failure("Không tìm thấy khóa học.");

                if (course.Status != "Open")
                    return OperationResult.Failure("Khóa học đã đóng, không thể duyệt đăng ký.");

                if (trainingClass.Status != "Open")
                    return OperationResult.Failure("Chỉ được duyệt khi lớp đang ở trạng thái Open.");

                if (DateTime.Today >= trainingClass.StartDate.Date)
                    return OperationResult.Failure("Lớp đã bắt đầu học, không thể duyệt.");

                int approvedCount = context.Enrollments.Count(e =>
                    e.ClassId == trainingClass.Id && e.Status == "Approved");

                if (approvedCount >= trainingClass.Capacity)
                {
                    trainingClass.Status = "Full";
                    context.SaveChanges();
                    transaction.Commit();
                    return OperationResult.Failure("Lớp đã đủ chỗ tại thời điểm duyệt.");
                }

                bool sameCourseConflict =
                    (from e in context.Enrollments
                     join c in context.Classes on e.ClassId equals c.Id
                     where e.StudentId == enrollment.StudentId
                           && e.Id != enrollment.Id
                           && (e.Status == "Pending" || e.Status == "Approved")
                           && c.CourseId == trainingClass.CourseId
                     select e.Id).Any();

                if (sameCourseConflict)
                    return OperationResult.Failure("Học viên đang có lớp Pending/Approved khác của cùng khóa học.");

                var targetSchedules = context.Schedules
                    .Where(x => x.ClassId == trainingClass.Id)
                    .Select(x => new { x.DayOfWeek, x.SlotId })
                    .AsEnumerable()
                    .Select(x => ((int)x.DayOfWeek, x.SlotId))
                    .ToHashSet();

                if (targetSchedules.Count == 0)
                    return OperationResult.Failure("Lớp chưa có lịch học hợp lệ.");

                var approvedClassesOfStudent =
                    (from e in context.Enrollments
                     join c in context.Classes on e.ClassId equals c.Id
                     where e.StudentId == enrollment.StudentId
                           && e.Id != enrollment.Id
                           && e.Status == "Approved"
                           && c.StartDate <= trainingClass.EndDate
                           && c.EndDate >= trainingClass.StartDate
                     select new { c.Id, c.ClassCode })
                     .AsEnumerable()
                     .ToList();

                string? conflictClassCode = null;

                foreach (var item in approvedClassesOfStudent)
                {
                    var existingSchedules = context.Schedules
                        .Where(s => s.ClassId == item.Id)
                        .Select(s => new { s.DayOfWeek, s.SlotId })
                        .AsEnumerable()
                        .Select(s => (s.DayOfWeek, s.SlotId));

                    if (existingSchedules.Any(s => targetSchedules.Contains(s)))
                    {
                        conflictClassCode = item.ClassCode;
                        break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(conflictClassCode))
                    return OperationResult.Failure($"Học viên bị trùng lịch với lớp đã Approved: {conflictClassCode}.");

                enrollment.Status = "Approved";
                context.SaveChanges();

                approvedCount = context.Enrollments.Count(e =>
                    e.ClassId == trainingClass.Id && e.Status == "Approved");

                trainingClass.Status = approvedCount >= trainingClass.Capacity ? "Full" : "Open";
                context.SaveChanges();

                transaction.Commit();
                return OperationResult.Success("Duyệt đăng ký thành công.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return OperationResult.Failure("Lỗi khi duyệt đăng ký: " + ex.Message);
            }
        }

        public OperationResult RejectEnrollment(int enrollmentId)
        {
            using var context = DbContextFactory.CreateDbContext();
            using var transaction = context.Database.BeginTransaction();

            try
            {
                var enrollment = context.Enrollments.FirstOrDefault(e => e.Id == enrollmentId);
                if (enrollment == null)
                    return OperationResult.Failure("Không tìm thấy đơn đăng ký.");

                var trainingClass = context.Classes.FirstOrDefault(c => c.Id == enrollment.ClassId);
                if (trainingClass == null)
                    return OperationResult.Failure("Không tìm thấy lớp học.");

                if (DateTime.Today >= trainingClass.StartDate.Date)
                    return OperationResult.Failure("Lớp đã bắt đầu học, không thể đổi trạng thái.");

                if (enrollment.Status == "Cancel")
                    return OperationResult.Failure("Đơn đã bị sinh viên hủy, admin không thể tác động.");

                if (enrollment.Status == "Rejected")
                    return OperationResult.Failure("Đơn này đã ở trạng thái Rejected.");

                bool wasApproved = enrollment.Status == "Approved";

                enrollment.Status = "Rejected";
                context.SaveChanges();

                if (trainingClass.Status != "Closed")
                {
                    int approvedCount = context.Enrollments.Count(e =>
                        e.ClassId == trainingClass.Id && e.Status == "Approved");

                    trainingClass.Status = approvedCount >= trainingClass.Capacity ? "Full" : "Open";
                    context.SaveChanges();
                }

                transaction.Commit();
                return OperationResult.Success(wasApproved
                    ? "Chuyển từ Approved sang Rejected thành công."
                    : "Từ chối đăng ký thành công.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return OperationResult.Failure("Lỗi khi từ chối đăng ký: " + ex.Message);
            }
        }
    }
}