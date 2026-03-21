using BusinessObjects;

namespace Services
{
    public interface IEnrollmentService
    {
        List<EnrollmentApprovalItem> GetRegistrationList(string? statusFilter = null, string? keyword = null);
        OperationResult ApproveEnrollment(int enrollmentId);
        OperationResult RejectEnrollment(int enrollmentId);
    }
}