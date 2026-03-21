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
                bool classClosed = item.ClassStatus == "Closed";
                bool classFull = item.ApprovedCount >= item.Capacity;

                item.CanApprove =
                    (
                        item.EnrollmentStatus == "Pending"
                        && !classStarted
                        && !classClosed
                        && !classFull
                    )
                    ||
                    (
                        item.EnrollmentStatus == "Rejected"
                        && !classStarted
                        && !classClosed
                        && !classFull
                    );

                item.CanReject =
                    (item.EnrollmentStatus == "Pending" || item.EnrollmentStatus == "Approved")
                    && !classStarted;

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

            return query
                .OrderBy(x => x.EnrollmentId)   // sửa theo ID tăng dần
                .ToList();
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

                var trainingClass = context.Classes.FirstOrDefault(c => c.Id == enrollment.ClassId);
                if (trainingClass == null)
                    return OperationResult.Failure("Không tìm thấy lớp học.");

                if (DateTime.Today >= trainingClass.StartDate.Date)
                    return OperationResult.Failure("Lớp đã bắt đầu học, không thể đổi trạng thái.");

                if (trainingClass.Status == "Closed")
                    return OperationResult.Failure("Lớp đã đóng, không thể duyệt.");

                if (enrollment.Status == "Cancel")
                    return OperationResult.Failure("Đơn đã bị sinh viên hủy, admin không thể tác động.");

                if (enrollment.Status == "Approved")
                    return OperationResult.Failure("Đơn này đã được duyệt.");

                int approvedCount = context.Enrollments.Count(e =>
                    e.ClassId == trainingClass.Id && e.Status == "Approved");

                if (approvedCount >= trainingClass.Capacity)
                {
                    // Không auto reject cứng ở đây nữa.
                    // Giữ Rejected nếu không duyệt được do full.
                    enrollment.Status = "Rejected";
                    trainingClass.Status = "Full";
                    context.SaveChanges();
                    transaction.Commit();

                    return OperationResult.Failure("Lớp đã đủ người, không thể duyệt.");
                }

                enrollment.Status = "Approved";
                context.SaveChanges();

                approvedCount = context.Enrollments.Count(e =>
                    e.ClassId == trainingClass.Id && e.Status == "Approved");

                trainingClass.Status = approvedCount >= trainingClass.Capacity ? "Full" : "Open";
                context.SaveChanges();

                transaction.Commit();
                return OperationResult.Success("Đổi trạng thái sang Approved thành công.");
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

                if (wasApproved && trainingClass.Status != "Closed")
                {
                    int approvedCount = context.Enrollments.Count(e =>
                        e.ClassId == trainingClass.Id && e.Status == "Approved");

                    trainingClass.Status = approvedCount >= trainingClass.Capacity ? "Full" : "Open";
                    context.SaveChanges();
                }

                transaction.Commit();
                return OperationResult.Success("Đổi trạng thái sang Rejected thành công.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return OperationResult.Failure("Lỗi khi từ chối đăng ký: " + ex.Message);
            }
        }
    }
}