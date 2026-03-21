using BusinessObjects;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class SlotRepository : ISlotRepository
    {
        private readonly SlotDAO _slotDAO;

        public SlotRepository(LctmsDbContext context)
        {
            _slotDAO = new SlotDAO(context);
        }

        public List<Slot> GetAllSlots() => _slotDAO.GetAllSlots();

        public Slot? GetSlotById(int id) => _slotDAO.GetSlotById(id);
    }
}
