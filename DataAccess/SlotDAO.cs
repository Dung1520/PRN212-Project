using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class SlotDAO
    {
        private readonly LctmsDbContext _context;

        public SlotDAO(LctmsDbContext context)
        {
            _context = context;
        }

        public List<Slot> GetAllSlots()
        {
            return _context.Slots.ToList();
        }

        public Slot? GetSlotById(int id)
        {
            return _context.Slots.FirstOrDefault(s => s.Id == id);
        }
    }
}
