using BusinessObjects;
using DataAccess;

namespace Repositories
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly EnrollmentDao _dao = new EnrollmentDao();

        public List<EnrollmentApprovalItem> GetRegistrationList(string? statusFilter = null, string? keyword = null)
            => _dao.GetRegistrationList(statusFilter, keyword);

        public OperationResult ApproveEnrollment(int enrollmentId)
            => _dao.ApproveEnrollment(enrollmentId);

        public OperationResult RejectEnrollment(int enrollmentId)
            => _dao.RejectEnrollment(enrollmentId);
    }
}