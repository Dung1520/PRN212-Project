using BusinessObjects;

namespace Repositories
{
    public interface IEnrollmentRepository
    {
        List<EnrollmentApprovalItem> GetRegistrationList(string? statusFilter = null, string? keyword = null);
        OperationResult ApproveEnrollment(int enrollmentId);
        OperationResult RejectEnrollment(int enrollmentId);
    }
}