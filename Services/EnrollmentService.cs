using BusinessObjects;
using Repositories;

namespace Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentRepository _repository = new EnrollmentRepository();

        public List<EnrollmentApprovalItem> GetRegistrationList(string? statusFilter = null, string? keyword = null)
            => _repository.GetRegistrationList(statusFilter, keyword);

        public OperationResult ApproveEnrollment(int enrollmentId)
        {
            if (enrollmentId <= 0)
                return OperationResult.Failure("EnrollmentId không hợp lệ.");

            return _repository.ApproveEnrollment(enrollmentId);
        }

        public OperationResult RejectEnrollment(int enrollmentId)
        {
            if (enrollmentId <= 0)
                return OperationResult.Failure("EnrollmentId không hợp lệ.");

            return _repository.RejectEnrollment(enrollmentId);
        }
    }
}