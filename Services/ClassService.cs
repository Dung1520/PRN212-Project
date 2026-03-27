using BusinessObjects;
using DataAccess;
using Repositories;

namespace Services
{
    public class ClassService : IClassService
    {
        private readonly LctmsDbContext _context;
        private readonly IClassRepository _repo;

        public ClassService(IClassRepository repo, LctmsDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        public void AddClass(Class c, List<Schedule> schedules)
        {
            NormalizeClass(c);
            ValidateClass(c, schedules, isUpdate: false);

            using var tran = _context.Database.BeginTransaction();
            try
            {
                _context.Classes.Add(c);
                _context.SaveChanges();

                foreach (var s in schedules)
                {
                    s.ClassId = c.Id;
                    _context.Schedules.Add(s);
                }

                _context.SaveChanges();
                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public void UpdateClass(Class c, List<Schedule> schedules)
        {
            NormalizeClass(c);
            ValidateClass(c, schedules, isUpdate: true);

            using var tran = _context.Database.BeginTransaction();
            try
            {
                var existing = _context.Classes.FirstOrDefault(x => x.Id == c.Id);
                if (existing == null)
                    throw new Exception("Không tìm thấy lớp học.");

                existing.ClassCode = c.ClassCode;
                existing.CourseId = c.CourseId;
                existing.TeacherId = c.TeacherId;
                existing.StartDate = c.StartDate;
                existing.EndDate = c.EndDate;
                existing.Capacity = c.Capacity;
                existing.Status = c.Status;

                var oldSchedules = _context.Schedules.Where(x => x.ClassId == c.Id).ToList();
                _context.Schedules.RemoveRange(oldSchedules);
                _context.SaveChanges();

                foreach (var s in schedules)
                {
                    s.ClassId = c.Id;
                    _context.Schedules.Add(s);
                }

                _context.SaveChanges();
                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public void DeleteClass(int id)
        {
            throw new Exception("Theo yêu cầu hiện tại, module Class không dùng chức năng Delete.");
        }

        public List<Class> GetAllClasses()
        {
            var classes = _repo.GetAllClasses();

            foreach (var c in classes)
            {
                if (c.Status == "Closed") continue;

                int approvedCount = _context.Enrollments.Count(x => x.ClassId == c.Id && x.Status == "Approved");
                string actualStatus = approvedCount >= c.Capacity ? "Full" : "Open";

                var tracked = _context.Classes.FirstOrDefault(x => x.Id == c.Id);
                if (tracked != null && tracked.Status != actualStatus)
                {
                    tracked.Status = actualStatus;
                }
            }

            _context.SaveChanges();
            return _repo.GetAllClasses();
        }

        public Class? GetClassById(int id) => _repo.GetClassById(id);

        private void NormalizeClass(Class c)
        {
            if (c == null)
                throw new Exception("Dữ liệu lớp học không hợp lệ.");

            c.ClassCode = c.ClassCode?.Trim() ?? string.Empty;
            c.Status = string.IsNullOrWhiteSpace(c.Status) ? "Open" : c.Status.Trim();

            if (c.StartDate != default)
                c.StartDate = c.StartDate.Date;

            c.CreatedAt = c.CreatedAt == default ? DateTime.Now : c.CreatedAt;
        }

        private void ValidateClass(Class c, List<Schedule> schedules, bool isUpdate)
        {
            if (string.IsNullOrWhiteSpace(c.ClassCode))
                throw new Exception("Mã lớp không được để trống.");

            if (c.CourseId <= 0)
                throw new Exception("Bạn phải chọn khóa học.");

            if (c.Capacity <= 0)
                throw new Exception("Sức chứa phải lớn hơn 0.");

            if (c.StartDate == default)
                throw new Exception("Bạn phải chọn ngày bắt đầu.");

            if (c.StartDate.Date < DateTime.Today)
                throw new Exception("Ngày bắt đầu không được nhỏ hơn ngày hiện tại.");

            if (c.Status != "Open" && c.Status != "Full" && c.Status != "Closed")
                throw new Exception("Trạng thái lớp chỉ được là Open, Full hoặc Closed.");

            var course = _context.Courses.FirstOrDefault(x => x.Id == c.CourseId);
            if (course == null)
                throw new Exception("Khóa học không tồn tại.");

            if (course.Status != "Open" && c.Status == "Open")
                throw new Exception("Không được mở lớp khi khóa học đang Closed.");

            c.EndDate = c.StartDate.AddDays(course.DurationWeeks * 7 - 1);

            bool duplicateClassCode = _context.Classes.Any(x =>
                x.ClassCode.ToLower() == c.ClassCode.ToLower() &&
                (!isUpdate || x.Id != c.Id));

            if (duplicateClassCode)
                throw new Exception("Mã lớp đã tồn tại.");

            if (schedules == null || schedules.Count == 0)
                throw new Exception("Bạn phải nhập ít nhất một dòng lịch học.");

            if (c.Status == "Open" && !c.TeacherId.HasValue)
                throw new Exception("Không được chuyển lớp sang Open khi chưa phân công giáo viên.");

            if (c.TeacherId.HasValue)
            {
                var teacher = _context.Teachers.FirstOrDefault(x => x.Id == c.TeacherId.Value);
                if (teacher == null || !teacher.IsActive)
                    throw new Exception("Giáo viên không tồn tại hoặc đã bị khóa.");
            }

            foreach (var s in schedules)
            {
                if (s.DayOfWeek < 1 || s.DayOfWeek > 7)
                    throw new Exception("DayOfWeek chỉ được từ 1 đến 7.");

                if (s.SlotId <= 0)
                    throw new Exception("Bạn phải chọn slot.");

                bool slotExists = _context.Slots.Any(x => x.Id == s.SlotId);
                if (!slotExists)
                    throw new Exception("Slot không tồn tại.");

                if (string.IsNullOrWhiteSpace(s.RoomName))
                    throw new Exception("Tên phòng không được để trống.");

                s.RoomName = s.RoomName.Trim();
            }

            bool duplicateScheduleInSameClass = schedules
                .GroupBy(x => new { x.DayOfWeek, x.SlotId })
                .Any(g => g.Count() > 1);

            if (duplicateScheduleInSameClass)
                throw new Exception("Trong cùng một lớp không được trùng Day + Slot.");

            foreach (var s in schedules)
            {
                bool roomConflict =
                    (from sc in _context.Schedules
                     join cl in _context.Classes on sc.ClassId equals cl.Id
                     where sc.DayOfWeek == s.DayOfWeek
                           && sc.SlotId == s.SlotId
                           && sc.RoomName != null
                           && sc.RoomName.ToLower() == s.RoomName.ToLower()
                           && (!isUpdate || cl.Id != c.Id)
                           && cl.StartDate <= c.EndDate
                           && cl.EndDate >= c.StartDate
                           && cl.Status != "Closed"
                     select sc.Id).Any();

                if (roomConflict)
                    throw new Exception($"Phòng {s.RoomName} đã bị trùng lịch trong khoảng thời gian lớp này học.");

                if (c.TeacherId.HasValue)
                {
                    bool teacherConflict =
                        (from sc in _context.Schedules
                         join cl in _context.Classes on sc.ClassId equals cl.Id
                         where sc.DayOfWeek == s.DayOfWeek
                               && sc.SlotId == s.SlotId
                               && cl.TeacherId == c.TeacherId
                               && (!isUpdate || cl.Id != c.Id)
                               && cl.StartDate <= c.EndDate
                               && cl.EndDate >= c.StartDate
                               && cl.Status != "Closed"
                         select sc.Id).Any();

                    if (teacherConflict)
                        throw new Exception("Giáo viên đã bị trùng lịch trong khoảng thời gian lớp này học.");
                }
            }

            int approvedCount = _context.Enrollments.Count(x => x.ClassId == c.Id && x.Status == "Approved");
            if (c.Status == "Full" && approvedCount < c.Capacity)
                throw new Exception("Không được tự đặt trạng thái Full khi số Approved chưa đủ capacity.");

            if (c.Status == "Open" && schedules.Count == 0)
                throw new Exception("Không được chuyển lớp sang Open khi chưa có schedule.");
        }
    }
}
