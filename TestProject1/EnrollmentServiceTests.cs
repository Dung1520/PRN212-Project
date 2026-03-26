using BusinessObjects;
using Repositories;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    public class EnrollmentServiceTests
    {
        [Fact]
        public void GetRegistrationList_ShouldReturnDataFromRepository()
        {
            var expected = new List<EnrollmentApprovalItem>
            {
                new EnrollmentApprovalItem
                {
                    EnrollmentId = 1,
                    StudentId = 101,
                    StudentCode = "ST001",
                    StudentName = "Nguyen Van A",
                    ClassId = 10,
                    ClassCode = "CL001",
                    EnrollmentStatus = "Pending"
                }
            };

            var fakeRepo = new FakeEnrollmentRepository
            {
                RegistrationListResult = expected
            };

            var service = new EnrollmentService(fakeRepo);

            var result = service.GetRegistrationList("Pending", "ST001");

            Assert.Same(expected, result);
            Assert.Equal("Pending", fakeRepo.LastStatusFilter);
            Assert.Equal("ST001", fakeRepo.LastKeyword);
        }

        [Fact]
        public void ApproveEnrollment_InvalidId_ShouldReturnFailure_AndNotCallRepository()
        {
            var fakeRepo = new FakeEnrollmentRepository();
            var service = new EnrollmentService(fakeRepo);

            var result = service.ApproveEnrollment(0);

            Assert.False(result.IsSuccess);
            Assert.Equal("EnrollmentId không hợp lệ.", result.Message);
            Assert.Equal(0, fakeRepo.ApproveCalledCount);
        }

        [Fact]
        public void ApproveEnrollment_ValidId_ShouldCallRepository_AndReturnRepositoryResult()
        {
            var expected = OperationResult.Success("Duyệt thành công.");
            var fakeRepo = new FakeEnrollmentRepository
            {
                ApproveResult = expected
            };

            var service = new EnrollmentService(fakeRepo);

            var result = service.ApproveEnrollment(5);

            Assert.Same(expected, result);
            Assert.Equal(1, fakeRepo.ApproveCalledCount);
            Assert.Equal(5, fakeRepo.LastApprovedEnrollmentId);
        }

        [Fact]
        public void RejectEnrollment_InvalidId_ShouldReturnFailure_AndNotCallRepository()
        {
            var fakeRepo = new FakeEnrollmentRepository();
            var service = new EnrollmentService(fakeRepo);

            var result = service.RejectEnrollment(-1);

            Assert.False(result.IsSuccess);
            Assert.Equal("EnrollmentId không hợp lệ.", result.Message);
            Assert.Equal(0, fakeRepo.RejectCalledCount);
        }

        [Fact]
        public void RejectEnrollment_ValidId_ShouldCallRepository_AndReturnRepositoryResult()
        {
            var expected = OperationResult.Success("Từ chối thành công.");
            var fakeRepo = new FakeEnrollmentRepository
            {
                RejectResult = expected
            };

            var service = new EnrollmentService(fakeRepo);

            var result = service.RejectEnrollment(7);

            Assert.Same(expected, result);
            Assert.Equal(1, fakeRepo.RejectCalledCount);
            Assert.Equal(7, fakeRepo.LastRejectedEnrollmentId);
        }

        private class FakeEnrollmentRepository : IEnrollmentRepository
        {
            public List<EnrollmentApprovalItem> RegistrationListResult { get; set; } = new();
            public OperationResult ApproveResult { get; set; } = OperationResult.Success("OK");
            public OperationResult RejectResult { get; set; } = OperationResult.Success("OK");

            public int ApproveCalledCount { get; private set; }
            public int RejectCalledCount { get; private set; }

            public int? LastApprovedEnrollmentId { get; private set; }
            public int? LastRejectedEnrollmentId { get; private set; }

            public string? LastStatusFilter { get; private set; }
            public string? LastKeyword { get; private set; }

            public List<EnrollmentApprovalItem> GetRegistrationList(string? statusFilter = null, string? keyword = null)
            {
                LastStatusFilter = statusFilter;
                LastKeyword = keyword;
                return RegistrationListResult;
            }

            public OperationResult ApproveEnrollment(int enrollmentId)
            {
                ApproveCalledCount++;
                LastApprovedEnrollmentId = enrollmentId;
                return ApproveResult;
            }

            public OperationResult RejectEnrollment(int enrollmentId)
            {
                RejectCalledCount++;
                LastRejectedEnrollmentId = enrollmentId;
                return RejectResult;
            }
        }
    }
}
